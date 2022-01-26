using Encore.Helpers;
using Encore.Models;
using Encore.Services;

namespace Encore;
public partial class FormMain : Form
{
    BackupService BackupService_;
    ProgressManager ProgressManager_;
    public FormMain(BackupService backup_service, ProgressManager progress_manager)
    {
        InitializeComponent();
        BackupService_ = backup_service;
        comboBoxDrive.DataSource = BackupService_.AvailableSourceDriveList;
        comboBoxBackup.DataSource = BackupService_.AvailableDestDriveList;
        ProgressManager_ = progress_manager;
        UpdateButtonStates();
    }

    private async void buttonCompare_Click(object sender, EventArgs e)
    {
        await Compare();
    }

    private async void buttonBackup_Click(object sender, EventArgs e)
    {
        await Backup();
    }

    private void comboBoxDrive_SelectedIndexChanged(object sender, EventArgs e)
    {
        Models.DriveInfo sourceDrive = (Models.DriveInfo)comboBoxDrive.SelectedItem;
        BackupService_.Source = sourceDrive.Drive;
        UpdateButtonStates();
    }

    private void comboBoxBackup_SelectedIndexChanged(object sender, EventArgs e)
    {
        Models.DriveInfo destDrive = (Models.DriveInfo)comboBoxBackup.SelectedItem;
        BackupService_.Dest = destDrive.Drive;
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        bool sourceDestNotEqual = SourceDestNotEqual();
        buttonCompare.Enabled = sourceDestNotEqual;
        buttonBackup.Enabled = sourceDestNotEqual;
    }

    private bool SourceDestNotEqual()
    { 
        Models.DriveInfo sourceDrive = (Models.DriveInfo) comboBoxDrive.SelectedItem;
        Models.DriveInfo destDrive = (Models.DriveInfo)comboBoxBackup.SelectedItem;
        return !string.IsNullOrEmpty(sourceDrive.Drive) && !string.IsNullOrEmpty(destDrive.Drive)  && 
        !string.Equals(sourceDrive.Drive, destDrive.Drive, StringComparison.CurrentCultureIgnoreCase);
    }

    private async Task Compare()
    {
        if (!PerformValidate()) return;
        try
        {
            ProgressManager_.Progress = new Progress<int>(percent => progressBar1.Value = Math.Min(percent, 100));
            await BackupService_.Compare();
        }
        catch (Exception ex)
        {
            NotCompleted(ex.Message);
            return;
        }
        Completed(true);
    }


    private async Task Backup()
    {
        if (!PerformValidate()) return;
        try
        {
            ProgressManager_.Progress = new Progress<int>(percent => progressBar1.Value = Math.Min(percent, 100));
            await BackupService_.Backup();
        }
        catch (Exception ex)
        {
            NotCompleted(ex.Message);
            return;
        }
        Completed(false);
    }
    /*void callback( long totalFileSize, long totalBytesTransferred, IProgress<int> progress)
    {
        fileProgress = totalBytesTransferred;
        totalProgress = totalFileSize;
        progress.Report(Convert.ToInt32(fileProgress / totalProgress));
        //return CopyFileEx.CopyFileCallbackAction.Continue;
    }*/


    private void NotCompleted(string message)
    {
        ProgressManager_.Reset();
        dataGridView1.DataSource = null;
        dataGridView2.DataSource = null;
        if (!string.IsNullOrEmpty(message))
            MessageBox.Show(message);
    }

    private void Completed(bool preview)
    {
        ProgressManager_.Finish();
        BackupService_.GetResults(preview, out List<FilesPair> diff_files_found, out List<FoldersPair> diff_folders_found, out string message);
        dataGridView1.DataSource = diff_files_found;
        dataGridView2.DataSource = diff_folders_found;
        MessageBox.Show(message);
        ProgressManager_.Reset();
    }

    private bool PerformValidate()
    {
        var result = BackupService_.PerformValidation();
        if (!result.IsValid)
        {
            MessageBox.Show(result.Message);
            return false;
        }
        return true;
    }

   
}

