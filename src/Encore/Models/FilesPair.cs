using Encore.Helpers;
using Storage;
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

        public bool IsSame()
        {
            return FileSystemHelper.DoFilesMatch(Start, End, true);
        }
    }
}
