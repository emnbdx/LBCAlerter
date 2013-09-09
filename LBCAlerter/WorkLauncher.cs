using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using LBCAlerter.Properties;
using System.Net;
using LBCMapping;
using LBCMapping.Saver;
using EMToolBox.Log;
using LBCMapping.Alerter;
using LBCMapping.Criteria;

namespace LBCAlerter
{
    public class WorkLauncher
    {
        #region private

        private List<Searcher> m_searchs;
        private Thread m_thread;
        private bool m_started;

        /// <summary>
        /// Get all criterion and return Searcher for all 'region'
        /// </summary>
        /// <param name="criterion">Criterion for search</param>
        /// <returns>List of Searcher</returns>
        private List<Searcher> BuildSearchs(List<Criteria> criterion)
        {
            List<Searcher> searchs = new List<Searcher>();
            foreach (Criteria criteria in criterion)
            {
                if (!criteria.Active)
                    continue;
                
                IAlerter alerter;
                if(Settings.Default.AlertMode == "mail")
                    alerter = new MailAlerter(Settings.Default.MailFrom, criteria.Mail, 
                                            "Nouvelle annonce recherche [" + criteria.KeyWord + "]", 
                                            Settings.Default.SMTPServer, 
                                            (int)Settings.Default.SMTPPort, 
                                            Settings.Default.SMTPUser, 
                                            Settings.Default.SMTPPassword, 
                                            (int)Settings.Default.MailRetry);
                else if(Settings.Default.AlertMode == "rss")
                    alerter = new RSSAlerter();
                else
                    alerter = new LogAlerter();

                Searcher search;

                if (criteria.Location == null || criteria.Location == criteria.Region)
                    search = new Searcher(String.Format(Resources.Url, criteria.Category, criteria.Region, "", "{0}", criteria.KeyWord.Replace(' ', '+'), criteria.Extra));
                else if (criteria.Location == Constants.LocationNextRegion)
                    search = new Searcher(String.Format(Resources.Url, criteria.Category, criteria.Region, Constants.LocationNextRegion + "/", "{0}", criteria.KeyWord.Replace(' ', '+'), criteria.Extra));
                else if (criteria.Location == Constants.LocationCountry)
                    search = new Searcher(String.Format(Resources.Url, criteria.Category, criteria.Region, Constants.LocationCountry + "/", "{0}", criteria.KeyWord.Replace(' ', '+'), criteria.Extra));
                else
                    search = new Searcher(String.Format(Resources.Url, criteria.Category, criteria.Region, criteria.Location + "/", "{0}", criteria.KeyWord.Replace(' ', '+'), criteria.Extra));
                
                if(Settings.Default.SaveMode == "settings")
                    search.SetSaveMode(new SettingsSaver(criteria.KeyWord.Replace(' ', '+')));
                else
                    search.SetSaveMode(new FileSaver(criteria.KeyWord.Replace(' ', '+')));
                
                search.AddAlerter(alerter);

                searchs.Add(search);
            }

            return searchs;
        }

        /// <summary>
        /// This method is used in a thread
        /// </summary>
        private void Watch()
        {
            try
            {
                while (true)
                {
                    lock (m_searchs)
                    {
                        foreach (Searcher search in m_searchs)
                            search.Launch();
                    }

                    Random rand = new Random();
                    double bonus = rand.NextDouble() * (double)60;

                    Thread.Sleep(((int)bonus + ((int)Settings.Default.RefreshTime * 60)) * 1000);
                }
            }
            catch (ThreadInterruptedException)
            { /*Do nothing only leave while*/ }
        }

        #endregion private
        #region public

        /// <summary>
        /// Constructor, you need to call it with criterion list
        /// After you call start
        /// <param name="criterion">Criterions for search</param>
        /// </summary>
        public WorkLauncher(List<Criteria> criterion)
        {
            Log.Add("Détermination des urls de recherche", LogSeverity.Info);
            m_searchs = BuildSearchs(criterion);

            Log.Add("Création du thread", LogSeverity.Info);
            m_thread = new Thread(this.Watch);
        }

        /// <summary>
        /// Inform user and launch thread
        /// </summary>
        public void Start()
        {
            Log.Add("Lancement du thread", LogSeverity.Info);
            m_thread.Start();

            m_started = true;
        }

        /// <summary>
        /// Stop thread
        /// </summary>
        public void Stop()
        {
            Log.Add("Arret du thread", LogSeverity.Info);
            m_thread.Interrupt();

            m_started = false;
        }

        public bool Started
        {
            get { return m_started; }
            set { m_started = value; }
        }

        public void UpdateCriterion(List<Criteria> criterion)
        {
            lock (m_searchs)
            {
                Log.Add("Mise à jours des urls de recherche", LogSeverity.Info);
                m_searchs = BuildSearchs(criterion);
            }
        }

        #endregion public
    }
}