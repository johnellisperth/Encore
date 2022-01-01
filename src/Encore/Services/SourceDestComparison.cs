using Encore.Helpers;
using Encore.Models;
using Microsoft.Extensions.Logging;
using Storage;
using System.ComponentModel;

namespace Encore.Services;

public class SourceDestComparison// : INotifyPropertyChanged
{
    public string Source { get; private set; } = string.Empty;
    public string Dest { get; private set; } = string.Empty;
    public List<FoldersPair> LonelySourceFolders { get; private set; } = new ();
    public List<FoldersPair> LonelyDestFolders { get; private set; } = new ();
    public List<FilesPair> DiffSourceFiles { get; private set; } = new();
    public List<FilesPair> DiffDestFiles { get; private set; } = new();


    private readonly SafeFileSystemHelper SafeFileHelper_ ;
    private readonly ILogger Log_;
    private ProgressManager ProgressManager_;
    private AppSettings AppSettings_;

   // public event PropertyChangedEventHandler? PropertyChanged;

    public SourceDestComparison(ProgressManager progress_manager, ILogger<SourceDestComparison> logger,
        SafeFileSystemHelper safe_file_helper, AppSettings app_settings)
    {
        Log_ = logger;
        ProgressManager_ = progress_manager;
        ProgressManager_ = progress_manager;
        SafeFileHelper_ = safe_file_helper;
        AppSettings_ = app_settings;
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
                DetermineLonelyDestFolders(true);
                long bytes_to_change = LonelyDestFolders.Sum(f => f.EndFolderSize);
                ProgressManager_.NextSubStep(bytes_to_change);
                DeleteLonelyFoldersInDest();

                DetermineDiffDestFiles();
                bytes_to_change = DiffDestFiles.Sum(f => f.EndFileSize);
                ProgressManager_.NextSubStep(bytes_to_change);
                DeleteDiffDestFiles();

                DetermineLonelySourceFolders(true);
                bytes_to_change = LonelySourceFolders.Sum(f => f.StartFolderSize);
                ProgressManager_.NextSubStep(bytes_to_change);
                CopyFoldersToDest();

                DetermineDiffSourceFiles();
                bytes_to_change = DiffSourceFiles.Sum(f => f.StartFileSize);
                ProgressManager_.NextSubStep(bytes_to_change);
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
        DetermineLonelyDestFolders(false);
        ProgressManager_.UpdateProgress(25);
        DetermineLonelySourceFolders(false);
        ProgressManager_.UpdateProgress(50);
        DetermineDiffDestFiles();
        ProgressManager_.UpdateProgress(75);
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
            if (!fp.IsSameSize)///IsSame(true, 2000000000))
                DiffSourceFiles.Add(fp);
        }
    }



    private void DetermineDiffDestFiles()
    {
        LogInfo("Determine all dest files that are different from matching source files");

        DiffDestFiles = new();
        var lonelyDestFolders = LonelyDestFolders.Select(fp => fp.End).ToArray();
        foreach (var dest_file in FileCompareHelper.GetAllFiles(Dest))
        {
            FilesPair fp = new (FileCompareHelper.DiffDriveFilename(Source, dest_file), dest_file);
            if (!fp.IsSameSize)//IsSame(true, 2000000000))
                DiffDestFiles.Add(fp);
        }
    }

    private void DetermineLonelyDestFolders(bool determine_folder_size)
    {
        LogInfo("Determine all dest folders that have no matching source folders.");

        LonelyDestFolders = new();
           
        foreach (var dest_folder in FileCompareHelper.GetAllFolders(Dest))
        {
            FoldersPair fp = new (FileCompareHelper.DiffDriveFilename(Source, dest_folder), dest_folder,determine_folder_size);
                
            if (!fp.BothExist())///same as saying if the Source doesnt Exists
                LonelyDestFolders.Add(fp);
        }
    }

    private void DetermineLonelySourceFolders(bool determine_folder_size)
    {
        LogInfo("Determine all source folders that have no matching dest folders.");

        LonelySourceFolders = new();
        foreach (var source_folder in FileCompareHelper.GetAllFolders(Source))
        {
            FoldersPair fp = new (source_folder, FileCompareHelper.DiffDriveFilename(Dest, source_folder), determine_folder_size);
            if (!fp.BothExist())
                LonelySourceFolders.Add(fp);
        }
    }

    private void CopyFilesToDest()
    {
        foreach (var file_pair in DiffSourceFiles)
        {
            SafeFileHelper_.CopyFile(file_pair.Start, file_pair.End, true);
            ProgressManager_.Update(file_pair.StartFileSize);
        }
    }

    private void CopyFoldersToDest()
    {
        foreach (var folder_pair in LonelySourceFolders)
        {
            SafeFileHelper_.CopyFolder(folder_pair.Start, folder_pair.End);
            ProgressManager_.Update(folder_pair.StartFolderSize);
        }
    }

    private void DeleteLonelyFoldersInDest()
    {
        foreach (var folder_pair in LonelyDestFolders)
        {
            SafeFileHelper_.DeleteFolder(folder_pair.End);
            ProgressManager_.Update(folder_pair.EndFolderSize);
        }
    }

    private void DeleteDiffDestFiles()
    {
        foreach (var file_pair in DiffDestFiles)
        {
            SafeFileHelper_.DeleteFile(file_pair.End);
            ProgressManager_.Update(file_pair.EndFileSize);
        }
    }

    private void LogInfo(string message) => Log_.LogInformation($"{Source} -> {Dest}: {message}");

    private void LogError(string message) => Log_.LogError($"{Source} -> {Dest}: {message}");

}

