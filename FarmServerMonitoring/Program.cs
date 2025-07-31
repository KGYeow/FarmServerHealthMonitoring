using FarmServerMonitoring.DTOs;
using FarmServerMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FarmServerMonitoring
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var docReportContentList = DocumentFile.ReadDocsFromLocalOneDrive();

            if (docReportContentList.Count != 0)
            {
                int i = 1;
                foreach (var docReport in docReportContentList)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Document #{i}");
                    Console.WriteLine($"Name    : {docReport.FileName}");
                    Console.WriteLine("");

                    // Insert the document report data into the database
                    InsertDocReportDataIntoDatabase(docReport.FileContent);

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(new string('-', 50));
                    Console.ResetColor();
                    i = i + 1;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[INFO] No document report found.");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[SUCCESS] Process completed.");
            Console.ResetColor();

            Console.ReadKey();
        }

        // Extract the data from the document report text body and insert data to database
        static void InsertDocReportDataIntoDatabase(string docText)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Starting database insertion process...");
            Console.ResetColor();

            // Create a new instance of the database context to interact with the database
            using (var context = new FarmServerMonitoringDB_TestContext())
            {
                // Create report data
                var reportId = InsertReportData(docText, context);

                // Check if the report has already existed in the database
                var isReportExist = context.ServerHealthReport.Where(a => a.Id == reportId).Any();
                if (isReportExist)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[INFO] Skipped: Duplicate report detected.");
                    Console.ResetColor();
                    return;
                }

                // Create collection table related data
                InsertCollectionTableData(docText, reportId, context);

                // Create connection broker data
                InsertConnectionBrokerData(docText, reportId, context);

                context.SaveChanges();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCCESS] Report inserted successfully.");
            Console.ResetColor();
        }

        // Insert the report to the database
        static string InsertReportData(string docText, FarmServerMonitoringDB_TestContext context)
        {
            try
            {
                // Split the document text content into string array
                var docTextArray = docText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => x.Trim()).Skip(1).ToArray();

                // Extract the report information
                var reportName = docTextArray.FirstOrDefault();
                var scriptStartTime = Regex.Match(docText, @"Script Start time:\s*(.+)").Groups[1].Value;
                var scriptEndTime = Regex.Match(docText, @"Script End time:\s*(.+)").Groups[1].Value;

                // Create a farm server health report data
                var report = new ServerHealthReport()
                {
                    Id = reportName.Replace("RDS Health Report", "").Replace(" ", "") + "_" + DateTime.Parse(scriptStartTime).ToString("ddMMyyyy_HHmmss"),
                    ReportName = reportName,
                    ScriptStartTime = DateTime.Parse(scriptStartTime),
                    ScriptEndTime = DateTime.Parse(scriptEndTime),
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

        // Insert the collection table to the database
        static void InsertCollectionTableData(string docText, string reportId, FarmServerMonitoringDB_TestContext context)
        {
            // Split the document text content into string array
            var lines = docText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => x.Trim()).Skip(1).ToArray();

            // Get the list of collection tables
            var collectionTables = SplitByCollectionTables(lines.ToList());

            // Action for each collection table
            foreach (var collectionTable in collectionTables)
            {
                // Get the collection name from table header
                var tableHeaderLine = collectionTable.First();
                var collectionName = Regex.Match(tableHeaderLine, @"Collection:\s*(\S+)", RegexOptions.IgnoreCase).Groups[1].Value;

                // Use the collection body to extract records, averages, and total sections
                var body = collectionTable.Skip(1).ToList();
                var (records, averages, totals) = ParseCollectionTableSection(body);

                // Create a collection table data
                var collection = new Collection
                {
                    ReportId = reportId,
                    Name = collectionName,
                    CpuUsageAvg = averages[0],
                    MemoryUsageAvg = averages[1],
                    CdriveFreeSpaceAvg = averages[2],
                    DdriveFreeSpaceAvg = averages[3],
                    SessionsTotalAvg = averages[6],
                    SessionsActiveAvg = averages[7],
                    SessionsDiscAvg = averages[8],
                    SessionsNullAvg = averages[9],
                    SessionsTotalSum = totals[1],
                    SessionsActiveSum = totals[2],
                    SessionsDiscSum = totals[3],
                    SessionsNullSum = totals[4]
                };
                context.Collection.Add(collection);
                context.SaveChanges();

                // Insert for the collection table's records data
                InsertCollectionRecordData(records, collection.Id, context);
            }
        }

        // Split the collection tables from the report text content
        static List<List<string>> SplitByCollectionTables(List<string> docTextArray)
        {
            var result = new List<List<string>>();
            List<string> currentTable = null;

            foreach (var line in docTextArray)
            {
                if (line.StartsWith("Collection:", StringComparison.OrdinalIgnoreCase))
                {
                    currentTable = new List<string>{ line };
                    result.Add(currentTable);
                }
                if (currentTable != null)
                    currentTable.Add(line);
            }
            return result;
        }

        public enum CollectionTableSection { Records, Averages, Totals }

        // Extract the records, averages, and totals sections from a collection table
        static (List<string> records, List<string> averages, List<string> totals) ParseCollectionTableSection(List<string> tableBody)
        {
            var records = new List<string>();
            var averages = new List<string>();
            var totals = new List<string>();

            bool isHeaderSkipped = false;
            var currentSection = CollectionTableSection.Records;

            foreach (var line in tableBody)
            {
                if (!isHeaderSkipped)
                {
                    if (line.Contains("Null"))  // Skip the header until "Null", the final header
                        isHeaderSkipped = true;
                    continue;
                }
                if (line.Contains("Average"))
                {
                    currentSection = CollectionTableSection.Averages;
                    continue;
                }
                if (line.Contains("Total"))
                {
                    currentSection = CollectionTableSection.Totals;
                    continue;
                }

                switch (currentSection)
                {
                    case CollectionTableSection.Records:
                        records.Add(line);
                        break;
                    case CollectionTableSection.Averages:
                        averages.Add(line);
                        break;
                    case CollectionTableSection.Totals:
                        totals.Add(line);
                        break;
                }
            }

            return (records, averages, totals);
        }

        // Insert the collection table's records to the database
        static void InsertCollectionRecordData(List<string> collectionRecords, int collectionId, FarmServerMonitoringDB_TestContext context)
        {
            // Initialize the number of rows and columns in a collection table
            var numRow = collectionRecords.Count(x => x.IndexOf("MYPEN", StringComparison.OrdinalIgnoreCase) >= 0);
            var numCol = 13; // 12 columns + 1 empty line

            try
            {
                // Loop through all the rows of a collection table
                for (int i = 0; i < numRow; i++)
                {
                    // Get one row of collection data
                    var collectionRow = collectionRecords.Skip(i * numCol).Take(numCol).ToList();

                    // Create a collection record
                    var collectionRecord = new CollectionRecord()
                    {
                        CollectionId = collectionId,
                        ServerName = collectionRow[0],
                        Enabled = collectionRow[1],
                        CpuUsage = collectionRow[2],
                        MemoryUsage = collectionRow[3],
                        CdriveFreeSpace = collectionRow[4],
                        DdriveFreeSpace = collectionRow[5],
                        Uptime = collectionRow[6],
                        PendingReboot = collectionRow[7],
                        SessionsTotal = collectionRow[8],
                        SessionsActive = collectionRow[9],
                        SessionsDisc = collectionRow[10],
                        SessionsNull = collectionRow[11]
                    };
                    context.CollectionRecord.Add(collectionRecord);
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Insert the connection brokers to the database
        static void InsertConnectionBrokerData(string docText, string reportId, FarmServerMonitoringDB_TestContext context)
        {
            // Get the connection brokers from the document report text
            var connectionBrokers = Regex.Match(docText, @"ConnectionBrokers:\s*(.+)").Groups[1].Value.Split(new[] { ", " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();

            foreach (var connectionBroker in connectionBrokers)
            {
                try
                {
                    // Check if the connection broker has already existed in the database
                    var isConnectionBrokerExist = context.ConnectionBroker.Where(a => a.Name == connectionBroker).Any();

                    // Create the connection broker if it doesn't exist in database
                    if (!isConnectionBrokerExist)
                        context.ConnectionBroker.Add(new ConnectionBroker() { Name = connectionBroker });

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
        static void MapConnectionBrokerToReport(string connectionBroker, string reportId, FarmServerMonitoringDB_TestContext context)
        {
            // Map the connection broker to the report ID
            var mapping = new ConnectionBrokerServerHealthMap()
            {
                ConnectionBrokerName = connectionBroker,
                ReportId = reportId
            };

            context.ConnectionBrokerServerHealthMap.Add(mapping);
        }
    }
}