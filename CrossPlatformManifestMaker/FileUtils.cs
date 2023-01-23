using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossPlatformManifestMaker
{
    public static class FileUtils
    {
        public static string[] GetAllLinesInFile(string path)
        {
            string[] allLinesInFile = File.ReadAllLines(path);

            return allLinesInFile;
        }

        public static void WriteStringToFile(string stringToWrite, string filePathWithName)
        {
            if (!Exists(filePathWithName))
                File.Create(filePathWithName);

            try
            {
                File.WriteAllText(filePathWithName, stringToWrite);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cant write to text file? {e}");
                throw;
            }
            
        }
        
        public static List<FileInfo> GetAllFilesInFolderWith(string path, string stringToInclude, string notIncluding = "-1")
        {
            
            DirectoryInfo d = new DirectoryInfo(path); //Assuming Test is your Folder

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
            
            List<FileInfo> listOfFiles = files.Where(file => !file.Name.Contains(notIncluding)).ToList();
            return listOfFiles;
        }

        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}