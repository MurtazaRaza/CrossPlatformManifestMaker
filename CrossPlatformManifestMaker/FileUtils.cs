using System;
using System.Collections.Generic;
using System.IO;

namespace CrossPlatformManifestMaker
{
    public static class FileUtils
    {
        public static string[] GetAllLinesInFile(string path)
        {
            string[] allLinesInFile = File.ReadAllLines(path);

            return allLinesInFile;
        }
        

        public static string[] GetAllFilesInFolderWith(string stringToInclude)
        {
            throw new NotImplementedException();
        }
    }
}