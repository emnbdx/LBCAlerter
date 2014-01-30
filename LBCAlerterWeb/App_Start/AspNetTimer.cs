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

        public static void Start()
        {
            _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(60 * 1000));
        }

        private static void OnTimerElapsed(object sender)
        {
            _jobHost.DoWork(() => { /* What is it that you do around here */ });
        }
    }
}