using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using EMToolBox.Job;
using HtmlAgilityPack;
using LBCMapping.Alerter;
using LBCMapping.Saver;
using log4net;

namespace LBCMapping
{
    public class Search : IJob
    {
        private static ILog log = LogManager.GetLogger(typeof(Search));

        private string m_criteria;
        private string m_keyword;
        private int m_refreshTime;
        private List<IAlerter> m_alerter = new List<IAlerter>();
        private ISaver m_saver;

        /// <summary>
        /// Extract keyword &q= param from url
        /// </summary>
        /// <returns>Keyword extracted and well formated</returns>
        private string ExtractKeyWordFromCriteria()
        {
            if (!String.IsNullOrEmpty(m_keyword))
                return m_keyword;

            //Cut path to get keyword
            int startIndex = m_criteria.IndexOf(HtmlParser.KEYWORD_URL_PARAM);
            int endIndex = m_criteria.IndexOf("&", startIndex + HtmlParser.KEYWORD_URL_PARAM.Length);
            if (endIndex == -1)
            {
                m_keyword = m_criteria.Substring(startIndex + HtmlParser.KEYWORD_URL_PARAM.Length, m_criteria.Length - (startIndex + HtmlParser.KEYWORD_URL_PARAM.Length));
            }
            else
            {
                m_keyword = m_criteria.Substring(startIndex + HtmlParser.KEYWORD_URL_PARAM.Length, endIndex - (startIndex + HtmlParser.KEYWORD_URL_PARAM.Length));
            }
            m_keyword = m_keyword.Replace("+", " ");

            return m_keyword;
        }

        public Search()
        { }

        public Search(string criteria, string keyword, int refreshTime)
        {
            m_criteria = HtmlParser.CleanCriteria(criteria);
            m_keyword = keyword;
            m_refreshTime = refreshTime;
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
            get { return ExtractKeyWordFromCriteria(); }
            set { m_keyword = value; }
        }

        [XmlElement("RefreshTime")]
        public int RefreshTime
        {
            get { return m_refreshTime; }
            set { m_refreshTime = value; }
        }

        public override string ToString()
        {
            return ExtractKeyWordFromCriteria();
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

                while (true)
                {
                    List<HtmlNode> links = HtmlParser.GetHtmlAd(m_criteria, currentPage);

                    if (links == null)
                        break; //Nothing return or empty page go out

                    bool elementFound = false;
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
                    }

                    if (elementFound)
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
            }
        }

        #region serialization

        private static string m_fileName = "criterion.xml";

        public static List<Search> Load()
        {
            if (!File.Exists(m_fileName))
                File.Create(m_fileName);

            var serializer = new XmlSerializer(typeof(List<Search>));
            using (StreamReader sr = new StreamReader(m_fileName))
                return (List<Search>)serializer.Deserialize(sr);
        }

        public static void Save(List<Search> search)
        {
            if (!File.Exists(m_fileName))
                File.Create(m_fileName);

            var serializer = new XmlSerializer(typeof(List<Search>));
            using (StreamWriter sw = new StreamWriter(m_fileName))
                serializer.Serialize(sw, search);
        }

        #endregion serialization
    }
}