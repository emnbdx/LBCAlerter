using System;
using EMToolBox;
using log4net;
using LBCMapping.Mailer;

namespace LBCMapping.Alerter
{
    /// <summary>
    /// Send mail to alert
    /// </summary>
    public class MailAlerter : IAlerter
    {
        #region private

        private static ILog log = LogManager.GetLogger(typeof(MailAlerter));

        private string m_to;
        private string m_subject;
        private int m_retry;

        #endregion private

        #region public

        public MailAlerter(string to, string subject, int retry)
        {
            m_to = to;
            m_subject = subject;
            m_retry = retry;
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

            int attemps = 0;
            while (attemps <= m_retry)
            {
                try
                {
                    if (attemps != 0)
                        log.Info("Nouvelle tentative #" + attemps + " après erreur");

                    new MailerSoapClient().SendSmtpMail(m_subject, body, m_to);
                    break;
                }
                catch (Exception e)
                {
                    log.Error(e.Message + "\r\n" + e.InnerException);
                }
                attemps++;
            }
        }

        #endregion public
    }
}