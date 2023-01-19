using System;
using System.Collections.Generic;
using System.IO;

namespace CrossPlatformManifestMaker
{
    class Program
    {

        private static bool IsFirstManifest = false;
        
        /// <summary>
        /// Generate Manifest through commandline
        /// </summary>
        /// <param name="args">
        /// arg 0 : Path of last manifest file (or null)
        /// arg 1 : Path where all new paks are [eg: C:/Users/Paks/]
        /// arg 2 : Build Version Number (build content id) [eg: v3.2.0]
        /// arg 3 : Platform [Windows, Android, IOS, TvOS]
        /// arg 4 : Quality [high, medium, low]
        /// arg 5+ : List of all changed paks
        /// </param>
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Need at least 4 arguments");
                return;
            }
            
            string previousReleaseBuildManifestPath = args[0]; // TODO check validity here only and throw error if incorrect or inaccessible
            if (!File.Exists(previousReleaseBuildManifestPath))
            {
                Console.WriteLine("Previous Manifest Not Found, comparisions will be skipped");
                IsFirstManifest = true;
            }
            string paksPath = args[1]; // TODO check validity here only and throw error if incorrect or inaccessible
            string buildVersionNumber = args[2];
            
            string platform = args[3];
            if (!CheckPlatformSupport(platform))
                return;
            
            string quality = args[4];
            if (!CheckQualitySupport(quality))
                return;

            List<string> pakNamesToUpdate = new List<string>();
            for (int i = 5; i < args.Length; i++)
            {
                pakNamesToUpdate.Add(args[i]);
            }

            if (IsFirstManifest)
            {
                
            }
            else
            {
                string[] allLinesInManifest = FileUtils.GetAllLinesInFile(previousReleaseBuildManifestPath);
                BuildManifestDeSerialized buildManifestDeSerialized = new BuildManifestDeSerialized();
                buildManifestDeSerialized.DeserializeBuildManifestFileLines(allLinesInManifest);

                string ReSerialized = buildManifestDeSerialized.Serialize(platform, quality);
            }
        }

        private static bool CheckPlatformSupport(string platform)
        {
            if (!platform.Equals("Windows") && !platform.Equals("Android") && !platform.Equals("IOS") &&
                !platform.Equals("TvOS"))
            {
                Console.WriteLine("Unsupported platform entered; only supports Windows, Android, IOS, and TvOS strings");
                return false;
            }

            return true;
        }
        
        private static bool CheckQualitySupport(string quality)
        {
            if (!quality.Equals("high") && !quality.Equals("medium") && !quality.Equals("low"))
            {
                Console.WriteLine("Unsupported quality entered; only supports high, medium, and low");
                return false;
            }

            return true;
        }
    }
}
