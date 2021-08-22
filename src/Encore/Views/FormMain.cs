using Encore.Helpers;
using Encore.Models;
using Encore.Services;
using System.Windows.Forms;

namespace Encore
{
    public partial class FormMain : Form
    {
        BackupService BackupService_;
        ProgressManager ProgressManager_;
        public FormMain(BackupService backup_service, ProgressManager progress_manager)
        {
            InitializeComponent();
            BackupService_ = backup_service;
            ProgressManager_ = progress_manager;
            comboBoxDrive.DataSource = BackupService_.Drives;
            comboBoxBackup.DataSource = BackupService_.Drives;
        }

        private async void buttonFind_Click(object sender, EventArgs e)
        {
            string error_message = "";
            try
            {
                //var drive = BackupService_.GetDriveSettings(comboBoxDrive.Text);
                // if (drive?.PerformDriveValidation(out error_message) == true)//to move to backupservice
                var result = BackupService_.PerformValidation();
                if (!result.IsValid)
                {
                    MessageBox.Show(result.Message);
                    return;
                }
                progressBar1.Value = 0;
                var progress = new Progress<int>(percent => progressBar1.Value = Math.Min(percent, 100));

                await BackupService_.PerformPreviewOrBackupAsync(new Helpers.ProgressManager(progress, 25), true);
                   
                BackupService_.GetResults(true,
                    out List<FilesPair> diff_files_found,
                    out List<FoldersPair> diff_folders_found,
                    out int diff_files_found_total,
                    out int diff_folders_found_total,
                    out string message);
                  
                // await drive.FindDifferencesBetweenDrivesAsync(new Helpers.ProgressManager(progress,25));
                dataGridView1.DataSource = diff_files_found;// drive.GetSdc()?.Files;
                dataGridView2.DataSource = diff_folders_found;// drive.GetSdc()?.Folders;
                MessageBox.Show(message);
                
            }
            catch (Exception ex)
            {
                error_message = ex.Message;
            }
            if (!string.IsNullOrEmpty(error_message))
                MessageBox.Show(error_message);

            progressBar1.Value = 0;

        }

        private async void buttonEcho_Click(object sender, EventArgs e)
        {
            // var drive = BackupService_.GetDriveSettings(comboBoxDrive.Text);
            // if (drive is null)
            //     return;
            var result = BackupService_.PerformValidation();
            if (!result.IsValid)
            {
                MessageBox.Show(result.Message);
                return;
            }

                progressBar1.Value = 0;
            var progress = new Progress<int>(percent =>
            {
                progressBar1.Value = Math.Min(percent, 100);

            });
            await BackupService_.PerformPreviewOrBackupAsync(new Helpers.ProgressManager(progress), false);
            BackupService_.GetResults(true,
                         out List<FilesPair> diff_files_found,
                         out List<FoldersPair> diff_folders_found,
                         out int diff_files_found_total,
                         out int diff_folders_found_total,
                         out string message);
          
            MessageBox.Show(message);
            progressBar1.Value = 0;
            
        }

        private void comboBoxDrive_SelectedIndexChanged(object sender, EventArgs e)
        {
            BackupService_.Source = comboBoxDrive.Text;
        }

        private void comboBoxBackup_SelectedIndexChanged(object sender, EventArgs e)
        {
            BackupService_.Dest = comboBoxBackup.Text;
        }
    }
}
