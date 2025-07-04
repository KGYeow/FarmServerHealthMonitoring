using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmServerMonitoring.DTOs
{
    public class MsGraphEmailReader
    {
        public string EmailFrom { get; set; }
        public string EmailSubject { get; set; }
        public DateTime EmailReceivedTime { get; set; }
        public string EmailBody { get; set; }

        public static async Task<List<MsGraphEmailReader>> ReadMailItemsAsync(string readStatus)
        {
            var listEmailDetails = new List<MsGraphEmailReader>();

            try
            {
                var tenantId = "YOUR_TENANT_ID";
                var clientId = "YOUR_CLIENT_ID";
                var clientSecret = "YOUR_CLIENT_SECRET";
                var userEmail = "Kok_Yeow@jabil.com"; // Replace with the mailbox to read from

                var confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithTenantId(tenantId)
                    .WithClientSecret(clientSecret)
                    .Build();

                // 2) Create a GraphServiceClient with a delegate that injects the token
                var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(async request =>
                {
                    var authResult = await msalClient
                        .AcquireTokenForClient(scopes)
                        .ExecuteAsync();

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                }));

                // Get the folder ID for "Farm Health Report"
                var folders = await graphClient.Users[userEmail].MailFolders
                    .Request()
                    .GetAsync();

                var targetFolder = folders.CurrentPage.FirstOrDefault(f => f.DisplayName == "Farm Health Report");
                if (targetFolder == null)
                    throw new Exception("Folder 'Farm Health Report' not found.");

                // Build the filter
                string filter = "subject eq 'PEN7-2 RDS Health report - Asia'";
                if (readStatus == "true")
                    filter += " and isRead eq true";
                else if (readStatus == "false")
                    filter += " and isRead eq false";

                // Get messages
                var messages = await graphClient.Users[userEmail].MailFolders[targetFolder.Id].Messages
                    .Request()
                    .Filter(filter)
                    .Top(50)
                    .Select("sender,subject,receivedDateTime,body,isRead")
                    .OrderBy("receivedDateTime desc")
                    .GetAsync();

                foreach (var message in messages.CurrentPage)
                {
                    var emailDetails = new MsGraphEmailReader
                    {
                        EmailFrom = message.Sender?.EmailAddress?.Address,
                        EmailSubject = message.Subject,
                        EmailReceivedTime = message.ReceivedDateTime?.DateTime ?? DateTime.MinValue,
                        EmailBody = message.Body?.Content
                    };

                    listEmailDetails.Add(emailDetails);

                    // Mark as read if needed
                    if (message.IsRead == false)
                    {
                        await graphClient.Users[userEmail].Messages[message.Id]
                            .Request()
                            .UpdateAsync(new Message { IsRead = true });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return listEmailDetails;
        }
    }
}