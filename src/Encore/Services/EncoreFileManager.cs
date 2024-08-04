using Encore.Helpers;
using Encore.Models;
using Encore.Railway;
using Microsoft.Extensions.Logging;
using Storage;

namespace Encore.Services;

public class EncoreFileManager(ProgressManager progressManager, ILogger<EncoreFileManager> logger,
        SafeFileSystemHelper safeFileHelper, AppSettings appSettings)
{
    public string Source { get; private set; } = string.Empty;
    public string Dest { get; private set; } = string.Empty;
    public List<FoldersPair> LonelySourceFolders { get; private set; } = [];
    public List<FoldersPair> LonelyDestFolders { get; private set; } = [];
    public List<FilesPair> DiffSourceFiles { get; private set; } = [];
    public List<FilesPair> DiffDestFiles { get; private set; } = [];

    readonly SafeFileSystemHelper SafeFileHelper_ = safeFileHelper;
    readonly ILogger Log_ = logger;
    readonly ProgressManager ProgressManager_ = progressManager;
    readonly AppSettings AppSettings_ = appSettings;

    public void SetSourceDest(string source, string dest)
    {
        Source = source;
        Dest = dest;
        SafeFileHelper_.EditableDrive = dest;
    }

    public bool PerformEcho()
    {
        try
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
            

            ProgressManager_.Finish();
        }
        catch (Exception ex)
        {
            LogError($"Exception raised:{ex.Message}");
            return false;
        }
        return true;
    }

    public void PerformPreviewComparisonAsync()
    {
        LogInfo($"Performing preview.");
        var result = ResultExtensions.TryAsync(() => DetermineLonelyDestFoldersAsync(false)
            .OnSuccess(result => ProgressManager_.UpdateProgressBar(25))
            .OnSuccess(result => DetermineLonelySourceFoldersAsync(false))
            .OnSuccess(result => ProgressManager_.UpdateProgressBar(50))
            .OnSuccess(result => DetermineDiffDestFilesAsync())
            .OnSuccess(result => ProgressManager_.UpdateProgressBar(75))
            .OnSuccess(result => DetermineDiffSourceFilesAsync())
            );
    }

    public void PerformPreviewComparison()
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

    void DetermineDiffSourceFiles()
    {
        LogInfo($"Determine all source files that are different from matching dest files");
        DiffSourceFiles = new();
        foreach (var sourceFile in FileCompareHelper.GetAllFiles(Source).Where(f => !OnExclusionList(f)))
        {
          
            FilesPair fp = new (sourceFile, FileCompareHelper.DiffDriveFilename(Dest, sourceFile));
            if (!fp.IsSameSize)///IsSame(true, 2000000000))
                DiffSourceFiles.Add(fp);
        }
    }

    async Task<Result<int>> DetermineDiffSourceFilesAsync()
    {
        LogInfo($"Determine all source files that are different from matching dest files");
        DiffSourceFiles = [];
        var files = await FileCompareHelper.GetAllFilesAsync(Source);
        
        foreach (var sourceFile in files.Where(f => !OnExclusionList(f)))
        {

            FilesPair fp = new(sourceFile, FileCompareHelper.DiffDriveFilename(Dest, sourceFile));
            if (!fp.IsSameSize)///IsSame(true, 2000000000))
                DiffSourceFiles.Add(fp);
        }
        return Result<int>.Success(42);
    }


    void DetermineDiffDestFiles()
    {
        LogInfo("Determine all dest files that are different from matching source files");

        DiffDestFiles = new();
       // var lonelyDestFolders = LonelyDestFolders.Select(fp => fp.Dest).ToArray();
        foreach (var destFile in FileCompareHelper.GetAllFiles(Dest).Where(f=>!OnExclusionList(f)))
        {
            FilesPair filePair = new (FileCompareHelper.DiffDriveFilename(Source, destFile), destFile);
            if (!filePair.IsSameSize)//IsSame(true, 2000000000))
                DiffDestFiles.Add(filePair);
        }
    }

    async Task<Result<int>> DetermineDiffDestFilesAsync()
    {
        LogInfo("Determine all dest files that are different from matching source files");

        DiffDestFiles = new();
        var lonelyDestFolders = LonelyDestFolders.Select(fp => fp.Dest).ToArray();
        var files = await FileCompareHelper.GetAllFilesAsync(Dest);
        foreach (var destFile in files.Where(f => !OnExclusionList(f)))
        {
            FilesPair filePair = new(FileCompareHelper.DiffDriveFilename(Source, destFile), destFile);
            if (!filePair.IsSameSize)//IsSame(true, 2000000000))
                DiffDestFiles.Add(filePair);
        }
        return Result<int>.Success(42);
    }


    void DetermineLonelyDestFolders(bool determineFolderSize)
    {
        LogInfo("Determine all dest folders that have no matching source folders.");

        LonelyDestFolders = new();
           
        foreach (var destFolder in FileCompareHelper.GetAllFolders(Dest).Where(f => !OnExclusionList(f)))
        {
            FoldersPair folderPair = new (FileCompareHelper.DiffDriveFilename(Source, destFolder), destFolder,determineFolderSize);
                
            if (!folderPair.BothExist())///same as saying if the Source doesnt Exists
                LonelyDestFolders.Add(folderPair);
        }
    }


    public async Task<Result<int>> DetermineLonelyDestFoldersAsync(bool determineFolderSize)
    {
        var directories = await FileCompareHelper.GetAllFoldersAsync(Dest);
        foreach (var destFolder in directories.Where(f => !OnExclusionList(f)))
        {
            FoldersPair folderPair = new(FileCompareHelper.DiffDriveFilename(Source, destFolder), destFolder, determineFolderSize);

            if (!folderPair.BothExist())///same as saying if the Source doesnt Exists
                LonelyDestFolders.Add(folderPair);
        }

        return Result<int>.Success(42); // Or return Result<int>.Failure("Some error")
    }


    bool OnExclusionList(string folder) => folder.Contains("\\PC") || folder.Contains("\\Videos") || folder.Contains("\\Games");




    void DetermineLonelySourceFolders(bool determineFolderSize)
    {
        LogInfo("Determine all source folders that have no matching dest folders.");

        LonelySourceFolders = [];
        foreach (var sourceFolder in FileCompareHelper.GetAllFolders(Source).Where(f=>!OnExclusionList(f)))
        {
            FoldersPair folderPair = new (sourceFolder, FileCompareHelper.DiffDriveFilename(Dest, sourceFolder), determineFolderSize);
            if (!folderPair.BothExist())
                LonelySourceFolders.Add(folderPair);
        }
    }

    public async Task<Result<int>> DetermineLonelySourceFoldersAsync(bool determineFolderSize)
    {
        var directories = await FileCompareHelper.GetAllFoldersAsync(Source);
        LogInfo("Determine all source folders that have no matching dest folders.");

        LonelySourceFolders = [];
        foreach (var sourceFolder in directories.Where(f => !OnExclusionList(f)))
        {
            FoldersPair folderPair = new(sourceFolder, FileCompareHelper.DiffDriveFilename(Dest, sourceFolder), determineFolderSize);
            if (!folderPair.BothExist())
                LonelySourceFolders.Add(folderPair);
        }

        return Result<int>.Success(42); // Or return Result<int>.Failure("Some error")
    }


    void CopySourceFilesToDest()
    {
        foreach (var filePair in DiffSourceFiles)
        {
            SafeFileHelper_.CopyFile(filePair.Source, filePair.Dest, true);
            ProgressManager_.Update(filePair.StartFileSize);
        }
    }

    void CopySourceFoldersToDest()
    {
        foreach (var folderPair in LonelySourceFolders)
        {
            SafeFileHelper_.CopyFolder(folderPair.Source, folderPair.Dest);
            ProgressManager_.Update(folderPair.StartFolderSize);
        }
    }

    void DeleteLonelyFoldersInDest()
    {
        foreach (var folderPair in LonelyDestFolders)
        {
            SafeFileHelper_.DeleteFolder(folderPair.Dest);
            ProgressManager_.Update(folderPair.EndFolderSize);
        }
    }

    void DeleteDiffDestFiles()
    {
        foreach (var filePair in DiffDestFiles)
        {
            SafeFileHelper_.DeleteFile(filePair.Dest);
            ProgressManager_.Update(filePair.EndFileSize);
        }
    }

    void LogInfo(string message) => Log_.LogInformation($"{Source} -> {Dest}: {message}");

    void LogError(string message) => Log_.LogError($"{Source} -> {Dest}: {message}");

}




