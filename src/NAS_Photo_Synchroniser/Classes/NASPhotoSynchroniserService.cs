using NAS_Photo_Synchroniser.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace NAS_Photo_Synchroniser.Classes
{
    public class NASPhotoSynchroniserService : INASPhotoSynchroniserService
    {
        private static readonly StringBuilder logMessage = new();

        public void DoWork()
        {
            try
            {
                LogMessage($"Beginning process at {DateTime.Now}");

                ReadConfiguration();
                var foldersToProcess = GetAllSubFoldersFromSource(LocalSettings.SourceFolder);

                LogMessage($"Number of folders found : {foldersToProcess.Count}");

                if (foldersToProcess != null)
                {
                    ClearListFromExcludedFolder(foldersToProcess);

                    LogMessage($"Number of folders to process {foldersToProcess.Count}");

                    foreach (var folder in foldersToProcess)
                    {
                        LogMessage($"Processing folder {folder}");
                        ProcessFolder(folder);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"An error occured during the process of the application :\n {ex.Message}");
            }
            finally
            {
                LogMessage($"Ending process at {DateTime.Now}");
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), $"NasPhotoSynchroniser_Logs_{DateTime.Now:yyyyMMdd_hhmmss}.txt"), logMessage.ToString());
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Method that will read configuration and get the source folder where the photos and videos are
        /// </summary>
        private static void ReadConfiguration()
        {
            LogMessage("Reading configuration");

            LocalSettings.SourceFolder = ConfigurationManager.AppSettings["SourceFolder"];

            if (string.IsNullOrEmpty(LocalSettings.SourceFolder))
            {
                throw new Exception("AppSettings 'SourceFolder' is empty or missing from configuration");
            }

            LocalSettings.DestinationFolder = ConfigurationManager.AppSettings["DestinationFolder"];

            if (string.IsNullOrEmpty(LocalSettings.DestinationFolder))
            {
                throw new Exception("AppSettings 'DestinationFolder' is empty or missing form configuration");
            }

            LocalSettings.ShouldDeleteFolders = ConfigurationManager.AppSettings["ShouldDeleteFolders"].Equals("True", StringComparison.OrdinalIgnoreCase);
            LocalSettings.ExludedFolders = ConfigurationManager.AppSettings["ExcludedFolders"]?.Split(";")?.ToList();
        }

        /// <summary>
        /// Get all the subfolders from the subFolder to process
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <returns></returns>
        private static List<string> GetAllSubFoldersFromSource(string sourceFolder)
        {
            LogMessage("Get all directories from source");
            return Directory.GetDirectories(sourceFolder).ToList();
        }

        /// <summary>
        /// Method that will process one folder, take files in it and move them in the correct destination folder
        /// </summary>
        /// <param name="folder"></param>
        private static void ProcessFolder(string folder)
        {
            List<string> files = GetAllFilesFromFolder(folder);

            //TODO : Exclude uneeded files
            //ExcludeUneededFilesFromList(files);

            MoveFileToDestinationFolder(files, folder);
        }

        /// <summary>
        /// GetAlLFilesFromFolder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static List<string> GetAllFilesFromFolder(string folder)
        {
            LogMessage($"Getting all files from folder {folder} to process");
            return Directory.GetFiles(folder).ToList();
        }

        /// <summary>
        /// Clear the list of uneeded folder
        /// </summary>
        /// <param name="foldersToProcess"></param>
        private static void ClearListFromExcludedFolder(List<string> foldersToProcess)
        {
            LogMessage("Exclude excluded folders from initial list");
            LocalSettings.ExludedFolders.ForEach(x => foldersToProcess.Remove(Path.Combine(LocalSettings.SourceFolder, x)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        private static void MoveFileToDestinationFolder(List<string> files, string folder)
        {
            LogMessage($"Number of files to process in the folder {folder} : {files.Count}");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                string fileCreationYear = fileInfo.LastWriteTimeUtc.Date.Year.ToString();
                var fileDestination = Path.Combine(LocalSettings.DestinationFolder, fileCreationYear, fileInfo.Name);
                if (!Directory.GetDirectories(LocalSettings.DestinationFolder).ToList().Contains(Path.Combine(LocalSettings.DestinationFolder, fileCreationYear)))
                {
                    LogMessage($"Folder {fileCreationYear} does not exists yet, creation of the folder");
                    Directory.CreateDirectory(Path.Combine(LocalSettings.DestinationFolder, fileCreationYear));
                }

                LogMessage($"Moving file {file} to {Path.Combine(LocalSettings.DestinationFolder, fileCreationYear)}");
                File.Move(file, fileDestination);
            }

            if (Directory.GetFiles(folder).Length == 0 && 
                Directory.GetDirectories(folder).Length == 0 && 
                LocalSettings.ShouldDeleteFolders)
            {
                LogMessage($"folder {folder} is empty, deleting folder");
                Directory.Delete(folder);
            }
        }

        private static void LogMessage(string message)
        {
            logMessage.AppendLine(message);
            Console.WriteLine(message);
        }
    }
}
