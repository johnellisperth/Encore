using Encore.Helpers;
using System.IO;

namespace Encore.Models
{
    public class FilesPair
    {
        public string Start { get; set; }
        public string End { get; set; }
        public bool SourceExists { get; set; }
        public bool DestExists { get; set; }

        public FilesPair(string start, string end)
        {
            Start = start;
            End = end;
            SourceExists = File.Exists(Start);
            DestExists = File.Exists(End);
        }

       // public void DeleteDestIfDiff() => FileSystemHelper.RemoveFile(End);

       // public void CopySourceIfDiff() => File.Copy(Start, End, true);

       
    }
}
