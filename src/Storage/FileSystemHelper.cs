

namespace Storage
{
    public static class FileSystemHelper
    {
        public static bool DoFilesMatch(string full_source_filename, string full_dest_filename, bool check_filesize_only = false)
        {
            if (!File.Exists(full_source_filename) || !File.Exists(full_dest_filename))
                return false;
            FileInfo fi1 = new FileInfo(full_source_filename);
            FileInfo fi2 = new FileInfo(full_dest_filename);
            return FileCompareHelper.AreFilesEqual(fi1, fi2, check_filesize_only);
        }

        public static void RemoveFile(string filename)///, bool use_recycle_bin = false)
        {
            if (!File.Exists(filename))
                return;
            File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);//remove readonly first other acces denied
            File.Delete(filename);
        }

        public static void CopyFile(string source_file, string dest_file, bool overwrite = true)
        {
            //do we need dest to readwrite in case its readonly?
            File.Copy(source_file, dest_file, overwrite);
        }

        public static void DeleteFolder(string folder)//, bool use_recycle_bin = false)
        {
            var root = Path.GetPathRoot(folder);
            if (!Directory.Exists(folder))
                return;
            var di = new DirectoryInfo(folder);
            di.Attributes &= ~FileAttributes.ReadOnly;
            foreach (var file in di.GetFiles("*", SearchOption.AllDirectories))
                file.Attributes &= ~FileAttributes.ReadOnly;
            Directory.Delete(folder, true);
        }

        public static bool CopyFolder(string source, string dest, bool fail_on_dest_existing = true)
        {
            if (fail_on_dest_existing && Directory.Exists(dest)) return false;
            //if (check_source_is_source)
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(source, dest);
            return true;
        }
    }
}