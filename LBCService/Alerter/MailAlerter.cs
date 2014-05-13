using System;
using EMToolBox.Mail;
using log4net;
using LBCMapping;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace LBCService.Alerter
{
    /// <summary>
    /// Send mail to alert
    /// </summary>
    public class MailAlerter : IAlerter
    {
        #region private

        private static ILog log = LogManager.GetLogger(typeof(MailAlerter));
        private EMMail mailer = new EMMail();

        private string m_to;
        private string m_subject;

        #endregion private

        #region public

        public MailAlerter(string to, string subject)
        {
            m_to = to;
            m_subject = subject;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ad"></param>
        public void Alert(Ad ad)
        {
            log.Info("Envoie du mail [" + ad.Title + "]");

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("[Title]", ad.Title);
            parameters.Add("[Date]", ad.Date);
            parameters.Add("[Place]", ad.Place);
            parameters.Add("[Price]", ad.Price);
            parameters.Add("[AdUrl]", ad.AdUrl);
            parameters.Add("[PictureUrl]", ad.PictureUrl);
            mailer.Add(m_subject, m_to, "LBC_AD", parameters);
            //mailer.SendSmtpMail(m_subject, m_to, MailPattern.GetPattern(MailType.Ad), parameters);
        }

        #endregion public
    }
}