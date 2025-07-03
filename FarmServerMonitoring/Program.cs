using FarmServerMonitoring.DTOs;
using FarmServerMonitoring.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmServerMonitoring
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mails = OutlookEmails.ReadMailItems("false");
            int i = 1;

            foreach (var mail in mails)
            {
                Console.WriteLine("Mail No: " + i);
                Console.WriteLine("Mail Subject: " + mail.EmailSubject);
                Console.WriteLine("Mail Received Time: " + mail.EmailReceivedTime);

                // Extract the data from email report to insert data records into the database
                InsertMailReportDataIntoDatabase(mail.EmailBody);

                Console.WriteLine(new string('=', 100));
                i = i + 1;
            }
            
            Console.WriteLine("Process End");
            Console.ReadKey();
        }

        // Extract the data from email report text body and insert data to database
        static void InsertMailReportDataIntoDatabase(string emailTextBody)
        {
            var bodyArray = emailTextBody.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var filteredBodyArray = bodyArray.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            // Create a new instance of the database context to interact with the database
            using (var context = new FarmServerMonitoringDBContext())
            {
                var reportId = InsertReportData(filteredBodyArray, context);

                // Check if the report has already existed in the database
                var isReportExist = context.ServerHealthReport.Where(a => a.Id == reportId).Any();
                if (isReportExist)
                {
                    Console.WriteLine("Skip duplicated report.");
                    return;
                }

                InsertCollectionData(filteredBodyArray, reportId, context);
                InsertConnectionBrokerData(filteredBodyArray, reportId, context);

                context.SaveChanges();
            }
            Console.WriteLine("Insert report successfully.");
        }

        // Insert the report to the database
        static string InsertReportData(string[] emailBodyArray, FarmServerMonitoringDBContext context)
        {
            // Create a server health report
            try
            {
                var report = new ServerHealthReport()
                {
                    Id = DateTime.Parse(emailBodyArray[255].Replace(": ", "")).ToString("MMddyyyyHHmmss"),
                    ReportName = emailBodyArray[0],
                    ScriptStartTime = DateTime.Parse(emailBodyArray[255].Replace(": ", "")),
                    ScriptEndTime = DateTime.Parse(emailBodyArray[257].Replace(": ", "")),
                    CollectionName = emailBodyArray[19].Split(' ')[1],
                    CpuUsageAvg = Double.Parse(emailBodyArray[202].Replace("%", "").Trim()),
                    MemoryUsageAvg = Double.Parse(emailBodyArray[203].Replace("%", "").Trim()),
                    CdriveFreeSpaceAvg = Double.Parse(emailBodyArray[204].Replace("%", "").Trim()),
                    DdriveFreeSpaceAvg = Double.Parse(emailBodyArray[205].Replace("%", "").Trim()),
                    SessionsTotalAvg = Double.Parse(emailBodyArray[206].Trim()),
                    SessionsActiveAvg = Double.Parse(emailBodyArray[207].Trim()),
                    SessionsDiscAvg = Double.Parse(emailBodyArray[208].Trim()),
                    SessionsNullAvg = Double.Parse(emailBodyArray[209].Trim()),
                    SessionsTotalSum = Int32.Parse(emailBodyArray[211].Trim()),
                    SessionsActiveSum = Int32.Parse(emailBodyArray[212].Trim()),
                    SessionsDiscSum = Int32.Parse(emailBodyArray[213].Trim()),
                    SessionsNullSum = Int32.Parse(emailBodyArray[214].Trim())
                };
                context.ServerHealthReport.Add(report);
                return report.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "";
        }

        // Insert the collections to the database
        static void InsertCollectionData(string[] emailBodyArray, string reportId, FarmServerMonitoringDBContext context)
        {
            // Get all the collections from the email body
            var collections = ExtractCollectionTableData(emailBodyArray);

            // Initialize the number of rows and columns in the collection table
            var numRow = 14;
            var numCol = 12;

            // Loop through all the rows of collection table
            for (int i = 0; i < numRow; i++)
            {
                // Get one row of collection data
                var collectionRow = collections.Skip(i * numCol).Take(numCol).ToList();

                try {
                    // Create a collection
                    var collection = new Collection()
                    {
                        ReportId = reportId,
                        ServerName = collectionRow[0],
                        Enabled = collectionRow[1],
                        CpuUsage = Double.Parse(collectionRow[2].Replace("%", "")),
                        MemoryUsage = Double.Parse(collectionRow[3].Replace("%", "")),
                        CdriveFreeSpace = Double.Parse(collectionRow[4].Replace("%", "")),
                        DdriveFreeSpace = Double.Parse(collectionRow[5].Replace("%", "")),
                        Uptime = collectionRow[6],
                        PendingReboot = collectionRow[7],
                        SessionsTotal = Int32.Parse(collectionRow[8]),
                        SessionsActive = Int32.Parse(collectionRow[9]),
                        SessionsDisc = Int32.Parse(collectionRow[10]),
                        SessionsNull = Int32.Parse(collectionRow[11])
                    };
                    context.Collection.Add(collection);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // Insert the connection brokers to the database
        static void InsertConnectionBrokerData(string[] emailBodyArray, string reportId, FarmServerMonitoringDBContext context)
        {
            // Get the connection brokers from the email body
            var connectionBrokers = emailBodyArray[3].Trim().Split(new[] { ", " }, StringSplitOptions.None);

            foreach (var connectionBroker in connectionBrokers)
            {
                try
                {
                    // Check if the connection broker has already existed in the database
                    var isConnectionBrokerExist = context.ConnectionBroker.Where(a => a.ServerName == connectionBroker).Any();

                    // Create the connection broker if it doesn't exist in database
                    if (!isConnectionBrokerExist)
                        context.ConnectionBroker.Add(new ConnectionBroker() { ServerName = connectionBroker });

                    // Map the connection broker to the report based on report ID
                    MapConnectionBrokerToReport(connectionBroker, reportId, context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // Map the connection broker to the report based on report ID
        static void MapConnectionBrokerToReport(string connectionBroker, string reportId, FarmServerMonitoringDBContext context)
        {
            // Map the connection broker to the report ID
            var mapping = new ConnectionBrokerServerHealthMap()
            {
                ConnectionBrokerName = connectionBroker,
                ReportId= reportId
            };

            context.ConnectionBrokerServerHealthMap.Add(mapping);
        }

        static List<string> ExtractCollectionTableData(string[] emailBodyArray)
        {
            var tableData = new List<string>();
            bool inTable = false;
            string[] headerKeywords = new[]
            {
                "Server Name", "Enabled", "CPU %", "Memory %", "C:\\ % Free", "D:\\ % Free", "Uptime", "Pending Reboot", "Sessions", "Total", "Active", "Disc", "Null"
            };

            foreach (var line in emailBodyArray)
            {
                if (line.Contains("Collection: PEN7VAV01"))
                {
                    inTable = true;
                    continue;
                }
                if (inTable && line.Contains("Average"))
                {
                    break;
                }
                if (inTable)
                {
                    if (string.IsNullOrWhiteSpace(line) || headerKeywords.Any(keyword => line.Contains(keyword)))
                        continue;

                    tableData.Add(line.Trim());
                }
            }
            return tableData;
        }
    }
}