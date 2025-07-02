using FarmServerMonitoring.DTOs;
using FarmServerMonitoring.Models;
using Microsoft.Office.Interop.Outlook;
using System;

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
                // Extract the data from email report to insert data records into the database
                InsertMailReportDataIntoDatabase(mail.EmailBody);

                Console.WriteLine("Mail No: " + i);
                Console.WriteLine("Mail Subject: " + mail.EmailSubject);
                Console.WriteLine(new string('=', 100));
                i = i + 1;
            }

            Console.ReadKey();
        }

        // Extract the data from email report text body and insert data to database
        static void InsertMailReportDataIntoDatabase(string emailTextBody)
        {
            // Create a new instance of the database context to interact with the database
            var context = new FarmServerMonitoringDB_TestContext();

            // Create a server health report
            var report = new ServerHealthReport();
            var collection = new Collection();

            context.ServerHealthReport.Add(report);
            context.Collection.Add(collection);
            context.SaveChanges();

            Console.WriteLine("Report inserted successfully.");
        }
    }
}