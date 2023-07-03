﻿using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DFACore.Repository
{
    public class MessageService : IMessageService
    {

        public async Task SendEmailAsync(
            string toName,
            string toEmailAddress,
            string subject,
            string message,
            params Attachment[] attachments)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Apostille Appointment", "oca.authentication@dfa.gov.ph"));
            email.To.Add(new MailboxAddress(toName, toEmailAddress));
            email.Subject = subject;

            var body = new BodyBuilder
            {
                HtmlBody = message
            };

            foreach (var attachment in attachments)
            {
                body.Attachments.Add(attachment.FileName, attachment.MyStream, attachment.MyContentType);
                //using (var stream = await attachment.ContentToStreamAsync())
                //{
                //    body.Attachments.Add(attachment.FileName, stream);
                //    //body.Attachments.Add(attachment.FileName, CopyToArray(stream), );
                //}
            }

            //body.Attachments.Add("myFile.pdf", TestStream(), new ContentType("application", "pdf"));

            //body.Attachments.Add(@"D:\test\testing.txt");

            email.Body = body.ToMessageBody();
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp-relay.sendinblue.com", 587, false).ConfigureAwait(false);
                await client.AuthenticateAsync("oca.authentication@dfa.gov.ph", "tU52Yr8cBGjAROpb").ConfigureAwait(false);
                // End of provider specific settings

                await client.SendAsync(email).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }

        private System.IO.Stream TestStream()
        {
            System.IO.Stream fs = System.IO.File.OpenRead(@"D:\test\dfa.pdf");
            return fs;
        }

        public static byte[] CopyToArray(Stream input)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public void ConnectToPDF(string key)
        {


        }

        public bool IsAuthenticate()
        {
            return true;
        }



    }
}
