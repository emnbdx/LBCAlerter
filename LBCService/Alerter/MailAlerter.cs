namespace LBCService.Alerter
{
    using EMToolBox.Mail;
    using LBCMapping;
    using log4net;

    using Newtonsoft.Json;

    /// <summary>
    /// Send mail to alert
    /// </summary>
    public class MailAlerter : IAlerter
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MailAlerter));

        /// <summary>
        /// The mailer.
        /// </summary>
        private readonly EmMail mailer = new EmMail();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MailAlerter"/> class.
        /// </summary>
        /// <param name="to">
        /// The to.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="fullMode">
        /// The full mode.
        /// </param>
        public MailAlerter(string to, string subject, bool fullMode)
        {
            this.To = to;
            this.Subject = subject;
            this.FullMode = fullMode;
        }

        /// <summary>
        /// Gets the to.
        /// </summary>
        public string To { get; private set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether full mode.
        /// </summary>
        public bool FullMode { get; set; }

        /// <summary>
        /// The alert.
        /// </summary>
        /// <param name="ad">
        /// The ad.
        /// </param>
        public void Alert(Ad ad)
        {
            Log.Info("Ajout d'un mail à la file d'envoie [" + ad.Title + "]");
            var parameters = JsonConvert.SerializeObject(ad);

            this.mailer.Add(this.Subject, this.To, this.FullMode ? "LBC_AD_FULL" : "LBC_AD", parameters);
        }
    }
}