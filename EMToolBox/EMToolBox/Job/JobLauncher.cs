using System.Threading;
using log4net;
using System;

namespace EMToolBox.Job
{
    public class JobLauncher
    {
        #region private

        private static ILog log = LogManager.GetLogger(typeof(JobLauncher));

        protected IJob m_job;
        protected int m_launchInterval;
        protected Thread m_thread;
        protected bool m_started;

        /// <summary>
        /// This method is used in a thread
        /// </summary>
        protected virtual void Watch()
        {
            try
            {
                while (true)
                {
                    lock (m_job)
                    {
                        m_job.Launch();
                    }

                    Thread.Sleep(m_launchInterval * 60 * 1000);
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
        public JobLauncher(IJob job, int launchInterval)
        {
            m_job = job;
            m_launchInterval = launchInterval;

            log.Info("Création du thread");
            m_thread = new Thread(this.Watch);
        }

        /// <summary>
        /// Inform user and launch thread
        /// </summary>
        public void Start()
        {
            log.Info("Lancement du thread");
            if(m_thread.ThreadState == System.Threading.ThreadState.Aborted ||
                m_thread.ThreadState == System.Threading.ThreadState.Stopped)
                m_thread = new Thread(this.Watch);
            m_thread.Start();

            m_started = true;
        }

        /// <summary>
        /// Stop thread
        /// </summary>
        public void Stop()
        {
            log.Info("Arret du thread");
            m_thread.Interrupt();
            m_thread.Abort();

            m_started = false;
        }

        /// <summary>
        /// Interval time to launch job
        /// </summary>
        public int IntervalTime
        {
            get { return m_launchInterval; }
            set { m_launchInterval = value; }
        }

        /// <summary>
        /// IJob instance launched by this launcher
        /// </summary>
        public IJob Job
        {
            get { return m_job; }
        }

        /// <summary>
        /// Does the thread is started (not like running get ThreadState) 
        /// </summary>
        public bool Started
        {
            get { return m_started; }
        }

        /// <summary>
        /// Real thread current state
        /// </summary>
        public ThreadState ThreadState
        {
            get { return m_thread.ThreadState; }
        }

        #endregion public
    }
}