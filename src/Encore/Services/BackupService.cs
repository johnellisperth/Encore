﻿using Microsoft.Extensions.Logging;
using Encore.Helpers;
using Encore.Models;
using Storage;
using Encore.Validation;

namespace Encore.Services
{
    public class BackupService 
    {
        public List<DriveSettings> DriveList { get; set; } = new List<DriveSettings>();
        public SourceDestComparison LastSourceDestComparison { get; private set; }
        private readonly SourceDestValidator Validator_;
        public string Source { get; set; }
        public string Dest { get; set; }
        private readonly ILogger Log_;
        public BackupService(ILogger<BackupService> logger, SourceDestComparison source_dest_comparison, SourceDestValidator validator)
        {
            Log_ = logger;
            LastSourceDestComparison = source_dest_comparison;
            Validator_ = validator;
            foreach (var drive in UserFileSystem.PCDriveList)
                DriveList.Add(new DriveSettings()
                {
                    DriveLetter = $"{drive}",//$"{drive.Key}:\\",
                    IsBitlockable = false,//drive.Value,
                    Password = "NA"//drive.Value ? "": "NA",
                });
        }

        public List<string> Drives => DriveList.Select(d => d.DriveLetter).ToList();

      /*  public DriveSettings GetDriveSettings(string drive) =>  DriveList.FirstOrDefault(d => 
        string.Equals(drive, d.DriveLetter) && 
        !string.IsNullOrEmpty(d.BackupDrive) && 
        !string.Equals(d.DriveLetter, d.BackupDrive));//backup drive must be different and set
      */
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

        public ValidationResult PerformValidation()
        {
            var result = Validator_.IsSourceDestValid(Source, Dest);
            if (!result.IsValid)
                Log_.LogInformation(result.Message);

            return result;
        }
     
        public async Task PerformPreviewOrBackupAsync(ProgressManager progress_manager, bool preview)
        {
            string preview_string = preview ? "preview " : "";
            Log_.LogInformation($"Performing backup {preview_string}of {Source} onto {Dest}");
            LastSourceDestComparison.SetSourceDest(Source, Dest);
            await Task.Run(() => LastSourceDestComparison.PerformEcho(preview));
            Log_.LogInformation($"Finsihed performing backup {preview_string}of {Source} onto {Dest}");
        }
    }
}