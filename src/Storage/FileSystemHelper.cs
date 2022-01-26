namespace Storage;
public static class FileSystemHelper
{
    public static long GetFileSize(string filename)
    {
        if (!File.Exists(filename))
            return -1;
        FileInfo fi1 = new FileInfo(filename);
        return fi1.Length;
    }

    public static long GetFolderSize(string folder)
    {
        long folderSize = 0;
        if (!Directory.Exists(folder))
            return folderSize; 
        DirectoryInfo di = new DirectoryInfo(folder);
        FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);
        for (int i = 0; i < fi.Count(); i++)
        {
            folderSize += fi[i].Length;
        }
        return folderSize;
    }

    public static bool DoFilesMatch(string fullSourceFilename, string fullDestFilename, bool compareOnlyFileSize = false, long performContentsEqualTestSizeCutoff = long.MaxValue)
    {
        if (!File.Exists(fullSourceFilename) || !File.Exists(fullDestFilename))
            return false;
        FileInfo fi1 = new FileInfo(fullSourceFilename);
        FileInfo fi2 = new FileInfo(fullDestFilename);
        return FileCompareHelper.AreFilesEqual(fi1, fi2, compareOnlyFileSize, performContentsEqualTestSizeCutoff);
    }

    public static void RemoveFile(string filename)///, bool use_recycle_bin = false)
    {
        if (!File.Exists(filename))
            return;
        File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);//remove readonly first other acces denied
        File.Delete(filename);
    }

    public static void CopyFile(string sourceFile, string destFile, bool overwrite = true)
    {
        //do we need dest to readwrite in case its readonly?
        File.Copy(sourceFile, destFile, overwrite);
    }

    public static void DeleteFolder(string folder)//, bool use_recycle_bin = false)
    {
        //var root = Path.GetPathRoot(folder);
        if (!Directory.Exists(folder))
            return;
        var di = new DirectoryInfo(folder);
        di.Attributes &= ~FileAttributes.ReadOnly;
        foreach (var file in di.GetFiles("*", SearchOption.AllDirectories))
            file.Attributes &= ~FileAttributes.ReadOnly;
        Directory.Delete(folder, true);
    }

    public static bool CopyFolder(string source, string dest, bool failOnDestExisting = true)
    {
        if (failOnDestExisting && Directory.Exists(dest)) return false;
        Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(source, dest);
        return true;
    }
}
