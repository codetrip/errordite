﻿using System.Collections.Generic;
using System.Diagnostics;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Errordite.Core.Extensions;
using Errordite.Core.Configuration;

namespace Errordite.Core.Notifications.Sending
{
    public class AmazonSimpleEmailSender : ComponentBase, IEmailSender
    {
        private readonly EmailConfiguration _config;
        private readonly AmazonSimpleEmailServiceClient _emailClient;

        public AmazonSimpleEmailSender(EmailConfiguration config, AmazonSimpleEmailServiceClient emailClient)
        {
            _config = config;
            _emailClient = emailClient;
        }

        public void Send(Message message)
        {
            Stopwatch watch = Stopwatch.StartNew();

            Trace("Starting...");
            Trace("Sending email message to:={0}...", message.To);

            var to = new List<string>();
            var cc = new List<string>();
            var bcc = new List<string>();
            var replyTo = new List<string>();

            if (message.To.IsNotNullOrEmpty())
            {
                to.AddRange(message.To.Split(new[] {',', ';'}));
            }

            if (message.Cc.IsNotNullOrEmpty())
            {
                cc.AddRange(message.Cc.Split(new[] {',', ';'}));
            }

            if (message.Bcc.IsNotNullOrEmpty())
            {
                bcc.AddRange(message.Bcc.Split(new[] {',', ';'}));
            }

            if (message.ReplyTo.IsNotNullOrEmpty())
            {
                replyTo.AddRange(message.ReplyTo.Split(new[] {',', ';'}));
            }

            Trace("Sending...");

            _emailClient.SendEmail(new SendEmailRequest
            {
                Destination = new Destination
                {
                    ToAddresses = to,
                    CcAddresses = cc,
                    BccAddresses = bcc
                },
                Message = new Amazon.SimpleEmail.Model.Message
                {
                    Body = new Body(new Content(message.Body)),
                    Subject = new Content(message.Subject)
                },
                ReplyToAddresses = replyTo
            });

            watch.Stop();
            Trace("...Sent, Elapsed:={0} ({1}ms)", watch.Elapsed, watch.ElapsedMilliseconds);
        }
    }
}