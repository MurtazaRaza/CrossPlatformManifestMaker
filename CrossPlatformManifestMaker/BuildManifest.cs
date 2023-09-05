using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.CompilerServices;

namespace CrossPlatformManifestMaker
{
    public class BuildManifest
    {
        private int NumberOfPaks;
        private string BuildId;
        private Dictionary<string, ManifestPakDetail> manifestPakDetailsDictionary = new Dictionary<string, ManifestPakDetail>();

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
            ReorderInAscending();
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("$NUM_ENTRIES = ").Append(NumberOfPaks.ToString()).AppendLine();
            stringBuilder.Append("$BUILD_ID = ").Append(BuildId).AppendLine();

            foreach (var (key, value) in manifestPakDetailsDictionary)
            {
                stringBuilder.Append(value.PakChunkName).Append("\t")
                    .Append(value.PakSizeInBytes).Append("\t")
                    .Append(value.PakVersionNumber).Append("\t").Append(value.ChunkId)
                    .Append("\t").Append(value.PathRelativeToManifest).Append("\n");
            }

            return stringBuilder.ToString();
        }

        private void ReorderInAscending()
        {
            manifestPakDetailsDictionary = manifestPakDetailsDictionary.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
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
                manifestPakDetailsDictionary.Add(manifestPakDetail.PakChunkName, manifestPakDetail);
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
            // This loops through all the paks read from previous manifest (if any) and removes all the paks from the list that 
            // are not found in the new pak files folder
            foreach (var (key, value) in manifestPakDetailsDictionary)
            {
                if (!pakFiles.Exists(x => x.Name.Equals(key)))
                    manifestPakDetailsDictionary.Remove(key);
            }

            foreach (var pakFile in pakFiles)
            {
                // This loops through all the pak files and if any pak is found that exists in the new pak folder that was not 
                // found in the previous one add it
                
                if (manifestPakDetailsDictionary.TryGetValue(pakFile.Name, out var manifestPakDetailFromList))
                {
                    // Because we might not get changed list from caller - temporary check added here to re get size in case 
                    // size was changed // TODO Remove later!
                    manifestPakDetailFromList.PakSizeInBytes = pakFile.Length.ToString();
                }
                else
                {
                    ManifestPakDetail manifestPakDetail = new ManifestPakDetail();
                    manifestPakDetail.PakChunkName = pakFile.Name;
                    manifestPakDetail.PakSizeInBytes = pakFile.Length.ToString();
                    manifestPakDetail.PakVersionNumber = DEFAULT_VERSION_STRING;
                    manifestPakDetail.ChunkId = GetChunkIdFromPakName(pakFile.Name);
                    manifestPakDetail.PathRelativeToManifest =
                        GetPathRelativeToManifest(pakFile.Name, qualityType, platformName);

                    FileUtils.AddLineToVersionLogFile(
                        $"{manifestPakDetail.PakChunkName}\t{manifestPakDetail.PakVersionNumber}->{manifestPakDetail.PakVersionNumber}");

                    manifestPakDetailsDictionary.Add(manifestPakDetail.PakChunkName, manifestPakDetail);
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

        public void UpdateVersionsOfPaks(HashSet<string> recievedPakNamesToUpdate, UpdateType updateType = UpdateType.All)
        {
            HashSet<string> pakNamesToUpdate = new HashSet<string>(recievedPakNamesToUpdate);
            
            if (updateType == UpdateType.All)
            {
                UpdateAllPaks(ref pakNamesToUpdate);
            }
            else if (updateType == UpdateType.Patch)
            {
                UpdatePatchPaks(ref pakNamesToUpdate);
            }
            else
            {
                UpdateSelectivePaks(recievedPakNamesToUpdate, ref pakNamesToUpdate);
            }
            
            FileUtils.AddLineToVersionLogFile("Updates:");
            UpdateVersionOfPaksInternal(pakNamesToUpdate.ToList());

        }

        private void UpdateAllPaks(ref HashSet<string> pakNamesToUpdate)
        {
            foreach (var manifestPakDetail in manifestPakDetailsDictionary)
            {
                pakNamesToUpdate.Add(manifestPakDetail.Key);
            }
        }

        private void UpdatePatchPaks(ref HashSet<string> pakNamesToUpdate)
        {
            foreach (var manifestPakDetail in manifestPakDetailsDictionary)
            {
                if (manifestPakDetail.Key.Contains("_P") || manifestPakDetail.Key.Contains("_p"))
                    pakNamesToUpdate.Add(manifestPakDetail.Key);
            }
        }

        private void UpdateSelectivePaks(HashSet<string> recievedPakNamesToUpdate, ref HashSet<string> pakNamesToUpdate)
        {
            foreach (var recievedPakNameToUpdate in recievedPakNamesToUpdate)
            {
                if (!recievedPakNameToUpdate.Contains("_s") && !recievedPakNameToUpdate.Contains("optional")) 
                    continue;
                
                if (!manifestPakDetailsDictionary.TryGetValue(recievedPakNameToUpdate,
                        out ManifestPakDetail manifestPakDetail))
                    continue;

                var paksWithSameId =
                    manifestPakDetailsDictionary.Where(x => x.Value.ChunkId == manifestPakDetail.ChunkId);

                foreach (var paksKeyValue in paksWithSameId)
                {
                    if (!recievedPakNameToUpdate.Contains("_s") &&
                        !recievedPakNameToUpdate.Contains("optional") && !recievedPakNamesToUpdate.Contains("_P")
                        && !recievedPakNamesToUpdate.Contains("_p"))
                    {
                        pakNamesToUpdate.Add(paksKeyValue.Key);
                    }
                }
            }
        }

        private void UpdateVersionOfPaksInternal(List<string> pakNamesToUpdate)
        {
            foreach (var pakName in pakNamesToUpdate)
            {
                if(manifestPakDetailsDictionary.TryGetValue(pakName, out ManifestPakDetail manifestPakDetail ))
                    manifestPakDetail.IncrementPakVersionNumber();
            }
        }

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

    public enum UpdateType
    {
        All,
        Patch,
        Selective
    }

}