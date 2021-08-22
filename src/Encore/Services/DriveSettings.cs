
using Encore.Helpers;
using System.ComponentModel;

namespace Encore.Services
{
    public class DriveSettings : INotifyPropertyChanged
    {
        public string DriveLetter { get; set; }
        public bool IsBitlockable { get; set; }
        public string Password { get; set; } 

        [DisplayName("Backup Drive")]
        [Description("Drive to select for back up")]
        [DefaultValue("")]
        [TypeConverter(typeof(FormatStringConverter))]
        public string BackupDrive { get; set; }
        public bool BackupEnabled { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool PerformDriveValidation(out string error_message)
        {
            error_message = default;
         

            if (string.IsNullOrEmpty(BackupDrive))
            {
                error_message = "Backup drive not selected";
                return false;
            }
            DriveSettings backup_drive = new DriveSettings() { DriveLetter = BackupDrive };
            //unlock drives if required

           /* if (IsPasswordSet(Password) && !BitLockerHelper.UnlockDrive(DriveLetter, Password))
            {
                error_message = "Failed to unlock drive:" + DriveLetter;
                return false;
            }

            if (IsPasswordSet(backup_drive.Password) && !BitLockerHelper.UnlockDrive(backup_drive.DriveLetter, backup_drive.Password))
            {
                error_message = "Failed to unlock drive:" + backup_drive.DriveLetter;
                return false;
            }*/

            //if (!preview)

            return true;
        }
 
        bool IsPasswordSet(string password) => !string.IsNullOrEmpty(password) && password != "NA";
   
    }
}
