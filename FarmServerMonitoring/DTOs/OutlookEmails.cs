using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;

namespace FarmServerMonitoring.DTOs
{
    public class OutlookEmails
    {
        public string EmailFrom { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }

        public static List<OutlookEmails> ReadMailItems(string readStatus) 
        {
            Application outlookApplication = null;
            NameSpace outlookNamespace = null;
            MAPIFolder inboxFolder = null;

            Items mailItems = null;
            List<OutlookEmails> listEmailDetails = new List<OutlookEmails>();
            OutlookEmails emailDetails;

            try
            {
                outlookApplication = new Application();
                outlookNamespace = outlookApplication.GetNamespace("MAPI");
                //inboxFolder = outlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                inboxFolder = outlookNamespace.Folders[1].Folders["Farm Health Report"];

                if (readStatus == "true")
                    mailItems = inboxFolder.Items.Restrict("[Unread]=false");
                else if (readStatus == "false")
                    mailItems = inboxFolder.Items.Restrict("[Unread]=true");
                else
                    mailItems = inboxFolder.Items;

                foreach (MailItem item in mailItems)
                {
                    emailDetails = new OutlookEmails
                    {
                        EmailFrom = item.SenderEmailAddress,
                        EmailSubject = item.Subject,
                        EmailBody = item.Body
                    };
                    listEmailDetails.Add(emailDetails);

                    item.UnRead = false;
                    item.Save();
                    ReleaseComObject(item);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally 
            {
                ReleaseComObject(mailItems);
                ReleaseComObject(inboxFolder);
                ReleaseComObject(outlookNamespace);
                ReleaseComObject(outlookApplication);
            }
            return listEmailDetails;
        }

        public static void ReleaseComObject(object obj) 
        {
            if (obj != null) 
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
        }
    }
}