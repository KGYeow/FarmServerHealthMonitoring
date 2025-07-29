using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FarmServerMonitoring.DTOs
{
    public class SharePointEmail
    {
        public string EmailFrom { get; set; }
        public string EmailSubject { get; set; }
        public DateTime EmailReceivedTime { get; set; }
        public string EmailBody { get; set; }

        private static readonly string sharePointSiteUrl = "https://yourcompany.sharepoint.com/sites/yoursite";
        private static readonly string documentLibrary = "Shared Documents/FarmHealthEmails";
        private static readonly string username = "yourdomain\\yourusername";
        private static readonly string password = "yourpassword";

        public static async Task<List<SharePointEmail>> ReadMailItemsFromSharePointAsync()
        {
            var listEmailDetails = new List<SharePointEmail>();

            try
            {
                var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(username.Split('\\')[1], password, username.Split('\\')[0])
                };

                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(sharePointSiteUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json;odata=verbose"));

                    string requestUrl = $"/_api/web/GetFolderByServerRelativeUrl('{documentLibrary}')/Files";

                    var response = await client.GetAsync(requestUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to get files: {response.StatusCode}");
                        return listEmailDetails;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(json);
                    var files = jsonDoc.RootElement
                        .GetProperty("d")
                        .GetProperty("results");

                    foreach (var file in files.EnumerateArray())
                    {
                        var fileName = file.GetProperty("Name").GetString();
                        var fileUrl = file.GetProperty("ServerRelativeUrl").GetString();

                        if (fileName.EndsWith(".txt") || fileName.EndsWith(".json"))
                        {
                            // Download file content
                            var fileResponse = await client.GetAsync(fileUrl);
                            var fileContent = await fileResponse.Content.ReadAsStringAsync();

                            // Parse the email (assume body contains From, Subject, Date, Body)
                            var email = ParseEmailTextFile(fileContent);
                            listEmailDetails.Add(email);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching from SharePoint: {ex.Message}");
            }

            return listEmailDetails;
        }

        private static SharePointEmail ParseEmailTextFile(string content)
        {
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var email = new SharePointEmail
            {
                EmailFrom = lines.Length > 0 ? lines[0].Replace("From:", "").Trim() : "unknown",
                EmailSubject = lines.Length > 1 ? lines[1].Replace("Subject:", "").Trim() : "No Subject",
                EmailReceivedTime = DateTime.TryParse(lines.Length > 2 ? lines[2].Replace("Date:", "").Trim() : "", out var dt) ? dt : DateTime.Now,
                EmailBody = string.Join("\n", lines, 4, lines.Length - 4)
            };

            return email;
        }
    }
}
