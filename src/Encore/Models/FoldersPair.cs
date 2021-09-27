
using System.IO;

namespace Encore.Models
{
    public class FoldersPair
    {
        public string Start { get; }
        public string End { get; }
        public bool SourceExists { get; set; }
        public bool DestExists { get; set; }

        public FoldersPair(string start, string end)
        {
            Start = start; 
            End = end;
            SourceExists = Directory.Exists(Start);
            DestExists = Directory.Exists(End);
        }

        public bool BothExist() => SourceExists && DestExists;


    }
}
