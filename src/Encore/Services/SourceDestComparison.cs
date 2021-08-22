using Encore.Helpers;
using Encore.Models;
using Microsoft.Extensions.Logging;
using Storage.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace RemoveDuplicateFiles.Services
{
    public class SourceDestComparison : INotifyPropertyChanged
    {
        public string Source { get; }
        public string Dest { get; }
        public List<FoldersPair> LonelySourceFolders { get; private set; }
        public List<FoldersPair> LonelyDestFolders { get; private set; }
        public List<FilesPair> DiffSourceFiles { get; private set; }
        public List<FilesPair> DiffDestFiles { get; private set; }

        private readonly SafeFileSystemHelper SafeFileHelper_ = new();

        private readonly ILogger Log_;
        private ProgressManager ProgressManager_;
        public SourceDestComparison(string source_folder, string dest_folder, ProgressManager progress_manager, ILogger logger)
        {
            Source = source_folder;
            Dest = dest_folder;
            SafeFileHelper_.EditableDrive = dest_folder;
            Log_ = logger;
            ProgressManager_ = progress_manager;

        }

        private void DetermineDiffSourceFiles()
        {
            LogInfo($"Determine all source files that are different from matching dest files");
            DiffSourceFiles = new();
            foreach (var source_file in FileCompareHelper.GetAllFiles(Source))
            {
                string possible_dest_file = FileCompareHelper.DiffDriveFilename(Dest, source_file);
                if (!FileSystemHelper.DoFilesMatch(source_file, possible_dest_file, true))
                    DiffSourceFiles.Add(new FilesPair(source_file, possible_dest_file));
            }
        }

        private void DetermineDiffDestFiles()
        {
            LogInfo("Determine all dest files that are different from matching source files");

            DiffDestFiles = new();
            foreach (var dest_file in FileCompareHelper.GetAllFiles(Dest))
            {
                string possible_source_file = FileCompareHelper.DiffDriveFilename(Source, dest_file);
                if (!FileSystemHelper.DoFilesMatch(possible_source_file, dest_file, true))
                    DiffDestFiles.Add(new(possible_source_file, dest_file));
            }
        }

        private void DetermineLonelyDestFolders()
        {
            LogInfo("Determine all dest folders that have no matching source folders.");

            LonelyDestFolders = new();
            foreach (var dest_folder in FileCompareHelper.GetAllFolders(Dest))
            {
                string possible_source_folder = FileCompareHelper.DiffDriveFilename(Source, dest_folder);
                if (!Directory.Exists(possible_source_folder))
                    LonelyDestFolders.Add(new(possible_source_folder, dest_folder));
            }
        }

        private void DetermineLonelySourceFolders()
        {
            LogInfo("Determine all source folders that have no matching dest folders.");

            LonelySourceFolders = new();
            foreach (var source_folder in FileCompareHelper.GetAllFolders(Source))
            {
                string possible_dest_folder = FileCompareHelper.DiffDriveFilename(Dest, source_folder);
                if (!Directory.Exists(possible_dest_folder))
                    LonelySourceFolders.Add(new FoldersPair(source_folder, possible_dest_folder));
            }
        }

        public void PerformPreviewComparison()
        {
            LogInfo($"Performing preview.");
            DetermineLonelyDestFolders();
            DetermineLonelySourceFolders();
            DetermineDiffDestFiles();
            DetermineDiffSourceFiles();
            LogInfo($"Finished performing preview.");

        }

        public bool PerformEcho(bool preview)
        {
            LogInfo($"Performing echoing.");
            if (preview)
                PerformPreviewComparison();
            else
            {
                LogInfo($"Performing echoing.");
                DetermineLonelyDestFolders();
                ProgressManager_.NextReport();
                DeleteLonelyFoldersInDest();

                DetermineDiffDestFiles();
                ProgressManager_.NextReport();
                DeleteDiffDestFiles();

                DetermineLonelySourceFolders();
                ProgressManager_.NextReport();
                CopyFoldersToDest();

                DetermineDiffSourceFiles();
                ProgressManager_.NextReport();
                CopyFilesToDest();

                PerformPreviewComparison();
                LogInfo($"Finished performing echoing.");
            }

            ProgressManager_.Finish();
            return true;
        }

        private void CopyFilesToDest()
        {
            foreach (var file_pair in DiffSourceFiles)
                SafeFileHelper_.CopyFile(file_pair.Start, file_pair.End, true);
        }

        private void CopyFoldersToDest()
        {
            foreach (var folder_pair in LonelySourceFolders)
                SafeFileHelper_.CopyFolder(folder_pair.Start, folder_pair.End);
        }

        private void DeleteLonelyFoldersInDest()
        {
            foreach (var folder_pair in LonelyDestFolders)
                SafeFileHelper_.DeleteFolder(folder_pair.End);
        }

        private void DeleteDiffDestFiles()
        {
            foreach (var file_pair in DiffDestFiles)
                SafeFileHelper_.DeleteFile(file_pair.End);
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void LogInfo(string message) => Log_.LogInformation($"{Source} -> {Dest}: {message}");

    }
}
