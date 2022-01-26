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

    public void SetSourceDest(string source, string dest)
    {
        Source = source;
        Dest = dest;
        SafeFileHelper_.EditableDrive = dest;
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
                long bytesToChange = LonelyDestFolders.Sum(f => f.EndFolderSize);
                ProgressManager_.NextSubStep(bytesToChange);
                DeleteLonelyFoldersInDest();

                DetermineDiffDestFiles();
                bytesToChange = DiffDestFiles.Sum(f => f.EndFileSize);
                ProgressManager_.NextSubStep(bytesToChange);
                DeleteDiffDestFiles();

                DetermineLonelySourceFolders(true);
                bytesToChange = LonelySourceFolders.Sum(f => f.StartFolderSize);
                ProgressManager_.NextSubStep(bytesToChange);
                CopySourceFoldersToDest();

                DetermineDiffSourceFiles();
                bytesToChange = DiffSourceFiles.Sum(f => f.StartFileSize);
                ProgressManager_.NextSubStep(bytesToChange);
                CopySourceFilesToDest();

                PerformPreviewComparison();
                LogInfo($"Finished performing echo.");
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
        var lonelyDestFolders = LonelyDestFolders.Select(fp => fp.Dest).ToArray();
        foreach (var destFile in FileCompareHelper.GetAllFiles(Dest))
        {
            FilesPair filePair = new (FileCompareHelper.DiffDriveFilename(Source, destFile), destFile);
            if (!filePair.IsSameSize)//IsSame(true, 2000000000))
                DiffDestFiles.Add(filePair);
        }
    }

    private void DetermineLonelyDestFolders(bool determineFolderSize)
    {
        LogInfo("Determine all dest folders that have no matching source folders.");

        LonelyDestFolders = new();
           
        foreach (var destFolder in FileCompareHelper.GetAllFolders(Dest))
        {
            FoldersPair folderPair = new (FileCompareHelper.DiffDriveFilename(Source, destFolder), destFolder,determineFolderSize);
                
            if (!folderPair.BothExist())///same as saying if the Source doesnt Exists
                LonelyDestFolders.Add(folderPair);
        }
    }

    private void DetermineLonelySourceFolders(bool determineFolderSize)
    {
        LogInfo("Determine all source folders that have no matching dest folders.");

        LonelySourceFolders = new();
        foreach (var sourceFolder in FileCompareHelper.GetAllFolders(Source))
        {
            FoldersPair folderPair = new (sourceFolder, FileCompareHelper.DiffDriveFilename(Dest, sourceFolder), determineFolderSize);
            if (!folderPair.BothExist())
                LonelySourceFolders.Add(folderPair);
        }
    }

    private void CopySourceFilesToDest()
    {
        foreach (var filePair in DiffSourceFiles)
        {
            SafeFileHelper_.CopyFile(filePair.Source, filePair.Dest, true);
            ProgressManager_.Update(filePair.StartFileSize);
        }
    }

    private void CopySourceFoldersToDest()
    {
        foreach (var folderPair in LonelySourceFolders)
        {
            SafeFileHelper_.CopyFolder(folderPair.Source, folderPair.Dest);
            ProgressManager_.Update(folderPair.StartFolderSize);
        }
    }

    private void DeleteLonelyFoldersInDest()
    {
        foreach (var folderPair in LonelyDestFolders)
        {
            SafeFileHelper_.DeleteFolder(folderPair.Dest);
            ProgressManager_.Update(folderPair.EndFolderSize);
        }
    }

    private void DeleteDiffDestFiles()
    {
        foreach (var filePair in DiffDestFiles)
        {
            SafeFileHelper_.DeleteFile(filePair.Dest);
            ProgressManager_.Update(filePair.EndFileSize);
        }
    }

    private void LogInfo(string message) => Log_.LogInformation($"{Source} -> {Dest}: {message}");

    private void LogError(string message) => Log_.LogError($"{Source} -> {Dest}: {message}");

}




