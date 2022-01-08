
using Storage;

namespace Encore.Models;
public class FilesPair
{
    public string Start { get; set; }
    public string End { get; set; }
    public bool SourceExists { get; set; }
    public bool DestExists { get; set; }
    public long StartFileSize { get; set; }
    public long EndFileSize { get; set; }
    public bool IsSameSize => StartFileSize == EndFileSize;
    public FilesPair(string start, string end)
    {
        Start = start;
        End = end;
        SourceExists = File.Exists(Start);
        DestExists = File.Exists(End);
        StartFileSize = FileSystemHelper.GetFileSize(Start);
        EndFileSize = FileSystemHelper.GetFileSize(End);
    }

    public bool IsSame(bool check_file_size_only, long perform_contents_equal_test_size_cutoff)
    {
        return FileSystemHelper.DoFilesMatch(Start, End, check_file_size_only, perform_contents_equal_test_size_cutoff);
    }
}

