using Microsoft.Extensions.Logging;
using Encore.Helpers;
using Encore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RemoveDuplicateFiles.Services;
using Storage.Helpers;

namespace Encore.Services
{
    public class BackupService 
    {
        public List<DriveSettings> DriveList { get; set; } = new List<DriveSettings>();
        public SourceDestComparison LastSourceDestComparison { get; private set; }

        public string Source { get; set; }
        public string Dest { get; set; }
        private readonly ILogger Log_;
        public BackupService(ILogger<BackupService> logger)
        {
            Log_ = logger;
            
            foreach (var drive in UserFileSystem.PCDriveList)
                DriveList.Add(new DriveSettings()
                {
                    DriveLetter = $"{drive}",//$"{drive.Key}:\\",
                    IsBitlockable = false,//drive.Value,
                    Password = "NA"//drive.Value ? "": "NA",
                });
        }

        public List<string> Drives => DriveList.Select(d => d.DriveLetter).ToList();

        public DriveSettings GetDriveSettings(string drive) =>   DriveList.FirstOrDefault(d => 
        string.Equals(drive, d.DriveLetter) && 
        !string.IsNullOrEmpty(d.BackupDrive) && 
        !string.Equals(d.DriveLetter, d.BackupDrive));//backup drive must be different and set

        public void GetResults(bool preview,
             out List<FilesPair> diff_files_found,
            out List<FoldersPair> diff_folders_found,
            out int diff_files_found_total,
            out int diff_folders_found_total,
            out string message)
        {
            message = "";
            diff_files_found = LastSourceDestComparison?.DiffSourceFiles.Concat(LastSourceDestComparison?.DiffDestFiles).ToList();
            diff_folders_found = LastSourceDestComparison?.LonelySourceFolders.Concat(LastSourceDestComparison?.LonelyDestFolders).ToList();
            diff_files_found_total = diff_files_found?.Count ?? 0;
            diff_folders_found_total = diff_folders_found?.Count ?? 0;
            if (LastSourceDestComparison is null) return;
            if (!preview)
                message = $"Copied {diff_files_found_total} different files. Copied { diff_folders_found_total} different folders.";
            else
                message = (diff_files_found_total == 0 && diff_folders_found_total == 0) ?
                    "No differences were found!" :
                    $"Found {diff_files_found_total} differences in files. Found { diff_folders_found_total} differences in folders.";
            Log_.LogInformation(message);
        }

        public async Task PerformPreviewOrBackupAsync(ProgressManager progress_manager, bool preview)
        {
            string preview_string = preview ? "preview " : "";
            Log_.LogInformation($"Performing backup {preview_string}of {Source} onto {Dest}");
            LastSourceDestComparison = new (Source, Dest, progress_manager, Log_);
            await Task.Run(() => LastSourceDestComparison.PerformEcho(preview));
            Log_.LogInformation($"Finsihed performing backup {preview_string}of {Source} onto {Dest}");
        }
    }
}
