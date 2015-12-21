// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationService.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   The notification service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Data.SqlClient;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

using LBCService.Alerter;
using LBCService.Counter;
using EMToolBox.Mail;
using EMToolBox.Services;
using LBCAlerterWeb.Models;
using LBCMapping;
using log4net;
using Newtonsoft.Json;
using LBCService.Saver;
using LBCService.Sql;

namespace LBCService
{   
    /// <summary>
    /// The notification service.
    /// </summary>
    public class NotificationService : SimpleService
    {      
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
                var enabledSearch = db.Database.SqlQuery<SearchSummary>("exec GetEnabledSearch").ToList();
                foreach (var s in enabledSearch)
                {
                    var lastAttempt = db.Database.SqlQuery<DateTime?>("exec GetLastAttempt @search_id", new SqlParameter("search_id", s.Id)).FirstOrDefault();

                    var runNeeded = s.MailAlert && (!lastAttempt.HasValue ||
                                                    lastAttempt.Value.AddMinutes(s.RefreshTime) < DateTime.Now);
                    if (runNeeded)
                    {
                        this.DoSearch(s);
                    }

                    var recapNeeded = s.MailRecap
                                      && (!s.LastRecap.HasValue || (s.LastRecap.Value < DateTime.Today))
                                      &&
                                      DateTime.Now.Hour ==
                                      Convert.ToInt32(ConfigurationManager.AppSettings["Heure mail recap"]);
                    if (recapNeeded)
                    {
                        this.SendMailRecap(s);
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
            var mail = new EMMail();
            var attempsCount = search.TodayAttempsCount;

            using (var db = new ApplicationDbContext())
            {
                var todayAds = db.Database.SqlQuery<Ad>("exec GetLastAdsFromSearch @search_id", new SqlParameter("search_id", search.Id));

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
        private void DoSearch(SearchSummary search)
        {
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
            alerter = new MailAlerter(search.UserName, "[LBCAlerter] - Nouvelle annonce pour [" + search.KeyWord + "]", search.IsPremiumUser);
            job.Alerters.Add(alerter);

            ICounter counter = new EfCounter(search.Id);
            job.Counter.Add(counter);
            Log.InfoFormat("Launch job {0}...", search.KeyWord);
            job.Launch();
        }
    }
}
