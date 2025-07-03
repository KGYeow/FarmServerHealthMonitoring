using FarmServerMonitoring.DTOs;
using FarmServerMonitoring.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;

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
                //InsertMailReportDataIntoDatabase(mail.EmailBody);

                Console.WriteLine("Report inserted successfully.");
                Console.WriteLine(new string('=', 100));
                i = i + 1;
            }

            Console.ReadKey();
        }

        // Extract the data from email report text body and insert data to database
        static void InsertMailReportDataIntoDatabase(string emailTextBody)
        {
            var reportId = InsertReportData(emailTextBody);

            InsertCollectionData(emailTextBody, reportId);
            InsertConnectionBrokerData(emailTextBody, reportId);
        }

        // Insert the report to the database
        static string InsertReportData(string emailTextBody)
        {
            // Create a new instance of the database context to interact with the database
            var context = new FarmServerMonitoringDB_TestContext();

            // Create a server health report
            var report = new ServerHealthReport()
            {
                Id = "",
                ReportName = "",
                ScriptStartTime = DateTime.Now,
                ScriptEndTime = DateTime.Now,
                CollectionName = "",
                CpuUsageAvg = Double.Parse("0".Replace("%", "")),
                MemoryUsageAvg = Double.Parse("0".Replace("%", "")),
                CdriveFreeSpaceAvg = Double.Parse("0".Replace("%", "")),
                DdriveFreeSpaceAvg = Double.Parse("0".Replace("%", "")),
                SessionsTotalAvg = Double.Parse("0"),
                SessionsActiveAvg = Double.Parse("0"),
                SessionsDiscAvg = Double.Parse("0"),
                SessionsNullAvg = Double.Parse("0"),
                SessionsTotalSum = Int32.Parse("0"),
                SessionsActiveSum = Int32.Parse("0"),
                SessionsDiscSum = Int32.Parse("0"),
                SessionsNullSum = Int32.Parse("0")
            };

            context.ServerHealthReport.Add(report);
            context.SaveChanges();

            return report.Id;
        }

        // Insert the collections to the database
        static void InsertCollectionData(string emailTextBody, string reportId)
        {
            // Create a new instance of the database context to interact with the database
            var context = new FarmServerMonitoringDB_TestContext();

            // Get all the collections from the email body
            var collections = new List<string>();

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
        static void InsertConnectionBrokerData(string emailTextBody, string reportId)
        {
            // Create a new instance of the database context to interact with the database
            var context = new FarmServerMonitoringDB_TestContext();

            // Get the connection brokers from the email body
            var connectionBrokers = new List<string>();

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
    }
}