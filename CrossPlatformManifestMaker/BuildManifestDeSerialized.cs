using System;
using System.Collections.Generic;
using System.Text;

namespace CrossPlatformManifestMaker
{
    public class BuildManifestDeSerialized
    {
        private int NumberOfPaks;
        private string BuildId;
        private List<ManifestPakDetail> ManifestPakDetails = new List<ManifestPakDetail>();

        /// <summary>
        /// Serialize all data into the format that is expected from the build manifest
        /// </summary>
        /// <returns></returns>
        public string Serialize(string platform, string quality)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("$NUM_ENTRIES = ").Append(NumberOfPaks.ToString()).AppendLine();
            stringBuilder.Append("$BUILD_ID = ").Append(BuildId).AppendLine();

            foreach (var manifestPakDetail in ManifestPakDetails)
            {
                stringBuilder.Append(manifestPakDetail.PakChunkName).Append("\t")
                    .Append(manifestPakDetail.PakSizeInBytes).Append("\t")
                    .Append(manifestPakDetail.PakVersionNumber).Append("\t").Append(manifestPakDetail.ChunkId)
                    .Append("\t").Append(manifestPakDetail.PathRelativeToManifest).AppendLine();
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Deserialize build manifest file
        /// </summary>
        /// <param name="buildManifestLines">File read and lines sent as a list</param>
        /// <exception cref="Exception"></exception>
        public void DeserializeBuildManifestFileLines(string[] buildManifestLines)
        {
            if (buildManifestLines.Length < 3)
            {
                Console.WriteLine("Build manifest cant have less than 3 lines");
                throw new Exception("Cant have less than 3 lines in the manifest");
                return;
            }

            string numberOfPaksLine = buildManifestLines[0];
            var splitStringNumberOfPaks = numberOfPaksLine.Split('=');
            try
            {
                NumberOfPaks = int.Parse(splitStringNumberOfPaks[1].Trim());
            }
            catch (Exception e)
            {
                Console.WriteLine("Number of paks line reading failed, could there be an error in format or parsing from string?");
                if (e.InnerException != null) throw e.InnerException;;
            }

            string buildIdLine = buildManifestLines[1];
            var splitStringBuildIdLine = buildIdLine.Split('=');
            BuildId = splitStringBuildIdLine[1].Trim();

            for (int i = 2; i < buildManifestLines.Length; i++)
            {
                ManifestPakDetail manifestPakDetail = ParseManifestPakDetailFromLine(buildManifestLines[i]);
                ManifestPakDetails.Add(manifestPakDetail);
            }
        }

        private ManifestPakDetail ParseManifestPakDetailFromLine(string buildManifestLine)
        {
            var splitStrings = buildManifestLine.Split("\t");

            ManifestPakDetail manifestPakDetail = new ManifestPakDetail();

            try
            {
                manifestPakDetail.PakChunkName = splitStrings[0];
                manifestPakDetail.PakSizeInBytes = UInt64.Parse(splitStrings[1]);
                manifestPakDetail.PakVersionNumber = splitStrings[2];
                manifestPakDetail.ChunkId = UInt32.Parse(splitStrings[3]);
                manifestPakDetail.PathRelativeToManifest = splitStrings[4];
            }
            catch (Exception e)
            {
                Console.WriteLine("Manifest line reading failed, could there be an error in format or parsing from string?");
                if (e.InnerException != null) throw e.InnerException;
            }

            return manifestPakDetail;
        }
    }

    public class ManifestPakDetail
    {
        public string PakChunkName;
        public UInt64 PakSizeInBytes;
        public string PakVersionNumber;
        public UInt32 ChunkId;
        public string PathRelativeToManifest;
    }
}