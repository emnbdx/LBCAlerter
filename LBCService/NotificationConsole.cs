using EMToolBox.Job;
using LBCAlerterWeb.Models;
using LBCMapping;
using LBCMapping.Alerter;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCService
{
    public class NotificationConsole
    {
        private static ILog log = LogManager.GetLogger(typeof(NotificationService));
        private static Dictionary<String, RandomJobLauncher> jobs = new Dictionary<string, RandomJobLauncher>();

        public void Process()
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                foreach (Search s in db.Searches)
                {
                    RandomJobLauncher jobLauncher;
                    jobs.TryGetValue(s.User.UserName + "_" + s.Url, out jobLauncher);

                    if (jobLauncher == null)
                    {
                        SearchJob job = new SearchJob(s.Url, s.KeyWord, s.Ads.Count == 0);
                        job.SetSaveMode(new EFSaver(s.ID));
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["logAlerter"]))
                        {
                            LogAlerter logAlerter = new LogAlerter();
                            job.AddAlerter(logAlerter);
                        }
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["mailAlerter"]))
                        {
                            MailAlerter mailAlerter = new MailAlerter(s.User.UserName, "Nouvelle annonce", 5);
                            job.AddAlerter(mailAlerter);
                        }
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["rssAlerter"]))
                        {
                            RSSAlerter rssAlerter = new RSSAlerter();
                            job.AddAlerter(rssAlerter);
                        }
                        log.Info("Add job [" + s.User.UserName + "_" + s.Url + "] to list");
                        RandomJobLauncher launcher = new RandomJobLauncher(job, 5);
                        jobs.Add(s.User.UserName + "_" + s.Url, launcher);
                        log.Info("Launch job...");
                        launcher.Start();
                    }
                }
            }
        }
    }
}
