namespace LBCService
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;

    using EMToolBox.Job;
    using EMToolBox.Mail;
    using EMToolBox.Services;
    using LBCAlerterWeb.Models;
    using LBCMapping;
    using LBCService.Alerter;
    using LBCService.Counter;
    using LBCService.Saver;
    using log4net;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// The notification service.
    /// </summary>
    public class NotificationService : SimpleService
    {
        /// <summary>
        /// The jobs.
        /// </summary>
        private static readonly Dictionary<int, RandomJobLauncher> Jobs = new Dictionary<int, RandomJobLauncher>();
        
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(NotificationService));

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="firstProcess">
        /// The first Process.
        /// </param>
        public override void Process(bool firstProcess)
        {
            base.Process(firstProcess);

            using (var db = new ApplicationDbContext())
            {
                if (firstProcess)
                {
                    foreach (var s in db.Searches.Where(s => s.Enabled && (s.MailAlert || s.MailRecap)))
                    {
                        this.CreateNewJob(s);
                    }
                }
                else
                {
                    foreach (var s in db.Searches)
                    {
                        this.SendMailRecap(s);

                        RandomJobLauncher jobLauncher;
                        Jobs.TryGetValue(s.ID, out jobLauncher);

                        if (jobLauncher == null)
                        {
                            this.CreateNewJob(s);
                        }
                        else
                        {
                            this.UpdateJob(s, jobLauncher);
                        }
                    }

                    this.StopDeletedJobs(db.Searches);
                }
            }

            GC.Collect();
        }

        /// <summary>
        /// Check if user is in admin or premium group
        /// </summary>
        /// <param name="user">Current search</param>
        /// <returns>True is user is admin or premium</returns>
        private bool IsPremium(IUser<string> user)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                    return userManager.IsInRole(user.Id, "admin") || userManager.IsInRole(user.Id, "premium");
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Send daily mail if not already send
        /// </summary>
        /// <param name="search">Current search</param>
        private void SendMailRecap(Search search)
        {
            if (!search.Enabled || !search.MailRecap
                || (search.LastRecap.HasValue && (search.LastRecap.Value.DayOfYear >= DateTime.Today.DayOfYear))
                || DateTime.Now.Hour != Convert.ToInt32(ConfigurationManager.AppSettings["Heure mail recap"]))
            {
                return;
            }

            var lastDay = DateTime.Now.AddDays(-1);

            var mail = new EMMail();
            var attempsCount = search.Attempts.Count(entry => entry.ProcessDate > lastDay);
            var todayAds = search.Ads.Where(entry => entry.Date > lastDay).OrderBy(entry => entry.Date);
            var parameters = new Dictionary<string, object>
                                 {
                                     {
                                         "{Title}",
                                         "Recap quotidien pour [" + search.KeyWord + "]"
                                     },
                                     {
                                         "{AdCount}",
                                         search.Ads.Count(entry => entry.Date > lastDay)
                                     },
                                     { "{AttemptCount}", attempsCount },
                                     {
                                         "{AttemptCadence}",
                                         24 * 60 / (attempsCount <= 0 ? 1 : attempsCount)
                                     },
                                     { "{Id}", search.ID },
                                     {
                                         "{AdId}",
                                         todayAds.FirstOrDefault() == null ? 0 : todayAds.FirstOrDefault().ID
                                     }
                                 };

            var ads = new StringBuilder();
            foreach (var formater in
                todayAds.Select(
                    ad =>
                    this.IsPremium(search.User)
                        ? new MailFormater(mail.GetPattern("LBC_RECAP_AD_FULL").CONTENT, ad)
                        : new MailFormater(mail.GetPattern("LBC_RECAP_AD").CONTENT, ad)))
            {
                ads.Append(formater.Formated);
            }

            parameters.Add("{Ads}", ads.ToString());

            mail.Add(
                "[LBCAlerter] - Recap quotidien pour [" + search.KeyWord + "]",
                search.User.UserName,
                this.IsPremium(search.User) ? "LBC_RECAP" : "LBC_RECAP_FULL",
                parameters);


            using (var db = new ApplicationDbContext())
            {
                var s = db.Searches.FirstOrDefault(entry => entry.ID == search.ID);
                if (s == null)
                {
                    throw new Exception("Recherche inexistante...");
                }

                s.LastRecap = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Create and launch new search job
        /// </summary>
        /// <param name="search">Current search</param>
        private void CreateNewJob(Search search)
        {
            if (!search.Enabled)
            {
                return;
            }

            var job = new SearchJob(search.Url, search.KeyWord, this.IsPremium(search.User), search.Ads.Count == 0)
                          {
                              FistTimeCount
                                  =
                                  5,
                              SaveMode
                                  = new EfSaver(search.ID)
                          };
            IAlerter alerter = new LogAlerter();
            job.Alerters.Add(alerter);
            if (search.MailAlert)
            {
                alerter = new MailAlerter(search.User.UserName, "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]", this.IsPremium(search.User));
                job.Alerters.Add(alerter);
            }

            ICounter counter = new EfCounter(search.ID);
            job.Counter.Add(counter);
            Log.Info("Add job [" + search.ID + "] to list");
            var launcher = new RandomJobLauncher(job, search.RefreshTime);
            Jobs.Add(search.ID, launcher);
            Log.Info("Launch job...");
            launcher.Start();
        }

        /// <summary>
        /// Update job interval and/or alerter
        /// </summary>
        /// <param name="search">Current search</param>
        /// <param name="jobLauncher">Launcher of this search</param>
        private void UpdateJob(Search search, JobLauncher jobLauncher)
        {
            if (jobLauncher.IntervalTime != search.RefreshTime)
            {
                jobLauncher.IntervalTime = search.RefreshTime;
            }

            if (!search.Enabled)
            {
                jobLauncher.Stop();
                Jobs.Remove(search.ID);
                return;
            }

            var job = jobLauncher.Job as SearchJob;
            if (job == null)
            {
                return;
            }

            var alerter = job.Alerters.OfType<MailAlerter>().Select(alert => alert).FirstOrDefault();

            if (alerter != null)
            {
                // delete
                if (!search.MailAlert) 
                {
                    job.Alerters.Remove(alerter);
                    jobLauncher.Stop();
                    Jobs.Remove(search.ID);
                }
                else
                {
                    // update
                    if (alerter.Subject != null && alerter.Subject != "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]") 
                    {
                        alerter.Subject = "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]";
                    }

                    if (search.User != null && job.Complete != this.IsPremium(search.User))
                    {
                        job.Complete = this.IsPremium(search.User);
                    }

                    if (search.User != null && alerter.FullMode != this.IsPremium(search.User))
                    {
                        alerter.FullMode = this.IsPremium(search.User);
                    }
                }
            }
            else
            {
                // add
                if (!search.MailAlert)
                {
                    return;
                }

                alerter = new MailAlerter(search.User.UserName, "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]", this.IsPremium(search.User));
                job.Alerters.Add(alerter);
            }
        }

        /// <summary>
        /// Delete job if deleted by user
        /// </summary>
        /// <param name="searches">Search in context</param>
        private void StopDeletedJobs(IQueryable<Search> searches)
        {
            if (Jobs.Count() <= searches.Count())
            {
                return;
            }

            var toStop = Jobs.Keys.Where(id => searches.FirstOrDefault(entry => entry.ID == id) == null);

            foreach (var id in toStop)
            {
                Jobs[id].Stop();
                Jobs.Remove(id);
            }
        }
    }
}
