
namespace Storage
{
    public class SafeFileSystemHelper
    {
        public string EditableDrive { get; set; }


        public void DeleteFile(string filename) => SafeFileOperation(filename => FileSystemHelper.RemoveFile(filename), filename);

        public void DeleteFolder(string folder) => SafeFileOperation(folder => FileSystemHelper.DeleteFolder(folder), folder);

        public void CopyFile(string source, string dest, bool overwrite = true) => SafeFileOperation(dest => FileSystemHelper.CopyFile(source, dest, overwrite), dest);

        public void CopyFolder(string source, string dest) => SafeFileOperation(dest => FileSystemHelper.CopyFolder(source, dest), dest);


        private void SafeFileOperation(Action<string> action, string dest)
        {
            try
            {
                string root = Path.GetPathRoot(dest);
                if (!root.Contains(EditableDrive))
                    throw new InvalidOperationException("An attempt was made on changing the content of a drive that was not editable.");

                action(dest);
            }
            catch { throw; }
        }

    }
}
