using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using NugetEmailDemo.DTO;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;

namespace NugetEmailDemo.Utility
{
    public class EmailHelper
    {
        #region '---- Members ----'

        private static Amazon.RegionEndpoint region = Amazon.RegionEndpoint.USWest2;

        #endregion

        #region '---- Methods ----'

        /// <summary>
        /// Method to send email without attachment.
        /// </summary>
        /// <param name="request">EmailSendRequestDTO object</param>
        /// <returns>true / false</returns>
        public static bool SendEmail(EmailSendRequestDTO request)
        {
            string provider = ConfigReader.Read(CommonEnums.ConfigKeys.EmailServiceProvider);

            bool isMailSent = false;

            switch (provider)
            {
                case "Sendgrid":
                    isMailSent = SendMailUsingSendgrid(request);
                    break;
                case "AWS":
                    isMailSent = SendMailUsingAWSSES(request);
                    break;
                case "Smtp":
                    isMailSent = SendMailUsingSMTP(request);
                    break;
                default:
                    isMailSent = SendMailUsingSMTP(request);
                    break;
            }

            return isMailSent;
        }

        /// <summary>
        /// Set smtp credentials for client.
        /// </summary>
        /// <returns>SmtpClient object</returns>
        private static SmtpClient SetSMTPClient()
        {
            SmtpClient client = new SmtpClient();
            client.EnableSsl = true; // TODO : implement configurable according to values from config file.
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Port = 587; // TODO : implement it as configurable.

            return client;
        }

        /// <summary>
        /// Method to set host for testing environment.
        /// </summary>
        /// <param name="client">SmtpClient object</param>
        private static void SetEmailServerForTesting(SmtpClient client)
        {
            client.Host = "localhost";
        }

        /// <summary>
        /// Method to set host for production environment.
        /// </summary>
        /// <param name="client">SmtpClient object</param>
        private static void SetEmailServerForProduction(SmtpClient client)
        {
            client.Host = "youremailserver.com";
        }

        /// <summary>
        /// Send mail using smtp server
        /// </summary>
        /// <param name="request">request object</param>
        /// <returns>true or false</returns>
        private static bool SendMailUsingSMTP(EmailSendRequestDTO request)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();

                mailMessage.From = !string.IsNullOrEmpty(request.FromAddress) ? new MailAddress(request.FromAddress) : null;

                if (!string.IsNullOrEmpty(request.ToAddresses))
                {
                    foreach (var toAddress in request.ToAddresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mailMessage.To.Add(toAddress);
                    }
                }

                if (!string.IsNullOrEmpty(request.CCAddresses))
                {
                    foreach (var ccAddress in request.CCAddresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mailMessage.CC.Add(ccAddress);
                    }
                }

                if (!string.IsNullOrEmpty(request.BCCAddresses))
                {
                    foreach (var bccAddress in request.BCCAddresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mailMessage.Bcc.Add(bccAddress);
                    }
                }

                mailMessage.Subject = request.Subject;
                mailMessage.Body = request.Body;
                mailMessage.IsBodyHtml = request.IsHtmlFormat;

                SmtpClient client = SetSMTPClient();
                SetEmailServerForTesting(client);
                //SetEmailServerForProduction(client); // TODO :  uncomment before production release.

                // Setting server certificates.
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
                System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };

                // Attachments
                if (!string.IsNullOrEmpty(request.AttachmentFile))
                {
                    foreach (var attachmentFile in request.AttachmentFile.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        using (FileStream fileStream = File.OpenRead(attachmentFile))
                        {
                            System.Net.Mail.Attachment messageAttachment = new System.Net.Mail.Attachment(attachmentFile);
                            mailMessage.Attachments.Add(messageAttachment);
                        }
                    }
                }

                client.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send mail using sendgrid server
        /// </summary>
        /// <param name="request">request object</param>
        /// <returns>true or false</returns>
        private static bool SendMailUsingSendgrid(EmailSendRequestDTO request)
        {
            try
            {
                string htmlContent = string.Empty;
                string plainTextContent = string.Empty;

                // Set apikey
                var apiKey = ConfigReader.Read(CommonEnums.ConfigKeys.SendgridApiKey);

                // Initialize sendgrid client.
                var client = new SendGridClient(apiKey);

                // From address
                var from = new EmailAddress(request.FromAddress, request.FromDisplayName);

                // Subject
                var subject = request.Subject;

                // Send mail to multiple recepients.
                var recepientList = new List<EmailAddress>();

                if (!string.IsNullOrEmpty(request.ToAddresses))
                {
                    foreach (var toAddress in request.ToAddresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        recepientList.Add(new EmailAddress(toAddress));
                    }
                }

                // If content in html format
                if (request.IsHtmlFormat)
                {
                    htmlContent = request.Body;
                }
                else
                {
                    plainTextContent = request.Body;
                }

                // Creat object to send mail message.
                var mailMessage = MailHelper.CreateSingleEmailToMultipleRecipients(from, recepientList, subject, plainTextContent, htmlContent);

                // add multiple cc.
                var ccList = new List<EmailAddress>();

                if (!string.IsNullOrEmpty(request.CCAddresses))
                {
                    foreach (var ccAddress in request.CCAddresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        ccList.Add(new EmailAddress(ccAddress));
                    }
                }

                // add multiple bcc.
                var bccList = new List<EmailAddress>();

                if (!string.IsNullOrEmpty(request.BCCAddresses))
                {
                    foreach (var bccAddress in request.BCCAddresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        bccList.Add(new EmailAddress(bccAddress));
                    }
                }

                // Attachments
                if (!string.IsNullOrEmpty(request.AttachmentFile))
                {
                    foreach (var attachmentFile in request.AttachmentFile.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        SendGrid.Helpers.Mail.Attachment messageAttachment = new SendGrid.Helpers.Mail.Attachment();
                        mailMessage.Attachments.Add(messageAttachment);
                    }
                }

                var response = client.SendEmailAsync(mailMessage).Result;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send mail using Amazon Web Services - Simple Emailing Service
        /// </summary>
        /// <param name="mailMessage">request object</param>
        /// <returns></returns>
        private static bool SendMailUsingAWSSES(EmailSendRequestDTO mailMessage)
        {
            // Email address must be verified.
            // Construct an object which can contain recipient's address.
            List<string> receipents = new List<string>();
            if (!string.IsNullOrEmpty(mailMessage.ToAddresses))
            {
                receipents = mailMessage.ToAddresses.Split(',').Select(e => e.Trim()).ToList();
            }

            Destination destination = new Destination();
            destination.ToAddresses = receipents;

            // Create the subject and body of the message.
            Amazon.SimpleEmail.Model.Content subjectContent = new Amazon.SimpleEmail.Model.Content(mailMessage.Subject);
            Amazon.SimpleEmail.Model.Content textBody = new Amazon.SimpleEmail.Model.Content(mailMessage.Body);

            // Initialize message content body.
            Body bodyContent = new Body();

            // This to handle the template content, if in html format.
            bodyContent.Html = textBody;

            // Create a message with the specified subject and body content.
            Message message = new Message(subjectContent, bodyContent);

            // Assemble the email.
            SendEmailRequest request = new SendEmailRequest(mailMessage.FromAddress, destination, message);

            // Set amazonAccessKeyId
            var amazonAccessKeyId = ConfigReader.Read(CommonEnums.ConfigKeys.AmazonAccessKeyID);

            // Set amazonSecretKey
            var amazonSecretKey = ConfigReader.Read(CommonEnums.ConfigKeys.AmazonSecretKey);

            // Instantiate an Amazon SES client, which will make the service call.
            AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient(
                amazonAccessKeyId, amazonSecretKey, region);

            // Send the email.
            try
            {
                client.SendEmail(request);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
