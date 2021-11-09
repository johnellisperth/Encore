using Microsoft.Extensions.Logging;
using Encore.Helpers;
using Encore.Models;
using Storage;
using Encore.Validation;

namespace Encore.Services
{
    public class BackupService 
    {
        private readonly SourceDestComparison LastSourceDestComparison_;
        private readonly SourceDestValidator Validator_;
        public static string[] Drives { get => UserFileSystem.PCDriveList; }
        public string Source { get; set; }
        public string Dest { get; set; }
        private readonly ILogger Log_;

        public BackupService(ILogger<BackupService> logger, SourceDestComparison source_dest_comparison, SourceDestValidator validator)
        {
            Log_ = logger;
            LastSourceDestComparison_ = source_dest_comparison;
            Validator_ = validator;
        }

        public void GetResults(bool preview, out List<FilesPair> diff_files_found,out List<FoldersPair> diff_folders_found, out string message)
        {
            message = "";
            diff_files_found = LastSourceDestComparison_.DiffSourceFiles.Concat(LastSourceDestComparison_.DiffDestFiles).ToList();
            diff_folders_found = LastSourceDestComparison_.LonelySourceFolders.Concat(LastSourceDestComparison_.LonelyDestFolders).ToList();
            var diff_files_found_total = diff_files_found?.Count ?? 0;
            var diff_folders_found_total = diff_folders_found?.Count ?? 0;
            if (LastSourceDestComparison_ is null) return;
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
            LastSourceDestComparison_.SetSourceDest(Source, Dest);
            await Task.Run(() => LastSourceDestComparison_.PerformEcho(preview));
            Log_.LogInformation($"Finsihed performing backup {preview_string}of {Source} onto {Dest}");
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
    }
}
