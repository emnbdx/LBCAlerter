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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCService
{
    class NotificationService : SimpleService
    {
        private static ILog log = LogManager.GetLogger(typeof(NotificationService));
        private static Dictionary<Int32, RandomJobLauncher> jobs = new Dictionary<Int32, RandomJobLauncher>();
        
        /// <summary>
        /// Check if user is in admin or premium group
        /// </summary>
        /// <param name="db">Current context</param>
        /// <param name="user">Current search</param>
        /// <returns>True is user is admin or premium</returns>
        private bool IsPremium(ApplicationDbContext db, ApplicationUser user)
        {
            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            return userManager.IsInRole(user.Id, "admin") || userManager.IsInRole(user.Id, "premium");
        }

        /// <summary>
        /// Send daily mail if not already send
        /// </summary>
        /// <param name="db">Current context</param>
        /// <param name="search">Current search</param>
        private void SendMailRecap(ApplicationDbContext db, Search search)
        {
            if (search.MailRecap
                && (!search.LastRecap.HasValue ||
                    search.LastRecap.HasValue && search.LastRecap.Value.DayOfYear < DateTime.Today.DayOfYear)
                && DateTime.Now.Hour == Convert.ToInt32(ConfigurationManager.AppSettings["Heure mail recap"]))
            {
                DateTime lastDay = DateTime.Now.AddDays(-1);

                EMMail mail = new EMMail();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("[Title]", "Recap quotidien pour [" + search.KeyWord + "]");
                parameters.Add("[AdCount]", search.Ads.Where(entry => entry.Date > lastDay).Count());
                int attempsCount = search.Attempts.Where(entry => entry.ProcessDate > lastDay).Count();
                parameters.Add("[AttemptCount]", attempsCount);
                parameters.Add("[AttemptCadence]", 24 * 60 / attempsCount);

                string ads = "";
                foreach (LBCAlerterWeb.Models.Ad ad in search.Ads.Where(entry => entry.Date > lastDay).OrderBy(entry => entry.Date))
                {
                    Formater f = null;
                    if (IsPremium(db, search.User))
                        f = new Formater(ad, mail.GetPattern("LBC_RECAP_AD_FULL").CONTENT);
                    else
                        f = new Formater(ad, mail.GetPattern("LBC_RECAP_AD").CONTENT);

                    ads += f.GetFormated();
                }

                parameters.Add("[Ads]", ads);

                if(IsPremium(db, search.User))
                    mail.Add("[LBCAlerter] - Recap quotidien pour [" + search.KeyWord + "]", search.User.UserName, "LBC_RECAP", parameters);
                else
                    mail.Add("[LBCAlerter] - Recap quotidien pour [" + search.KeyWord + "]", search.User.UserName, "LBC_RECAP_FULL", parameters);
                
                Search s = db.Searches.FirstOrDefault(entry => entry.ID == search.ID);
                if (s == null)
                    throw new Exception("Recherche inexistante...");
                s.LastRecap = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Create and launch new search job
        /// </summary>
        /// <param name="db">Current context</param>
        /// <param name="search">Current search</param>
        private void CreateNewJob(ApplicationDbContext db, Search search)
        {
            SearchJob job = new SearchJob(search.Url, search.KeyWord, IsPremium(db, search.User), search.Ads.Count == 0);
            job.FistTimeCount = 5;
            job.SaveMode = new EFSaver(search.ID);
            IAlerter alerter = new LogAlerter();
            job.Alerters.Add(alerter);
            if (search.MailAlert)
            {
                alerter = new MailAlerter(search.User.UserName, "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]", IsPremium(db, search.User));
                job.Alerters.Add(alerter);
            }
            ICounter counter = new EFCounter(search.ID);
            job.Counter.Add(counter);
            log.Info("Add job [" + search.ID + "] to list");
            RandomJobLauncher launcher = new RandomJobLauncher(job, search.RefreshTime);
            jobs.Add(search.ID, launcher);
            log.Info("Launch job...");
            launcher.Start();
        }

        /// <summary>
        /// Update job interval and/or alerter
        /// </summary>
        /// <param name="db">Current context</param>
        /// <param name="search">Current search</param>
        /// <param name="jobLauncher">Launcher of this search</param>
        private void UpdateJob(ApplicationDbContext db, Search search, JobLauncher jobLauncher)
        {
            if (jobLauncher.IntervalTime != search.RefreshTime)
                jobLauncher.IntervalTime = search.RefreshTime;

            SearchJob job = jobLauncher.Job as SearchJob;
            IAlerter alerter = null;
            foreach (IAlerter alert in job.Alerters)
            {
                if (alert is MailAlerter)
                {
                    alerter = alert; break;
                }
            }

            if (alerter != null && !search.MailAlert)
            {
                //Delete
                job.Alerters.Remove(alerter);
            }
            if (alerter == null && search.MailAlert)
            {
                //Add
                alerter = new MailAlerter(search.User.UserName, "Nouvelle annonce pour [" + search.KeyWord + "]", IsPremium(db, search.User));
                job.Alerters.Add(alerter);
            }
        }

        /// <summary>
        /// Delete job if deleted by user
        /// </summary>
        /// <param name="db">Context</param>
        private void StopDeletedJobs(ApplicationDbContext db)
        {
            if (jobs.Count() > db.Searches.Count())
            {
                List<Int32> toStop = new List<Int32>();
                foreach (Int32 id in jobs.Keys)
                {
                    if (db.Searches.FirstOrDefault(entry => entry.ID == id) == null)
                        toStop.Add(id);
                }

                foreach (Int32 id in toStop)
                {
                    jobs[id].Stop();
                    jobs.Remove(id);
                }
            }
        }
        
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Process()
        {
            base.Process();

            using(ApplicationDbContext db = new ApplicationDbContext())
            {
                //Add or edit job
                foreach (Search s in db.Searches)
                {
                    SendMailRecap(db, s);
                    
                    RandomJobLauncher jobLauncher;
                    jobs.TryGetValue(s.ID, out jobLauncher);

                    if (jobLauncher == null)
                        CreateNewJob(db, s);
                    else
                        UpdateJob(db, s, jobLauncher);
                }

                StopDeletedJobs(db);
            }
        }
    }
}
