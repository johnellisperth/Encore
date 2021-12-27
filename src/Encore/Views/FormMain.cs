using Encore.Helpers;
using Encore.Models;
using Encore.Services;

namespace Encore;
public partial class FormMain : Form
{
    BackupService BackupService_;
    ProgressManager ProgressManager_;
    public FormMain(BackupService backup_service,  ProgressManager progress_manager)
    {
        InitializeComponent();
      
        BackupService_ = backup_service;
       
        comboBoxDrive.DataSource =  BackupService.Drives.ToArray();
        comboBoxBackup.DataSource = BackupService.Drives.ToArray();
        ProgressManager_ = progress_manager;
    }

    private async void buttonFind_Click(object sender, EventArgs e)
    {
        await PerformAction(true);
    }
    private async void buttonEcho_Click(object sender, EventArgs e)
    {
        await PerformAction(false);
    }

    private void comboBoxDrive_SelectedIndexChanged(object sender, EventArgs e)
    {
        BackupService_.Source = comboBoxDrive.Text;
    }

    private void comboBoxBackup_SelectedIndexChanged(object sender, EventArgs e)
    {
        BackupService_.Dest = comboBoxBackup.Text;
    }

    private async Task PerformAction(bool preview)
    {
        if (!Validate()) return;
        try
        {
            ProgressManager_.Increment = 25;
            //ProgressManager_ = new ProgressManager();
            ProgressManager_.Progress = new Progress<int>(percent =>
            {
                progressBar1.Value = percent;
            });

            
            //var progress = new Progress<int>(percent => progressBar1.Value = Math.Min(percent, 100));

            await BackupService_.PerformPreviewOrBackupAsync(preview);
        }
        catch (Exception ex)
        {
            NotCompleted(ex.Message);
            return;
        }
        Completed(preview);
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

    private bool Validate()
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

