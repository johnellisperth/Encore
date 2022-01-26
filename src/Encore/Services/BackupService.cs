using Microsoft.Extensions.Logging;
using Encore.Helpers;
using Encore.Models;
using Storage;
using Encore.Validation;

namespace Encore.Services;
public class BackupService 
{
    private readonly EncoreFileManager EncoreFileManager_;
    private readonly SourceDestValidator Validator_;

    public static Models.DriveInfo[] DrivesInfo
    {
        get => UserFileSystem.PCDetailedDriveList.Select(x =>
            new Models.DriveInfo() { Drive = x.Item1, VolumeName = x.Item2 }).ToArray();
    }

    public List<Models.DriveInfo> AvailableSourceDriveList = new() { new Models.DriveInfo()};
    public List<Models.DriveInfo> AvailableDestDriveList = new() { new Models.DriveInfo() };
    public string Source { get; set; } = string.Empty;
    public string Dest { get; set; } = string.Empty;
    public  ProgressManager ProgressManager_;

    private readonly ILogger Log_;

    public BackupService(ILogger<BackupService> logger, EncoreFileManager encoreFileManager, SourceDestValidator validator, ProgressManager progressManager)
    {
        Log_ = logger;
        EncoreFileManager_ = encoreFileManager;
        Validator_ = validator;
        ProgressManager_ = progressManager;
        AvailableSourceDriveList.AddRange(DrivesInfo);
        AvailableDestDriveList.AddRange(DrivesInfo);
    }

    public void GetResults(bool preview, out List<FilesPair> diffFiles,out List<FoldersPair> diffFolders, out string message)
    {
        message = "";
        diffFiles = EncoreFileManager_.DiffSourceFiles.Concat(EncoreFileManager_.DiffDestFiles).ToList();
        diffFolders = EncoreFileManager_.LonelySourceFolders.Concat(EncoreFileManager_.LonelyDestFolders).ToList();
      
        if (EncoreFileManager_ is null) return;
        var diffFilesCount = diffFiles?.Count ?? 0;
        var diffFoldersCount = diffFolders?.Count ?? 0;
        message = preview ? GetCompareMessage(diffFilesCount, diffFoldersCount) : GetBackupMessage(diffFilesCount, diffFoldersCount);

        Log_.LogInformation(message);
    }

    private string GetBackupMessage(int diffFilesCount, int diffFoldersCount) =>
        $"Copied {diffFilesCount} different files. Copied {diffFoldersCount} different folders.";
    
    private string GetCompareMessage(int diffFilesCount, int diffFoldersCount) =>
         (diffFilesCount == 0 && diffFoldersCount == 0) ?
                "No differences were found!" :
                $"Found {diffFilesCount} differences in files. Found {diffFoldersCount} differences in folders.";
    
    public ValidationResult PerformValidation()
    {
        var result = Validator_.IsSourceDestValid(Source, Dest);
        if (!result.IsValid)
            Log_.LogInformation(result.Message);
        return result;
    }

    public async Task Backup()
    {
        Log_.LogInformation($"Begin backing up of {Source} onto {Dest}");
        EncoreFileManager_.SetSourceDest(Source, Dest);
        await Task.Run(() => EncoreFileManager_.PerformEcho(false));
        Log_.LogInformation($"Finsihed backing up {Source} onto {Dest}");
    }

    public async Task Compare()
    {
        Log_.LogInformation($"Begin comparing betwenn {Source} and {Dest}");
        EncoreFileManager_.SetSourceDest(Source, Dest);
        await Task.Run(() => EncoreFileManager_.PerformEcho(true));
        Log_.LogInformation($"Finsihed comparing betwenn {Source} and {Dest}");
    }

    public async Task PerformPreviewOrBackupAsync(bool preview)
    {
        string previewString = preview ? "preview " : "";
        Log_.LogInformation($"Performing backup {previewString}of {Source} onto {Dest}");
        EncoreFileManager_.SetSourceDest(Source, Dest);
        await Task.Run(() => EncoreFileManager_.PerformEcho(preview));
        Log_.LogInformation($"Finsihed performing backup {previewString}of {Source} onto {Dest}");
    }
    /*
        foreach (var drive in UserFileSystem.PCDriveList)
            DriveList.Add(new DriveSettings()
            {
                DriveLetter = $"{drive}",//$"{drive.Key}:\\",
                IsBitlockable = false,//drive.Value,
                Password = "NA"//drive.Value ? "": "NA",
            });
        */

    /*  public DriveSettings GetDriveSettings(string drive) =>  DriveList.FirstOrDefault(d => 
        string.Equals(drive, d.DriveLetter) && 
        !string.IsNullOrEmpty(d.BackupDrive) && 
        !string.Equals(d.DriveLetter, d.BackupDrive));//backup drive must be different and set
    */


    /*public void UpdateSourceList()
    {
        AvailableSourceDriveList = new() { new Models.DriveInfo() };
        AvailableSourceDriveList.AddRange(DrivesInfo);
        if (!string.IsNullOrEmpty(Dest))
            AvailableSourceDriveList.RemoveAll(dd=>dd.Drive ==  Dest);
    }

    public void UpdateDestList()
    {
        AvailableDestDriveList = new() { new Models.DriveInfo() };
        AvailableDestDriveList.AddRange(DrivesInfo);
        if (!string.IsNullOrEmpty(Source))
            AvailableDestDriveList.RemoveAll(dd => dd.Drive == Source);
    }*/
}

