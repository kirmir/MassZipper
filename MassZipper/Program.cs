using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Ionic.Zlib;

namespace MassZipper
{
    /// <summary>
    /// Program's main class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Program's entry point, main function.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            if (!args.Any() || args[0] == "/?")
            {
                Console.WriteLine("MassZipper by Miroshnichenko K.V.");
                Console.WriteLine("Usage: MassZipper.exe path [/d] [/r]");
                Console.WriteLine("       path - path to start searching for folders with files");
                Console.WriteLine("       /d   - delete compressed files");
                Console.WriteLine("       /r   - rename files as \"001\", \"002\", ..., \"999\" before compression");
                return;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("Specified path doesn't exist.");
                return;
            }

            // Read command-line parameters.
            var delete = args.Contains("/d");
            var rename = args.Contains("/r");

            // Rename, compress and delete files.
            var dirs = GetSubdirectories(args[0]).ToList();
            var count = dirs.Count;

            for (var i = 0; i < count; i++)
            {
                var dir = dirs[i];

                if (rename)
                    RenameFiles(dir);

                Console.WriteLine(string.Format("[{0}/{1}] \"{2}\"", i + 1, count, dir));
                Compress(dir);

                if (delete)
                    Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Gets the subdirectories which doesn't have any subdirectories but files.
        /// </summary>
        /// <param name="dir">The directory to start the search.</param>
        /// <returns>List of subdirectories.</returns>
        private static IEnumerable<string> GetSubdirectories(string dir)
        {
            return from d in Directory.GetDirectories(dir, "*.*", SearchOption.AllDirectories)
                   let files = Directory.GetFiles(d)
                   where !Directory.GetDirectories(d).Any() && files.Any() &&
                         !files.Any(f => Path.GetExtension(f) == ".zip")
                   select d;
        }

        /// <summary>
        /// Renames the files in directory.
        /// </summary>
        /// <param name="dir">The directory name.</param>
        private static void RenameFiles(string dir)
        {
            var dirInfo = new DirectoryInfo(dir);
            var files = dirInfo.GetFiles();
            
            var count = files.Count();

            var length = count.ToString().Length;
            if (length < 3) length = 3;

            var format = "D" + length;

            for (var i = 0; i < count; i++)
            {
                var file = files[i];

                if (file.DirectoryName != null)
                {
                    File.Move(file.FullName, Path.Combine(file.DirectoryName, (i + 1).ToString(format) + file.Extension));
                }
            }
        }

        /// <summary>
        /// Compresses files in the specified directory to ZIP-archive with the name of directory.
        /// </summary>
        /// <param name="dir">The directory.</param>
        private static void Compress(string dir)
        {
            using (var zip = new ZipFile())
            {
                zip.CompressionLevel = CompressionLevel.BestSpeed;
                zip.CompressionMethod = CompressionMethod.None;
                zip.AddFiles(Directory.GetFiles(dir), false, "/");
                zip.Save(dir + ".zip");
            }
        }
    }
}
