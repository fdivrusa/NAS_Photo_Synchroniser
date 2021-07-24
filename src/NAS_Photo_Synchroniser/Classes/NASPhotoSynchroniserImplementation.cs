using NAS_Photo_Synchroniser.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace NAS_Photo_Synchroniser.Classes
{
    public class NASPhotoSynchroniserImplementation : INASPhotoSynchroniserImplementation
    {
        public void DoWork()
        {
            try
            {
                ReadConfiguration();
                var foldersToProcess = GetAllSubFoldersFromSource(LocalSettings.SourceFolder);

                Console.WriteLine($"Number of folders found : {foldersToProcess.Count}");

                if (foldersToProcess != null)
                {
                    ClearListFromExcludedFolder(foldersToProcess);

                    Console.WriteLine($"Number of folders to process {foldersToProcess.Count}");

                    foreach (var folder in foldersToProcess)
                    {
                        ProcessFolder(folder);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured during the process of the application :\n {ex.Message}");
            }
        }

        /// <summary>
        /// Method that will read configuration and get the source folder where the photos and videos are
        /// </summary>
        private static void ReadConfiguration()
        {
            Console.WriteLine("Reading configuration");

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

            LocalSettings.AcceptedFiles = ConfigurationManager.AppSettings["AcceptedFiles"].Split(";").ToList();

            if (LocalSettings.AcceptedFiles == null || LocalSettings.AcceptedFiles.Count == 0)
            {
                throw new Exception("AppSettings 'AcceptedFiles' is empty or missing from configuration");
            }

            LocalSettings.ShouldDeleteFolders = ConfigurationManager.AppSettings["ShouldDeleteFolders"] == "False";
            LocalSettings.ExludedFolders = ConfigurationManager.AppSettings["ExcludedFolders"]?.Split(";")?.ToList();
        }

        /// <summary>
        /// Get all the subfolders from the subFolder to process
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <returns></returns>
        private static List<string> GetAllSubFoldersFromSource(string sourceFolder)
        {
            Console.WriteLine("Get all directories from source");
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

            MoveFileToDestinationFolder(files);
        }

        /// <summary>
        /// GetAlLFilesFromFolder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static List<string> GetAllFilesFromFolder(string folder)
        {
            return Directory.GetFiles(folder).ToList();
        }

        /// <summary>
        /// Clear the list of uneeded folder
        /// </summary>
        /// <param name="foldersToProcess"></param>
        private static void ClearListFromExcludedFolder(List<string> foldersToProcess)
        {
            Console.WriteLine("Exclude excluded folders from initial list");
            LocalSettings.ExludedFolders.ForEach(x => foldersToProcess.Remove(Path.Combine(LocalSettings.SourceFolder, x)));
        }

        /// <summary>
        /// Exclude all uneeded files from list
        /// </summary>
        /// <param name="files"></param>
        private static void ExcludeUneededFilesFromList(List<string> files)
        {
            foreach (var acceptedFile in LocalSettings.AcceptedFiles)
            {
                files.RemoveAll(x => !acceptedFile.Any(y => x.Contains(y)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        private static void MoveFileToDestinationFolder(List<string> files)
        {
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                string fileCreationYear = fileInfo.CreationTime.Date.Year.ToString();
                var fileDestination = Path.Combine(LocalSettings.DestinationFolder, fileCreationYear, fileInfo.Name);
                if (Directory.GetDirectories(LocalSettings.DestinationFolder).ToList().Contains(fileCreationYear))
                {
                    Console.WriteLine($"Folder {fileCreationYear} does not exists yet, creation of the folder");
                    Directory.CreateDirectory(Path.Combine(LocalSettings.DestinationFolder, fileCreationYear));
                }

                Console.WriteLine($"Moving file {file} to {fileDestination}");
                File.Move(file, fileDestination);
            }
        }
    }
}
