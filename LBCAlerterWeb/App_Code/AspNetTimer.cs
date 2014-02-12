using EMToolBox.Job;
using LBCAlerterWeb.Models;
using LBCMapping;
using LBCMapping.Alerter;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;

namespace LBCAlerterWeb
{
    public class AspNetTimer
    {
        private static readonly Timer _timer = new Timer(OnTimerElapsed);
        private static readonly JobHost _jobHost = new JobHost();
        private static Dictionary<String, RandomJobLauncher> jobs = new Dictionary<string, RandomJobLauncher>();

        private static ILog log = LogManager.GetLogger(typeof(AspNetTimer));

        public static void Start()
        {
            _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(10 * 1000));
        }

        private static void OnTimerElapsed(object sender)
        {
            _jobHost.DoWork(() => {
                ApplicationDbContext db = new ApplicationDbContext();
                foreach(Search s in db.Searches)
                {
                    RandomJobLauncher jobLauncher;
                    jobs.TryGetValue(s.User.UserName + "_" + s.Url, out jobLauncher);

                    if (jobLauncher == null)
                    {
                        SearchJob job = new SearchJob(s.Url, s.KeyWord);
                        job.SetSaveMode(new EFSaver(db, s));
                        if(Convert.ToBoolean(ConfigurationManager.AppSettings["logAlerter"]))
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
            });
        }

        public static void StopJob(Search s)
        {
            RandomJobLauncher jobLauncher;
            jobs.TryGetValue(s.User.UserName + "_" + s.Url, out jobLauncher);

            if (jobLauncher != null)
            {
                jobLauncher.Stop();
                jobs.Remove(s.User.UserName + "_" + s.Url);
            }
        }
    }
}