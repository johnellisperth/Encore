
namespace Encore.Models
{
    public class FoldersPair
    {
        public string Start { get; }
        public string End { get; }

        public FoldersPair(string start, string end)
        {
            Start = start; 
            End = end;
        }

       // public void DeleteIfLonelyDest() => FileSystemHelper.RemoveFolder(End);

       // public void CopyLonelySourceToDest() => FileSystemHelper.CopyFolder(Start, End);

    }
}
