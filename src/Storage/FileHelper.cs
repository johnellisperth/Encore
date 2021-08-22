using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Storage.Helpers
{
    public static class FileCompareHelper
    {
        public static List<string> GetAllFolders(string folder) => Directory.EnumerateDirectories(folder, "*", new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true

        }).ToList();
        public static List<string> GetAllFiles(string folder) => Directory.EnumerateFiles(folder, "*.*", new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true
        }).ToList();
        public static string NoDriveFilename(string filename) => filename.Remove(0, 2);
        public static string DiffDriveFilename(string drive, string filename) => drive + NoDriveFilename(filename);

        public static bool AreFilesEqual(FileInfo fi1, FileInfo fi2, bool check_filesize_only = false)
        {
            if (fi1.Length != fi2.Length) return false;
            if (check_filesize_only) return true;
            if (fi1.Length < 2000000000) return AreFileContentsEqual(fi1, fi2);
            foreach (var block in ReadChunks(fi1.FullName))
                foreach (var blockb in ReadChunks(fi2.FullName))
                    if (!block.SequenceEqual(blockb)) return false;
            return true;
        }
        public static bool AreFileContentsEqual(FileInfo fi1, FileInfo fi2) => /*fi1.Length == fi2.Length &&*/
            (fi1.Length == 0 || File.ReadAllBytes(fi1.FullName).SequenceEqual(File.ReadAllBytes(fi2.FullName)));



        /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/54eb346f-f979-49fb-aa2d-44dddad066bd/how-to-read-file-in-chunks-with-for-loop?forum=netfxbcl

        public static IEnumerable<byte[]> ReadChunks(string path)
        {

            // FileStream FS = new FileStream(path, FileMode.Open, FileAccess.Read);
            using (var FS = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long FSBytes = FS.Length;

                int ChunkSize = 2 << 8;

                byte[] B = new byte[ChunkSize];

                int Pos;

                for (Pos = 0; Pos < (FSBytes - ChunkSize); Pos += ChunkSize)

                {

                    FS.Read(B, 0, ChunkSize);

                    //  Write(B);
                    yield return B;
                }
                B = new byte[FSBytes - Pos];

                FS.Read(B, 0, (int)(FSBytes - Pos));
                yield return B;
            }

        }
        private static long GetFilesize(string filename) => File.Exists(filename) ? new FileInfo(filename).Length : 0;

    }


}


/* public bool CompareFiles(string first_filename, string second_filename)
       {

           FileInfo first_file_info = new FileInfo(first_filename);
           FileInfo second_file_info = new FileInfo(second_filename);
           if (first_file_info.Exists
           if (!second_file_info.Exists) return false;
           if (first_file_info.Length != second_file_info.Length) return false;

           {
               // Get file size  
               long size = fi.Length;
           }
       }
       public static bool AreFileContentsEqual(String path1, String path2) =>
           File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));*/