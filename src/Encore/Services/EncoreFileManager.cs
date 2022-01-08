using Encore.Helpers;
using Encore.Models;
using Microsoft.Extensions.Logging;
using Storage;

namespace Encore.Services;

public class EncoreFileManager
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

    public EncoreFileManager(ProgressManager progressManager, ILogger<EncoreFileManager> logger,
        SafeFileSystemHelper safeFileHelper, AppSettings appSettings)
    {
        Log_ = logger;
        ProgressManager_ = progressManager;
        ProgressManager_ = progressManager;
        SafeFileHelper_ = safeFileHelper;
        AppSettings_ = appSettings;
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
        foreach (var sourceFile in FileCompareHelper.GetAllFiles(Source))
        {
          
            FilesPair fp = new (sourceFile, FileCompareHelper.DiffDriveFilename(Dest, sourceFile));
            if (!fp.IsSameSize)///IsSame(true, 2000000000))
                DiffSourceFiles.Add(fp);
        }
    }

    private void DetermineDiffDestFiles()
    {
        LogInfo("Determine all dest files that are different from matching source files");

        DiffDestFiles = new();
        var lonelyDestFolders = LonelyDestFolders.Select(fp => fp.End).ToArray();
        foreach (var destFile in FileCompareHelper.GetAllFiles(Dest))
        {
            FilesPair fp = new (FileCompareHelper.DiffDriveFilename(Source, destFile), destFile);
            if (!fp.IsSameSize)//IsSame(true, 2000000000))
                DiffDestFiles.Add(fp);
        }
    }

    private void DetermineLonelyDestFolders(bool determineFolderSize)
    {
        LogInfo("Determine all dest folders that have no matching source folders.");

        LonelyDestFolders = new();
           
        foreach (var destFolder in FileCompareHelper.GetAllFolders(Dest))
        {
            FoldersPair fp = new (FileCompareHelper.DiffDriveFilename(Source, destFolder), destFolder,determineFolderSize);
                
            if (!fp.BothExist())///same as saying if the Source doesnt Exists
                LonelyDestFolders.Add(fp);
        }
    }

    private void DetermineLonelySourceFolders(bool determineFolderSize)
    {
        LogInfo("Determine all source folders that have no matching dest folders.");

        LonelySourceFolders = new();
        foreach (var sourceFolder in FileCompareHelper.GetAllFolders(Source))
        {
            FoldersPair fp = new (sourceFolder, FileCompareHelper.DiffDriveFilename(Dest, sourceFolder), determineFolderSize);
            if (!fp.BothExist())
                LonelySourceFolders.Add(fp);
        }
    }

    private void CopyFilesToDest()
    {
        foreach (var filePair in DiffSourceFiles)
        {
            SafeFileHelper_.CopyFile(filePair.Start, filePair.End, true);
            ProgressManager_.Update(filePair.StartFileSize);
        }
    }

    private void CopyFoldersToDest()
    {
        foreach (var folderPair in LonelySourceFolders)
        {
            SafeFileHelper_.CopyFolder(folderPair.Start, folderPair.End);
            ProgressManager_.Update(folderPair.StartFolderSize);
        }
    }

    private void DeleteLonelyFoldersInDest()
    {
        foreach (var folderPair in LonelyDestFolders)
        {
            SafeFileHelper_.DeleteFolder(folderPair.End);
            ProgressManager_.Update(folderPair.EndFolderSize);
        }
    }

    private void DeleteDiffDestFiles()
    {
        foreach (var filePair in DiffDestFiles)
        {
            SafeFileHelper_.DeleteFile(filePair.End);
            ProgressManager_.Update(filePair.EndFileSize);
        }
    }

    private void LogInfo(string message) => Log_.LogInformation($"{Source} -> {Dest}: {message}");

    private void LogError(string message) => Log_.LogError($"{Source} -> {Dest}: {message}");

}




