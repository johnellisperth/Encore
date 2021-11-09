using Encore.Helpers;
using Encore.Models;
using Microsoft.Extensions.Logging;
using Storage;
using System.ComponentModel;

namespace Encore.Services
{
    public class SourceDestComparison : INotifyPropertyChanged
    {
        public string Source { get; private set; }
        public string Dest { get; private set; }
        public List<FoldersPair> LonelySourceFolders { get; private set; }
        public List<FoldersPair> LonelyDestFolders { get; private set; }
        public List<FilesPair> DiffSourceFiles { get; private set; }
        public List<FilesPair> DiffDestFiles { get; private set; }

        private readonly SafeFileSystemHelper SafeFileHelper_ ;
        private readonly ILogger Log_;
        private ProgressManager ProgressManager_;
        

        public SourceDestComparison(ProgressManager progress_manager, ILogger<SourceDestComparison> logger,
            SafeFileSystemHelper safe_file_helper)
        {
            Log_ = logger;
            ProgressManager_ = progress_manager;
            ProgressManager_ = progress_manager;
            SafeFileHelper_ = safe_file_helper;
        }

        public void SetSourceDest(string source_folder, string dest_folder)
        {
            Source = source_folder;
            Dest = dest_folder;
            SafeFileHelper_.EditableDrive = dest_folder;
        }

        public bool PerformEcho(bool preview)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                LogError($"Exception raised:{ex.Message}");
                return false;
            }
            return true;
        }

        private void PerformPreviewComparison()
        {
            LogInfo($"Performing preview.");
            DetermineLonelyDestFolders();
            DetermineLonelySourceFolders();
            DetermineDiffDestFiles();
            DetermineDiffSourceFiles();
            LogInfo($"Finished performing preview.");
        }

        private void DetermineDiffSourceFiles()
        {
            LogInfo($"Determine all source files that are different from matching dest files");
            DiffSourceFiles = new();
            foreach (var source_file in FileCompareHelper.GetAllFiles(Source))
            {
                FilesPair fp = new (source_file, FileCompareHelper.DiffDriveFilename(Dest, source_file));
                if (!fp.IsSame())
                    DiffSourceFiles.Add(fp);
            }
        }

        private void DetermineDiffDestFiles()
        {
            LogInfo("Determine all dest files that are different from matching source files");

            DiffDestFiles = new();
            foreach (var dest_file in FileCompareHelper.GetAllFiles(Dest))
            {
                FilesPair fp = new (FileCompareHelper.DiffDriveFilename(Source, dest_file), dest_file);
                if (!fp.IsSame())
                    DiffDestFiles.Add(fp);
            }
        }

        private void DetermineLonelyDestFolders()
        {
            LogInfo("Determine all dest folders that have no matching source folders.");

            LonelyDestFolders = new();
           
            foreach (var dest_folder in FileCompareHelper.GetAllFolders(Dest))
            {
                FoldersPair fp = new (FileCompareHelper.DiffDriveFilename(Source, dest_folder), dest_folder);
                
                if (!fp.BothExist())
                    LonelyDestFolders.Add(fp);
            }
        }

        private void DetermineLonelySourceFolders()
        {
            LogInfo("Determine all source folders that have no matching dest folders.");

            LonelySourceFolders = new();
            foreach (var source_folder in FileCompareHelper.GetAllFolders(Source))
            {
                FoldersPair fp = new (source_folder, FileCompareHelper.DiffDriveFilename(Dest, source_folder));
                if (!fp.BothExist())
                    LonelySourceFolders.Add(fp);
            }
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

        private void LogError(string message) => Log_.LogError($"{Source} -> {Dest}: {message}");

    }
}
