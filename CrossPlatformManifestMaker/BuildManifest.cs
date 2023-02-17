using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrossPlatformManifestMaker
{
    public class BuildManifest
    {
        private int NumberOfPaks;
        private string BuildId;
        private List<ManifestPakDetail> ManifestPakDetails = new List<ManifestPakDetail>();

        private static readonly string DEFAULT_VERSION_STRING = "ver01";
        private readonly List<string> ESCAPE_CHARACTERS_LIST = new List<string>()
        {
            "-",
            "_",
            "-"
        };
        
        /// <summary>
        /// Serialize all data into the format that is expected from the build manifest
        /// </summary>
        /// <returns></returns>
        public string SerializeObject(string platform, string quality)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("$NUM_ENTRIES = ").Append(NumberOfPaks.ToString()).AppendLine();
            stringBuilder.Append("$BUILD_ID = ").Append(BuildId).AppendLine();

            ManifestPakDetails.OrderBy(x => x.ChunkId);

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
                Console.WriteLine("ERROR: Build manifest cant have less than 3 lines");
                throw new Exception("Cant have less than 3 lines in the manifest");
            }

            string numberOfPaksLine = buildManifestLines[0];
            var splitStringNumberOfPaks = numberOfPaksLine.Split('=');
            try
            {
                NumberOfPaks = int.Parse(splitStringNumberOfPaks[1].Trim());
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Number of paks line reading failed, could there be an error in format or parsing from string?");
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
                manifestPakDetail.PakSizeInBytes = splitStrings[1];
                manifestPakDetail.PakVersionNumber = splitStrings[2];
                manifestPakDetail.ChunkId = splitStrings[3];
                manifestPakDetail.PathRelativeToManifest = splitStrings[4];
            }
            catch (Exception e)
            {
                Console.WriteLine("Manifest line reading failed, could there be an error in format or parsing from string?");
                if (e.InnerException != null) throw e.InnerException;
            }

            return manifestPakDetail;
        }

        public void SetNumberOfPaks(int numberOfPaks)
        {
            NumberOfPaks = numberOfPaks;
        }

        public void SetBuildId(string buildId)
        {
            BuildId = buildId;
        }

        public void ReconcileWithCurrentPaks(List<FileInfo> pakFiles, string platformName, string qualityType)
        {
            foreach (var pakFile in pakFiles)
            {
                if (!ManifestPakDetails.Exists(x => x.PakChunkName.Equals(pakFile.Name)))
                {
                    ManifestPakDetail manifestPakDetail = new ManifestPakDetail();
                    manifestPakDetail.PakChunkName = pakFile.Name;
                    manifestPakDetail.PakSizeInBytes = pakFile.Length.ToString();
                    manifestPakDetail.PakVersionNumber = DEFAULT_VERSION_STRING;
                    manifestPakDetail.ChunkId = GetChunkIdFromPakName(pakFile.Name);
                    manifestPakDetail.PathRelativeToManifest =
                        GetPathRelativeToManifest(pakFile.Name, qualityType, platformName);
                    
                    FileUtils.AddLineToVersionLogFile($"{manifestPakDetail.PakChunkName}\t{manifestPakDetail.PakVersionNumber}->{manifestPakDetail.PakVersionNumber}");

                    ManifestPakDetails.Add(manifestPakDetail);
                    continue;
                }
            }
        }

        private string GetChunkIdFromPakName(string pakFileName)
        {
            string toBeSearched = "pakchunk";
            string subStringedIntermediate = pakFileName.Substring(pakFileName.IndexOf(toBeSearched, StringComparison.Ordinal) + toBeSearched.Length);
            string pakFileChunkId = GetUntilOrEmpty(subStringedIntermediate, ESCAPE_CHARACTERS_LIST);
            pakFileChunkId = GetChunkIdWithoutUnnecessaryText(pakFileChunkId);

            return pakFileChunkId;
        }
        
        private string GetUntilOrEmpty(string text, List<string> escapeCharacters)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            
            int charLocation = -1;

            foreach (var stopAt in escapeCharacters)
            {
                int currentEscapeCharacterIndex = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (currentEscapeCharacterIndex == 0) continue;
                    
                if (charLocation == -1)
                {
                    charLocation = currentEscapeCharacterIndex;
                    continue;
                }

                if (charLocation > currentEscapeCharacterIndex)
                {
                    charLocation = currentEscapeCharacterIndex;
                    continue;
                }
            }

            return charLocation > -1 ? text.Substring(0, charLocation) : string.Empty;
        }
        
        private string GetChunkIdWithoutUnnecessaryText(string text)
        {
            string numberOnly = Regex.Replace(text, "[^0-9.]", "");
            return numberOnly;
        }
        
        private string GetPathRelativeToManifest(string pakFileName, string qualityType, string platformName)
        {
            string pakFilePathRelative = "";
            if(!qualityType.Equals("Default"))
                pakFilePathRelative = $"/{platformName}/{qualityType}/{pakFileName}";
            else
                pakFilePathRelative = $"/{platformName}/{pakFileName}";

            return pakFilePathRelative;
        }

        public void UpdateVersionsOfPaks(List<string> pakNamesToUpdate)
        {
            FileUtils.AddLineToVersionLogFile("Updates:");
            foreach (var pakNameToUpdate in pakNamesToUpdate)
            {
                ManifestPakDetail manifestPakDetail =
                    ManifestPakDetails.FirstOrDefault(x => x.PakChunkName.Equals(pakNameToUpdate));

                if (manifestPakDetail == null)
                    continue;

                manifestPakDetail.IncrementPakVersionNumber();

                if (!manifestPakDetail.PakChunkName.Contains("_s") &&
                    !manifestPakDetail.PakChunkName.Contains("optional"))
                    continue;


                // TODO get all parent paks. Parent pak will have the same name as the pak but no _s or optional?
                // also check whether that is the pak that was updated so recursive call is not made
                List<ManifestPakDetail> paksWithSameId =
                    ManifestPakDetails.Where(x => x.ChunkId == manifestPakDetail.ChunkId).ToList();

                for (var index = paksWithSameId.Count - 1; index >= 0; index--)
                {
                    var pak = paksWithSameId[index];
                    // This already will be updated so no need to do it here
                    if (pakNamesToUpdate.Contains(pak.PakChunkName))
                        continue;

                    if (!pak.PakChunkName.Contains("_s") && !pak.PakChunkName.Contains("optional"))
                    {
                        pak.IncrementPakVersionNumber();
                    }
                }

            }
        }

        // TODO add log to a file with all the changes when changing versions
        private class ManifestPakDetail
        {
            public string PakChunkName;
            public string PakSizeInBytes;
            public string PakVersionNumber;
            public string ChunkId;
            public string PathRelativeToManifest;

            public void IncrementPakVersionNumber()
            {
                int currentVersionIntegerPart = GetNumberWithoutString(PakVersionNumber);
                if (currentVersionIntegerPart == -1)
                {
                    PakVersionNumber = DEFAULT_VERSION_STRING;
                    return;
                }

                currentVersionIntegerPart++;
                string versionStringPart = GetStringWithoutNumber(PakVersionNumber);

                string versionNumberBeforeUpdate = PakVersionNumber;

                PakVersionNumber = $"{versionStringPart}{currentVersionIntegerPart:00}";
                FileUtils.AddLineToVersionLogFile($"{PakChunkName}\t{versionNumberBeforeUpdate}->{PakVersionNumber}");
            }

            private string GetStringWithoutNumber(string text)
            {
                var onlyLetters = new String(text.Where(Char.IsLetter).ToArray());
                return onlyLetters;
            }
        
            private int GetNumberWithoutString(String text)
            {
                bool isSuccessful = int.TryParse(Regex.Replace(text, "[^0-9.]", ""), out int ver);
                return isSuccessful ? ver : -1;
            }
        }
    }

}