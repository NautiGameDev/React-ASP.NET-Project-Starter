using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Identity.Client;

namespace BaseAccountSystem.Server.Tests.Helpers
{
    public static class WriteTestResults
    {
        private static readonly object _fileLock = new object();
        public static string FileName { get; set; } = "";

        public static void SetDbName(string dbName)
        {
            FileName = $"{dbName}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";
        }

        public static void AppendData(string data)
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                SetDbName("TestRun_Default");
            }

            string projectFolder = Directory.GetCurrentDirectory();
            string testsFolderPath = Path.Combine(projectFolder, "Tests");

            if (!Directory.Exists(testsFolderPath))
            {
                Directory.CreateDirectory(testsFolderPath);
            }

            string filePath = Path.Combine(testsFolderPath, FileName);

            lock (_fileLock)
            {
                File.AppendAllText(filePath, data + Environment.NewLine);
            }
        }

    }
}
