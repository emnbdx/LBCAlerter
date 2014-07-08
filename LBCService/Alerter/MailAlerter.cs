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
        private bool m_fullMode;

        #endregion private

        #region public

        public MailAlerter(string to, string subject, bool fullMode)
        {
            m_to = to;
            m_subject = subject;
            m_fullMode = fullMode;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ad"></param>
        public void Alert(Ad ad)
        {
            log.Info("Ajout d'un mail à la file d'envoie [" + ad.Title + "]");

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("[Title]", ad.Title);
            parameters.Add("[Date]", ad.Date);
            parameters.Add("[Place]", ad.Place);
            parameters.Add("[Price]", ad.Price);
            parameters.Add("[AdUrl]", ad.AdUrl);
            parameters.Add("[PictureUrl]", ad.PictureUrl);

            if (m_fullMode)
            {
                parameters.Add("[Phone]", ad.Phone);
                parameters.Add("[AllowCommercial]", ad.AllowCommercial);
                parameters.Add("[Name]", ad.Name);
                parameters.Add("[ContactUrl]", ad.ContactUrl);
                parameters.Add("[Param]", ad.Param);
                parameters.Add("[Description]", ad.Description);

                mailer.Add(m_subject, m_to, "LBC_AD_FULL", parameters);
            }
            else
                mailer.Add(m_subject, m_to, "LBC_AD", parameters);
        }

        #endregion public
    }
}