using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using CodeTrip.Core;
using CodeTrip.Core.Extensions;
using Errordite.Core.Configuration;

namespace Errordite.Core.Notifications.Sending
{
    public class SmtpMessageSender : ComponentBase, IMessageSender
    {
        private readonly EmailConfiguration _config;

        public SmtpMessageSender(EmailConfiguration config)
        {
            _config = config;
        }

        public void Send(Message message)
        {
            Stopwatch watch = Stopwatch.StartNew();

            Trace("Starting...");
            Trace("Sending email message to:={0}...", message.To);

            using (var m = new MailMessage())
            {
                m.From = new MailAddress(_config.FromAddress, _config.FromDisplayName);
                m.Body = message.Body;
                m.Subject = message.Subject;
                m.IsBodyHtml = true;

                if (message.To.IsNotNullOrEmpty())
                {
                    foreach (string mail in message.To.Split(new [] { ',', ';' }))
                    {
                        m.To.Add(new MailAddress(mail));
                    }
                }

                if (!message.Cc.IsNullOrEmpty())
                {
                    foreach (string mail in message.Cc.Split(new [] { ',', ';' }))
                    {
                        m.CC.Add(new MailAddress(mail));
                    }
                }

                if (!message.Bcc.IsNullOrEmpty())
                {
                    foreach (string mail in message.Bcc.Split(new [] { ',', ';' }))
                    {
                        m.Bcc.Add(new MailAddress(mail));
                    }
                }

                if (!message.ReplyTo.IsNullOrEmpty())
                {
                    foreach (string mail in message.ReplyTo.Split(new [] { ',', ';' }))
                    {
                        m.ReplyToList.Add(mail);
                    }
                }

                using (var client = new SmtpClient(_config.SmtpServer, _config.SmtpServerPort)
                    {
                        EnableSsl = _config.IsSmtpSecureConnection
                    })
                {
                    if (!String.IsNullOrEmpty(_config.SmtpServerUsername))
                    {
                        client.Credentials = new NetworkCredential(_config.SmtpServerUsername, _config.SmtpServerPassword);
                    }

                    Trace("Sending...");
                    client.Send(m);
                    watch.Stop();
                    Trace("...Sent, Elapsed:={0} ({1}ms)", watch.Elapsed, watch.ElapsedMilliseconds);
                }
            }
        }
    }
}