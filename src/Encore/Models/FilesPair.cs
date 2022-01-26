
using Storage;

namespace Encore.Models;
public class FilesPair
{
    public string Source { get; }
    public string Dest { get; }
    public bool SourceExists { get; }
    public bool DestExists { get; }
    public long StartFileSize { get; }
    public long EndFileSize { get; }
    public bool IsSameSize => StartFileSize == EndFileSize;
    public FilesPair(string source, string dest)
    {
        Source = source;
        Dest = dest;
        SourceExists = File.Exists(Source);
        DestExists = File.Exists(Dest);
        StartFileSize = FileSystemHelper.GetFileSize(Source);
        EndFileSize = FileSystemHelper.GetFileSize(Dest);
    }

    public bool IsSame(bool compareOnlyFileSize, long perform_contents_equal_test_size_cutoff)
    {
        return FileSystemHelper.DoFilesMatch(Source, Dest, compareOnlyFileSize, perform_contents_equal_test_size_cutoff);
    }
}

