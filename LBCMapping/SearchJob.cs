using System;
using System.Collections.Generic;
using System.Diagnostics;
using EMToolBox.Job;
using HtmlAgilityPack;
using LBCMapping.Alerter;
using LBCMapping.Saver;
using log4net;
using System.Xml.Serialization;

namespace LBCMapping
{
    public class SearchJob : IJob
    {
        private static ILog log = LogManager.GetLogger(typeof(SearchJob));

        private string m_criteria;
        private string m_keyword;
        private bool m_first;
        private List<IAlerter> m_alerter = new List<IAlerter>();
        private ISaver m_saver;

        private string KeyWordNeeded()
        {
            if (String.IsNullOrEmpty(m_keyword))
                m_keyword = HtmlParser.ExtractKeyWordFromCriteria(m_criteria);

            return m_keyword;
        }

        public SearchJob()
        { }

        public SearchJob(string criteria, string keyword, bool firstTime)
        {
            m_criteria = HtmlParser.CleanCriteria(criteria);
            m_keyword = keyword;
            m_first = firstTime;
        }

        [XmlElement("Criteria")]
        public string Criteria
        {
            get { return m_criteria; }
            set { m_criteria = value; }
        }

        [XmlElement("Keyword")]
        public string Keyword
        {
            get { return KeyWordNeeded(); }
            set { m_keyword = value; }
        }

        public override string ToString()
        {
            return KeyWordNeeded();
        }

        /// <summary>
        /// Set saver, this is use to store ad
        /// </summary>
        /// <param name="saver">Instance of saver to set</param>
        public void SetSaveMode(ISaver saveMode)
        {
            if (saveMode == null)
                m_saver = new FileSaver(Keyword);
            else
                m_saver = saveMode;
        }

        /// <summary>
        /// Add alerter for new ad notification
        /// </summary>
        /// <param name="alerter">Instance of alerter to add</param>
        public void AddAlerter(IAlerter alerter)
        {
            if (alerter != null)
                m_alerter.Add(alerter);
        }

        /// <summary>
        /// Call Alert() method on all alerter
        /// </summary>
        /// <param name="ad">New Ad</param>
        public void Alert(Ad ad)
        {
            foreach (IAlerter alerter in m_alerter)
            {
                alerter.Alert(ad);
            }
        }

        /// <summary>
        /// Search Ad in html page by page, store new Ad with ISaver.Store(), alert user with all IAlerter.Alert().
        /// </summary>
        public void Launch()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            try
            {
                int currentPage = 1;
                int currentAd = 0, adsLimit = 0;
                if (m_first)
                    adsLimit = 35;
                
                while (true)
                {
                    List<HtmlNode> links = HtmlParser.GetHtmlAd(m_criteria, currentPage);

                    if (links == null)
                        break; //Nothing return or empty page go out

                    bool elementFound = false, limitReached = false;
                    foreach (HtmlNode link in links)
                    {
                        Ad tmp = HtmlParser.GetAdInformation(link);

                        if (!m_saver.Store(tmp))
                        {
                            elementFound = true;
                            break; //Element already exist go out
                        }
                        else
                            Alert(tmp);

                        currentAd++;

                        if (adsLimit != 0 && currentAd >= adsLimit)
                        {
                            limitReached = true;
                            break;
                        }
                    }

                    if (elementFound || limitReached)
                        break;

                    currentPage++;
                }

                timer.Stop();
                log.Info("Terminée en " + timer.ElapsedMilliseconds + "ms");
            }
            catch (Exception e)
            {
                log.Error("Erreur lors de la récupération des annonces\r\n" + e);
            }
            finally
            {
                if (timer.IsRunning)
                    timer.Stop();

                if (m_first)
                    m_first = false;
            }
        }
    }
}