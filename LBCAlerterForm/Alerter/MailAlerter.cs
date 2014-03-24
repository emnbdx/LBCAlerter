using System;
using EMToolBox;
using log4net;
using LBCMapping;

namespace LBCAlerterForm.Alerter
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

            string body = @"<html>
                                <body>
                                    <h1>" + ad.Title + @"</h1>
                                    <div>" +
                                        ad.Date + ", " + ad.Place + ", " + ad.Price +
                                    "<br><br>" +
                                    "<a href=\"" + ad.AdUrl + "\">" + (String.IsNullOrEmpty(ad.PictureUrl) ? "Pas d'image" : "<img src=\"" + ad.PictureUrl + "\">") + "</a>" +
                                    @"</div>
                                </body>
                            </html>";

            mailer.SendSmtpMail(m_subject, body, m_to);
        }

        #endregion public
    }
}