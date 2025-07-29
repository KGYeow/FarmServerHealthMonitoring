using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;

namespace FarmServerMonitoring.DTOs
{
    public class MailKitReader
    {
        public string EmailFrom { get; set; }
        public string EmailSubject { get; set; }
        public DateTime EmailReceivedTime { get; set; }
        public string EmailBody { get; set; }

        public static List<MailKitReader> ReadOutlookEmails()
        {
            var emails = new List<MimeMessage>();
            List<MailKitReader> listEmailDetails = new List<MailKitReader>();

            using (var client = new ImapClient())
            {
                // Connect to Office365 IMAP
                client.Connect("outlook.office365.com", 993, SecureSocketOptions.SslOnConnect);

                // Remove unnecessary authentication mechanisms
                //client.AuthenticationMechanisms.Remove("XOAUTH2");

                client.Authenticate("Kok_Yeow@jabil.com", "");

                // Open the Inbox
                var root = client.GetFolder(client.PersonalNamespaces[0]);
                var inbox = root.GetSubfolder("Farm Server Report");
                inbox.Open(FolderAccess.ReadWrite);

                //Console.WriteLine("Can open inbox folder?:" + inbox.CanOpen);

                // Search for unread messages with specific subject
                //var uids = inbox.Search(SearchQuery.NotSeen.And(SearchQuery.SubjectContains("PEN7-2 RDS Health report - Asia")));

                //int maxEmails = 5;
                //int start = inbox.Count - 1;
                //int end = Math.Max(0, inbox.Count - maxEmails);

                // Loop through all messages in the inbox
                for (int i = 0; i < inbox.Count; i++)
                {
                    var message = inbox.GetMessage(i);
                    emails.Add(message);

                    var emailDetails = new MailKitReader
                    {
                        EmailFrom = message.From.ToString(),
                        EmailSubject = message.Subject,
                        EmailReceivedTime = message.Date.DateTime,
                        EmailBody = message.TextBody ?? message.HtmlBody
                    };
                    listEmailDetails.Add(emailDetails);
                }

                //foreach (var uid in uids)
                //{
                //    var message = inbox.GetMessage(uid);
                //    emails.Add(message);

                //    var emailDetails = new MailKitReader
                //    {
                //        EmailFrom = message.From.ToString(),
                //        EmailSubject = message.Subject,
                //        EmailReceivedTime = message.Date.DateTime,
                //        EmailBody = message.TextBody ?? message.HtmlBody
                //    };
                //    listEmailDetails.Add(emailDetails);

                //    // Mark as read
                //    inbox.AddFlags(uid, MessageFlags.Seen, true);
                //}

                client.Disconnect(true);
            }

            return listEmailDetails;
        }
    }
}