using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;

namespace FarmServerMonitoring.DTOs
{
    public class DocumentFile
    {
        public string FileName { get; set; }
        public string FileContent { get; set; }

        // Retrieve the list of document file report text content from synced local OneDrive
        public static List<DocumentFile> ReadDocsFromLocalOneDrive()
        {
            var allDocs = new List<DocumentFile>();

            try
            {
                // Get all the htm files' path from the folder path
                string folderPath = @"C:\Users\4093094\Jabil\NurulNajihah AbdulRahim - FARM HEALTH DATA";
                var filePaths = Directory.GetFiles(folderPath, "*.htm");

                foreach (var filePath in filePaths)
                {
                    var doc = new HtmlDocument();
                    doc.Load(filePath);

                    // Extract document content in text string
                    allDocs.Add(new DocumentFile()
                    {
                        FileName = Path.GetFileName(filePath),
                        FileContent = doc.DocumentNode.InnerText,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n");
            }

            return allDocs;
        }
    }
}