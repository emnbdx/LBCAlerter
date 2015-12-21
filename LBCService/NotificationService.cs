// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationService.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   The notification service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using LBCService.Sql;

namespace LBCService
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;

    using EMToolBox.Job;
    using EMToolBox.Mail;
    using EMToolBox.Services;
    using LBCAlerterWeb.Models;
    using LBCMapping;
    using Alerter;
    using Counter;
    using Saver;
    using log4net;

    using Newtonsoft.Json;

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
                    var enabledSearch = db.Database.SqlQuery<SearchSummary>("exec GetEnabledSearch");
                    foreach (var s in enabledSearch)
                    {
                        CreateNewJob(s);
                    }
                }
                else
                {
                    var allSearch = db.Database.SqlQuery<SearchSummary>("exec GetAllSearch");
                    foreach (var s in allSearch)
                    {
                        SendMailRecap(s);

                        RandomJobLauncher jobLauncher;
                        Jobs.TryGetValue(s.Id, out jobLauncher);

                        if (jobLauncher == null)
                        {
                            CreateNewJob(s);
                        }
                        else
                        {
                            UpdateJob(s, jobLauncher);
                        }
                    }

                    if (Jobs.Count > allSearch.Count())
                    {
                        foreach (var s in allSearch)
                        {
                            StopDeletedJobs(s);
                        }
                    }
                }
            }

            GC.Collect();
        }

        /// <summary>
        /// Send daily mail if not already send
        /// </summary>
        /// <param name="search">Current search</param>
        private void SendMailRecap(SearchSummary search)
        {
            if (!search.Enabled || !search.MailRecap
                || (search.LastRecap.HasValue && (search.LastRecap.Value >= DateTime.Today))
                || DateTime.Now.Hour != Convert.ToInt32(ConfigurationManager.AppSettings["Heure mail recap"]))
            {
                return;
            }
            
            var mail = new EMMail();
            var attempsCount = search.TodayAttempsCount;

            using (var db = new ApplicationDbContext())
            {
                var todayAds = db.Database.SqlQuery<Ad>("exec GetLastAdsFromSearch", search.Id);

                var mailPattern = mail.GetPattern("LBC_RECAP_AD" + (search.IsPremiumUser ? "_FULL" : string.Empty)).CONTENT;
                var ads = new StringBuilder();

                var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

                foreach (
                    var formater in
                        todayAds.Select(
                            todayAd =>
                            new MailFormatter(
                                mailPattern,
                                JsonConvert.SerializeObject(todayAd, Formatting.Indented, settings))))
                {
                    ads.Append(formater.Formatted);
                }

                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using (var writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("Title");
                    writer.WriteValue("Recap quotidien pour [" + search.KeyWord + "]");
                    writer.WritePropertyName("AdCount");
                    writer.WriteValue(todayAds.Count());
                    writer.WritePropertyName("AttemptCount");
                    writer.WriteValue(attempsCount);
                    writer.WritePropertyName("AttemptCadence");
                    writer.WriteValue(24 * 60 / (attempsCount <= 0 ? 1 : attempsCount));
                    writer.WritePropertyName("Id");
                    writer.WriteValue(search.Id);
                    writer.WritePropertyName("AdId");
                    writer.WriteValue(todayAds.FirstOrDefault());
                    writer.WritePropertyName("Ads");
                    writer.WriteValue(ads.ToString());

                    writer.WriteEndObject();
                }

                mail.Add(
                    "[LBCAlerter] - Recap quotidien pour [" + search.KeyWord + "]",
                    search.UserName,
                    search.IsPremiumUser ? "LBC_RECAP" : "LBC_RECAP_FULL",
                    sb.ToString());

                db.Database.ExecuteSqlCommand("UPDATE Search set LastRecap = '" + DateTime.Now + "' WHERE Id = " + search.Id);
            }
        }

        /// <summary>
        /// Create and launch new search job
        /// </summary>
        /// <param name="search">Current search</param>
        private void CreateNewJob(SearchSummary search)
        {
            if (!search.Enabled)
            {
                return;
            }

            var job = new SearchJob(search.Url, search.KeyWord, search.IsPremiumUser, search.AdsCount == 0)
                          {
                              FistTimeCount
                                  =
                                  5,
                              SaveMode
                                  = new EfSaver(search.Id)
                          };
            IAlerter alerter = new LogAlerter();
            job.Alerters.Add(alerter);
            if (search.MailAlert)
            {
                alerter = new MailAlerter(search.UserName, "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]", search.IsPremiumUser);
                job.Alerters.Add(alerter);
            }

            ICounter counter = new EfCounter(search.Id);
            job.Counter.Add(counter);
            Log.Info("Add job [" + search.Id + "] to list");
            var launcher = new RandomJobLauncher(job, search.RefreshTime);
            Jobs.Add(search.Id, launcher);
            Log.Info("Launch job...");
            launcher.Start();
        }

        /// <summary>
        /// Update job interval and/or alerter
        /// </summary>
        /// <param name="search">Current search</param>
        /// <param name="jobLauncher">Launcher of this search</param>
        private void UpdateJob(SearchSummary search, JobLauncher jobLauncher)
        {
            if (jobLauncher.IntervalTime != search.RefreshTime)
            {
                jobLauncher.IntervalTime = search.RefreshTime;
            }

            if (!search.Enabled)
            {
                jobLauncher.Stop();
                Jobs.Remove(search.Id);
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
                    Jobs.Remove(search.Id);
                }
                else
                {
                    // update
                    if (alerter.Subject != null && alerter.Subject != "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]") 
                    {
                        alerter.Subject = "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]";
                    }

                    if (search.UserName != null && job.Complete != search.IsPremiumUser)
                    {
                        job.Complete = search.IsPremiumUser;
                    }

                    if (search.UserName != null && alerter.FullMode != search.IsPremiumUser)
                    {
                        alerter.FullMode = search.IsPremiumUser;
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

                alerter = new MailAlerter(search.UserName, "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]", search.IsPremiumUser);
                job.Alerters.Add(alerter);
            }
        }

        /// <summary>
        /// Delete job if deleted by user
        /// </summary>
        /// <param name="search">Search in context</param>
        private void StopDeletedJobs(SearchSummary search)
        {
            var toStop = Jobs.Keys.Where(key => search.Id == key).ToList();

            foreach (var id in toStop)
            {
                Jobs[id].Stop();
                Jobs.Remove(id);
            }
        }
    }
}
