using EMToolBox.Job;
using LBCAlerterWeb.Models;
using LBCMapping;
using LBCMapping.Alerter;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace LBCAlerterWeb
{
    public class AspNetTimer
    {
        private static readonly Timer _timer = new Timer(OnTimerElapsed);
        private static readonly JobHost _jobHost = new JobHost();
        private static Dictionary<String, SearchJob> jobs = new Dictionary<string, SearchJob>();

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
                    SearchJob job;
                    jobs.TryGetValue(s.User.UserName + "_" + s.Url, out job);

                    if(job == null)
                    {
                        job = new SearchJob(s.Url, s.KeyWord, 30);
                        job.SetSaveMode(new EFSaver(s));
                        LogAlerter alerter = new LogAlerter();
                        job.AddAlerter(alerter);
                        log.Info("Add job [" + s.User.UserName + "_" + s.Url + "] to list");
                        jobs.Add(s.User.UserName + "_" + s.Url, job);
                        log.Info("Launch job...");
                        RandomJobLauncher launcher = new RandomJobLauncher(job, job.RefreshTime);
                        launcher.Start();
                    }
                }
            });
        }
    }
}