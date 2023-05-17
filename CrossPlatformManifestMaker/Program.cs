using System;
using System.Collections.Generic;

namespace CrossPlatformManifestMaker
{
    class Program
    {

        private static bool isFirstManifest = false;

        /// <summary>
        /// Generate Manifest through commandline
        /// </summary>
        /// <param name="args">
        /// arg 0 : Path of last manifest file (or null)
        /// arg 1 : Path where all new paks are [eg: C:/Users/Paks/]
        /// arg 2 : Build Version Number (build content id) [eg: v3.2.0]
        /// arg 3 : Platform [Windows, Android, IOS, TvOS, Mac]
        /// arg 4 : Quality [high, medium, low]
        /// arg 5 : Should Update Versions of all files [true, false]
        /// arg 6+ : List of all changed paks
        /// </param>
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("ERROR: Need at least 4 arguments");
                return;
            }

            string previousReleaseBuildManifestPath = args[0];
            if (!FileUtils.Exists(previousReleaseBuildManifestPath))
            {
                Console.WriteLine("Previous Manifest Not Found, comparisions will be skipped");
                isFirstManifest = true;
            }

            string paksPath = args[1];
            string buildVersionNumber = args[2];

            string platform = args[3];
            if (!CheckPlatformSupport(platform))
                return;

            string quality = args[4];
            if (!CheckQualitySupport(quality))
                return;

            string shouldUpdateAllVersionsString = args[5];
            if (!bool.TryParse(args[5], out bool shouldUpdateAllVersions))
            {
                Console.WriteLine("Error parsing Should Update All Versions arg");
                return;
            }

            HashSet<string> pakNamesToUpdate = new HashSet<string>();
            for (int i = 6; i < args.Length; i++)
            {
                pakNamesToUpdate.Add(args[i]);
            }

            BuildManifest buildManifest = new BuildManifest();
            FileUtils.SetVersionLogFilePath($"{paksPath}/_VersionUpdateLog-{platform}.txt");
            
            if (!isFirstManifest)
            {
                string[] allLinesInManifest = FileUtils.GetAllLinesInFile(previousReleaseBuildManifestPath);
                buildManifest.DeserializeBuildManifestFileLines(allLinesInManifest);
            }
            var pakFiles = FileUtils.GetAllFilesInFolderWith(paksPath, "*.pak", "pakchunk0");
            buildManifest.ReconcileWithCurrentPaks(pakFiles, platform, quality);
            if (!isFirstManifest)
                buildManifest.UpdateVersionsOfPaks(pakNamesToUpdate, shouldUpdateAllVersions);
            buildManifest.SetNumberOfPaks(pakFiles.Count);
            buildManifest.SetBuildId(buildVersionNumber);

            FileUtils.WriteStringToFile(buildManifest.SerializeObject(platform, quality),
                $"{paksPath}/BuildManifest-{platform}.txt");

        }

        private static bool CheckPlatformSupport(string platform)
        {
            if (!platform.Equals("Windows") && !platform.Equals("Android") && !platform.Equals("IOS") &&
                !platform.Equals("TvOS") && !platform.Equals("Mac"))
            {
                Console.WriteLine("ERROR: Unsupported platform entered; only supports Windows, Android, IOS, Mac, and TvOS strings");
                return false;
            }

            return true;
        }
        
        private static bool CheckQualitySupport(string quality)
        {
            if (!quality.Equals("high") && !quality.Equals("medium") && !quality.Equals("low"))
            {
                Console.WriteLine("ERROR: Unsupported quality entered; only supports high, medium, and low");
                return false;
            }

            return true;
        }
    }
}
