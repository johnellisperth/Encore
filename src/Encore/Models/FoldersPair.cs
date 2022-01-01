
using Storage;

namespace Encore.Models;
public class FoldersPair
{
    public string Start { get; }
    public string End { get; }
    public bool SourceExists { get; }
    public bool DestExists { get; }
    public long StartFolderSize { get; set; }
    public long EndFolderSize { get; set; }
    public FoldersPair(string start, string end, bool determine_sizes = false)
    {
        Start = start; 
        End = end;
        SourceExists = Directory.Exists(Start);
        DestExists = Directory.Exists(End);
        if (determine_sizes)
        {
            StartFolderSize = FileSystemHelper.GetFolderSize(Start);
            EndFolderSize = FileSystemHelper.GetFolderSize(End);
        }
    }

    public bool BothExist() => SourceExists && DestExists;
}

