using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossPlatformManifestMaker
{
    public static class FileUtils
    {

        private static string LogFilePath;
        
        public static string[] GetAllLinesInFile(string path)
        {
            string[] allLinesInFile = File.ReadAllLines(path);

            return allLinesInFile;
        }

        public static void WriteStringToFile(string stringToWrite, string filePathWithName)
        {
            // Check if file already exists. If yes, delete it.     
            if (Exists(filePathWithName))
            {
                File.Delete(filePathWithName);
            }

            using StreamWriter sw = File.CreateText(filePathWithName);
            sw.Write(stringToWrite);
            sw.Close();
        }
        
        public static List<FileInfo> GetAllFilesInFolderWith(string path, string stringToInclude, string notIncluding = "-1", string exceptionToNotIncludeRule = "-1")
        {
            
            DirectoryInfo d = new DirectoryInfo(path);

            FileInfo[] files;
            try
            {
                files = d.GetFiles(stringToInclude); //Getting Text files
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            
            List<FileInfo> listOfFiles = files.Where(file => (!file.Name.ToLower().Contains(notIncluding) || file.Name.ToLower().Contains(exceptionToNotIncludeRule))).ToList();
            return listOfFiles;
        }

        public static void SetVersionLogFilePath(string path)
        {
            LogFilePath = path;
            
            // Check if file already exists. If yes, delete it.     
            if (Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }
        }

        public static void AddLineToVersionLogFile(string line)
        {
            if (string.IsNullOrEmpty(LogFilePath))
            {
                Console.WriteLine("Log File Path Not Specified");
                return;
            }
            
            using StreamWriter sw = File.AppendText(LogFilePath);
            sw.Write($"{line}\n");
            sw.Close();
        }

        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}