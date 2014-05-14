using System;
using System.Collections.Generic;
using System.Diagnostics;
using EMToolBox.Job;
using HtmlAgilityPack;
using System.Xml.Serialization;
using log4net;

namespace LBCMapping
{
    public class SearchJob : IJob
    {
        private static ILog log = LogManager.GetLogger(typeof(SearchJob));

        private string m_criteria;
        private string m_keyword;
        private bool m_first;
        private int m_firstCount = 35; //Default value 1 page of ads
        private List<IAlerter> m_alerter = new List<IAlerter>();
        private List<ICounter> m_counter = new List<ICounter>();
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

        public int FistTimeCount
        {
            get { return m_firstCount; }
            set { m_firstCount = value; }
        }

        /// <summary>
        /// Set saver, this is use to store ad
        /// </summary>
        public ISaver SaveMode
        {
            get { return m_saver; }
            set { m_saver = value; }
        }

        /// <summary>
        /// Get or modify alerter
        /// </summary>
        public List<IAlerter> Alerters
        {
            get { return m_alerter; }
            set { m_alerter = value; }
        }
        
        /// <summary>
        /// Get or modify counter
        /// </summary>
        public List<ICounter> Counter
        {
            get { return m_counter; }
            set { m_counter = value; }
        }

        public override string ToString()
        {
            return KeyWordNeeded();
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
        /// Call Count() method for each counter
        /// </summary>
        public void Count()
        {
            foreach (ICounter counter in m_counter)
            {
                counter.Count();
            }
        }

        /// <summary>
        /// Call Result() method for each counter
        /// </summary>
        /// <param name="count">Count ads found</param>
        public void Result(int count)
        {
            foreach (ICounter counter in m_counter)
            {
                counter.Result(count);
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
                int currentAd = 0;
                
                while (true)
                {
                    List<HtmlNode> links = HtmlParser.GetHtmlAd(m_criteria, currentPage);

                    if (links == null)
                        break; //Nothing return or empty page go out

                    bool elementFound = false, limitReached = false;
                    foreach (HtmlNode link in links)
                    {
                        Ad tmp = HtmlParser.ExtractAdInformation(link);

                        if (!m_saver.Store(tmp))
                        {
                            elementFound = true;
                            break; //Element already exist go out
                        }
                        else
                            Alert(tmp);

                        currentAd++;
                        Count();

                        if (m_first && currentAd >= FistTimeCount)
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
                log.Debug("Terminée en " + timer.ElapsedMilliseconds + "ms");
                Result(currentAd);
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