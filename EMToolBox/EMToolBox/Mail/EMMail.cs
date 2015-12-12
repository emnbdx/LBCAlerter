// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EMMail.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   Defines the EmMail type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EMToolBox.Mail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;

    using log4net;

    /// <summary>
    /// Manage mail, use Add to store mail to database, then send to consume queue
    /// </summary>
    public class EMMail
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly ILog log = LogManager.GetLogger(typeof(EMMail));

        /// <summary>
        /// The _context.
        /// </summary>
        private readonly EMMAILEntities context = new EMMAILEntities();

        /// <summary>
        /// The patterns.
        /// </summary>
        private List<PATTERN> patterns;

        /// <summary>
        /// The servers.
        /// </summary>
        private List<SERVER> servers;

        /// <summary>
        /// Gets the pattern.
        /// </summary>
        private IEnumerable<PATTERN> Pattern
        {
            get
            {
                return this.patterns ?? (this.patterns = this.context.PATTERN.ToList());
            }
        }

        /// <summary>
        /// Gets the server.
        /// </summary>
        private IEnumerable<SERVER> Server
        {
            get
            {
                return this.servers ?? (this.servers = this.context.SERVER.Where(entry => entry.ENABLE).ToList());
            }
        }

        /// <summary>
        /// The get pattern.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="PATTERN"/>.
        /// </returns>
        public PATTERN GetPattern(string name)
        {
            return this.Pattern.First(entry => entry.NAME == name);
        }

        /// <summary>
        /// Add Mail to queue
        /// </summary>
        /// <param name="subject">
        /// Mail subject
        /// </param>
        /// <param name="to">
        /// Receiver of mail
        /// </param>
        /// <param name="pattern">
        /// Mail pattern
        /// </param>
        /// <param name="data">
        /// Object use to replace tag in mail
        /// </param>
        public void Add(string subject, string to, string pattern, string data)
        {
            var pat = this.GetPattern(pattern);
            var formater = new MailFormatter(pat.CONTENT, data);

            this.context.QUEUE.Add(
                new QUEUE
                    {
                        PATTERN = pat,
                        SUBJECT = subject,
                        TO = to,
                        BODY = formater.Formatted,
                        CREATIONDATE = DateTime.Now
                    });

            this.context.SaveChanges();
        }

        /// <summary>
        /// Send all mail (not already send)
        /// </summary>
        public void Send()
        {
            var elements =
                this.context.QUEUE.Where(entry => entry.SEND == false).OrderBy(entry => entry.CREATIONDATE).ToList();
            if (elements.Count > 0)
            {
                this.log.Info("Envoi de " + elements.Count + " mails...");
            }

            foreach (var element in elements)
            {
                try
                {
                    this.Send(element);
                }
                catch
                {
                    this.log.Error("Erreur lors de l'envoi du mail [" + element.SUBJECT + "] pour [" + element.TO + "]");
                }
            }

            if (elements.Count > 0)
            {
                this.log.Info("OK");
            }
        }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <exception cref="Exception">
        /// If error occur when sending mail
        /// </exception>
        private void Send(QUEUE element)
        {
            foreach (var server in this.Server)
            {
                try
                {
                    var message = new MailMessage { From = new MailAddress(server.LOGIN) };

                    message.To.Add(element.TO);
                    message.Subject = element.SUBJECT;
                    message.IsBodyHtml = element.PATTERN.HTML;
                    message.Body = element.BODY;

                    using (var smtp = new SmtpClient(server.IP, server.PORT))
                    {
                        if (server.SSL)
                        {
                            smtp.EnableSsl = true;
                        }

                        if (!string.IsNullOrEmpty(server.LOGIN) && !string.IsNullOrEmpty(server.PASSWORD))
                        {
                            smtp.Credentials = new System.Net.NetworkCredential(server.LOGIN, server.PASSWORD);
                        }

                        smtp.Send(message);
                    }

                    element.SEND = true;
                    element.SENDDATE = DateTime.Now;

                    this.context.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new Exception("Error when sending email", e);
                }
            }
        }
    }
}
