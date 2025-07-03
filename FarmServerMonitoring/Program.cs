using FarmServerMonitoring.DTOs;
using FarmServerMonitoring.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

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

                // Extract the data from email report to insert data records into the database
                InsertMailReportDataIntoDatabase(mail.EmailBody);

                //var bodyArray = mail.EmailBody.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                //var filteredBodyArray = bodyArray.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                //int j = 0;
                //foreach (var line in filteredBodyArray)
                //{
                //    Console.WriteLine("Index [" + j + "]: " + line);
                //    j++;
                //}

                Console.WriteLine("Report insertd successfully.");
                Console.WriteLine(new string('=', 100));
                i = i + 1;
            }
            
            Console.WriteLine("The End");
            Console.ReadKey();
        }

        // Extract the data from email report text body and insert data to database
        static void InsertMailReportDataIntoDatabase(string emailTextBody)
        {
            var bodyArray = emailTextBody.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var filteredBodyArray = bodyArray.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            var reportId = InsertReportData(filteredBodyArray);

            InsertCollectionData(filteredBodyArray, reportId);
            InsertConnectionBrokerData(filteredBodyArray, reportId);
        }

        // Insert the report to the database
        static string InsertReportData(string[] emailBodyArray)
        {
            
            // Create a new instance of the database context to interact with the database
            var context = new FarmServerMonitoringDB_TestContext();

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
                context.SaveChanges();

                return report.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "";

        }

        // Insert the collections to the database
        static void InsertCollectionData(string[] emailBodyArray, string reportId)
        {
            // Create a new instance of the database context to interact with the database
            var context = new FarmServerMonitoringDB_TestContext();

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
            }
        }

        // Insert the connection brokers to the database
        static void InsertConnectionBrokerData(string[] emailBodyArray, string reportId)
        {
            // Create a new instance of the database context to interact with the database
            var context = new FarmServerMonitoringDB_TestContext();

            // Get the connection brokers from the email body
            var connectionBrokers = emailBodyArray[3].Trim().Split(new[] { ", " }, StringSplitOptions.None);

            foreach (var connectionBroker in connectionBrokers)
            {
                // Check if the connection broker has already existed in the database
                var isConnectionBrokerExist = context.ConnectionBroker.Where(a => a.ServerName == connectionBroker).Any();

                // Create the connection broker if it doesn't exist in database
                if (!isConnectionBrokerExist)
                    context.ConnectionBroker.Add(new ConnectionBroker() { ServerName = connectionBroker });
                
                // Map the connection broker to the report based on report ID
                MapConnectionBrokerToReport(connectionBroker, reportId);
            }

            context.SaveChanges();
        }

        // Map the connection broker to the report based on report ID
        static void MapConnectionBrokerToReport(string connectionBroker, string reportId)
        {
            // Create a new instance of the database context to interact with the database
            var context = new FarmServerMonitoringDB_TestContext();

            // Map the connection broker to the report ID
            var mapping = new ConnectionBrokerServerHealthMap()
            {
                ConnectionBrokerName = connectionBroker,
                ReportId= reportId
            };

            context.ConnectionBrokerServerHealthMap.Add(mapping);
            context.SaveChanges();
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