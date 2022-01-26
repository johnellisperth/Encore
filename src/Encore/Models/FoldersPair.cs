
using Storage;

namespace Encore.Models;
public class FoldersPair
{
    public string Source { get; }
    public string Dest { get; }
    public bool SourceExists { get; }
    public bool DestExists { get; }
    public long StartFolderSize { get; set; }
    public long EndFolderSize { get; set; }
    public FoldersPair(string source, string end, bool determineSize = false)
    {
        Source = source; 
        Dest = end;
        SourceExists = Directory.Exists(Source);
        DestExists = Directory.Exists(Dest);
        if (determineSize)
        {
            StartFolderSize = FileSystemHelper.GetFolderSize(Source);
            EndFolderSize = FileSystemHelper.GetFolderSize(Dest);
        }
    }

    public bool BothExist() => SourceExists && DestExists;
}

