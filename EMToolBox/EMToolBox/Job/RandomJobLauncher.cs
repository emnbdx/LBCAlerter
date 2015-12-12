using System;
using System.Threading;
using log4net;

namespace EMToolBox.Job
{
    public class RandomJobLauncher : JobLauncher
    {
        #region private

        private static ILog log = LogManager.GetLogger(typeof(JobLauncher));

        /// <summary>
        /// This method is used in a thread
        /// </summary>
        protected override void Watch()
        {
            try
            {
                while (true)
                {
                    lock (m_job)
                    {
                        m_job.Launch();
                    }

                    Random rand = new Random();
                    double bonus = rand.NextDouble() * (double)60;

                    Thread.Sleep(((int)bonus + (m_launchInterval * 60)) * 1000);
                }
            }
            catch (ThreadInterruptedException)
            { /*Do nothing only leave while*/ }
            catch (ThreadAbortException)
            { /*Do nothing only leave while*/ }
            catch (Exception e)
            {
                log.Error("Error on thread", e);
            }
        }

        #endregion private

        #region public

        /// <summary>
        /// RandomJobLauncher do search at refresh time + a random time (0 - 1 minute)
        /// <param name="job">Instance of IJob interface</param>
        /// <param name="refreshTime">Base time to launch the job in minute(s)</param>
        /// </summary>
        public RandomJobLauncher(IJob job, int launchInterval)
            : base(job, launchInterval)
        { }

        #endregion public
    }
}