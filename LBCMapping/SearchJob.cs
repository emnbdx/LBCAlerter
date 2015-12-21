// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchJob.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   The search job.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LBCMapping
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Serialization;

    using EMToolBox.Job;
    using log4net;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The search job.
    /// </summary>
    public class SearchJob : IJob
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(SearchJob));

        /// <summary>
        /// The criteria.
        /// </summary>
        private string criteria;

        /// <summary>
        /// The keyword.
        /// </summary>
        private string keyword;

        /// <summary>
        /// The complete.
        /// </summary>
        private bool complete;

        /// <summary>
        /// The first.
        /// </summary>
        private bool first;

        /// <summary>
        /// The first count.
        /// </summary>
        private int firstCount = 35; // Default value 1 page of ads

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchJob"/> class.
        /// </summary>
        public SearchJob()
        {
            this.Counter = new List<ICounter>();
            this.Alerters = new List<IAlerter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchJob"/> class.
        /// </summary>
        /// <param name="criteria">
        /// The criteria.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <param name="complete">
        /// The complete.
        /// </param>
        /// <param name="firstTime">
        /// The first time.
        /// </param>
        public SearchJob(string criteria, string keyword, bool complete, bool firstTime)
        {
            this.Counter = new List<ICounter>();
            this.Alerters = new List<IAlerter>();
            this.criteria = HtmlParser.CleanCriteria(criteria);
            this.keyword = keyword;
            this.complete = complete;
            this.first = firstTime;
        }

        /// <summary>
        /// Gets or sets the criteria.
        /// </summary>
        [XmlElement("Criteria")]
        public string Criteria
        {
            get { return this.criteria; }
            set { this.criteria = value; }
        }

        /// <summary>
        /// Gets or sets the keyword.
        /// </summary>
        [XmlElement("Keyword")]
        public string Keyword
        {
            get
            {
                if (string.IsNullOrEmpty(this.keyword))
                {
                    this.keyword = HtmlParser.ExtractKeyWordFromCriteria(this.criteria);
                }

                return this.keyword;
            }

            set
            {
                this.keyword = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether complete.
        /// </summary>
        [XmlElement("Complete")]
        public bool Complete
        {
            get { return this.complete; }
            set { this.complete = value; }
        }

        /// <summary>
        /// Gets or sets the fist time count.
        /// </summary>
        public int FistTimeCount
        {
            get { return this.firstCount; }
            set { this.firstCount = value; }
        }

        /// <summary>
        /// Gets or sets the save mode.
        /// </summary>
        public ISaver SaveMode { get; set; }

        /// <summary>
        /// Gets or sets the alerters.
        /// </summary>
        public List<IAlerter> Alerters { get; set; }

        /// <summary>
        /// Gets or sets the counter.
        /// </summary>
        public List<ICounter> Counter { get; set; }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Keyword;
        }

        /// <summary>
        /// Call Alert() method on all alerter
        /// </summary>
        /// <param name="ad">New Ad</param>
        public void Alert(JObject ad)
        {
            foreach (var alerter in this.Alerters)
            {
                alerter.Alert(ad);
            }
        }

        /// <summary>
        /// Call Count() method for each counter
        /// </summary>
        public void Count()
        {
            foreach (var counter in this.Counter)
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
            foreach (var counter in this.Counter)
            {
                counter.Result(count);
            }
        }

        /// <summary>
        /// Search Ad in html page by page, store new Ad with ISaver.Store(), alert user with all IAlerter.Alert().
        /// </summary>
        public void Launch()
        {
            var timer = new Stopwatch();
            timer.Start();

            try
            {
                var currentPage = 1;
                var currentAd = 0;
                var elementFoundCount = 0;

                while (true)
                {
                    var links = HtmlParser.GetHtmlAd(this.criteria, currentPage);

                    if (links == null)
                    {
                        break; // Nothing return or empty page go out
                    }

                    var limitReached = false;
                    foreach (var tmp in links.Select(HtmlParser.ExtractAdInformation))
                    {
                        if (this.complete)
                        {
                            try
                            {
                                HtmlParser.ExtractAllAdInformation(tmp);
                            }
                            catch (Exception e)
                            {
                                Log.ErrorFormat("Erreur lors de la récupération des informations complètes\r\n{0}", e);
                            }
                        }

                        if (!this.SaveMode.Store(tmp))
                        {
                            elementFoundCount++;

                            if (elementFoundCount >= 5)
                            {
                                break; // Element already exist go out
                            }

                            continue;
                        }

                        this.Alert(tmp);

                        currentAd++;
                        this.Count();

                        if (!this.first || currentAd < this.FistTimeCount)
                        {
                            continue;
                        }

                        limitReached = true;
                        break;
                    }

                    if (elementFoundCount >= 5 || limitReached)
                    {
                        break;
                    }

                    currentPage++;
                }

                timer.Stop();
                Log.DebugFormat("Terminée en {0}ms", timer.ElapsedMilliseconds);
                this.Result(currentAd);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Erreur lors de la récupération des annonces pour [{0}]\r\n{1}", this.criteria, e);
            }
            finally
            {
                if (timer.IsRunning)
                {
                    timer.Stop();
                }

                if (this.first)
                {
                    this.first = false;
                }
            }
        }
    }
}