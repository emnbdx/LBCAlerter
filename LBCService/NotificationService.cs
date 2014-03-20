using EMToolBox.Job;
using EMToolBox.Services;
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
    class NotificationService : SimpleService
    {
        private static ILog log = LogManager.GetLogger(typeof(NotificationService));
        private static Dictionary<Int32, RandomJobLauncher> jobs = new Dictionary<Int32, RandomJobLauncher>();
        
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Process()
        {
            base.Process();

            using(ApplicationDbContext db = new ApplicationDbContext())
            {
                foreach (Search s in db.Searches)
                {
                    RandomJobLauncher jobLauncher;
                    jobs.TryGetValue(s.ID, out jobLauncher);

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
                            MailAlerter mailAlerter = new MailAlerter(s.User.UserName, "Nouvelle annonce");
                            job.AddAlerter(mailAlerter);
                        }
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["rssAlerter"]))
                        {
                            RSSAlerter rssAlerter = new RSSAlerter();
                            job.AddAlerter(rssAlerter);
                        }
                        log.Info("Add job [" + s.ID + "] to list");
                        RandomJobLauncher launcher = new RandomJobLauncher(job, 5);
                        jobs.Add(s.ID, launcher);
                        log.Info("Launch job...");
                        launcher.Start();
                    }
                }

                if (jobs.Count() > db.Searches.Count())
                {
                    foreach (Int32 id in jobs.Keys)
                    {
                        if (db.Searches.FirstOrDefault(entry => entry.ID == id) == null)
                        {
                            jobs[id].Stop();
                            jobs.Remove(id);                        
                        }
                    }
                }
            }
        }
    }
}
