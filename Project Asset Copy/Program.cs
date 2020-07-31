using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Project_Asset_Copy
{
    class Program
    {
        static HashAlgorithm md5 = HashAlgorithm.Create("MD5");

        static int Main(string[] args)
        {
            ConsoleColor foreCol = Console.ForegroundColor;
            if (args.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Too many or few arguments! 2 directories/paths required, got {args.Length}:");
                foreach (string arg in args)
                    Console.WriteLine(arg);
                Console.ForegroundColor = foreCol;
                return 1;
            }
            if (args.Count(x=>string.IsNullOrWhiteSpace(x)) > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Input parameters cannot be blank.");
                Console.ForegroundColor = foreCol;
                return 1;
            }

            bool fileExists = File.Exists(args[0]);
            bool directoryExists = Directory.Exists(args[0]);

            if (!fileExists && !directoryExists)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"File or directory \"{args[0]}\" not found!");
                Console.ForegroundColor = foreCol;
                return 1;
            }

            string outputPath = directoryExists ? args[1] : Path.GetDirectoryName(args[1]);

            if (!string.IsNullOrWhiteSpace(outputPath) && !Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
                Console.WriteLine($"Made directory {outputPath}");
            }

            if (fileExists)
            {
                CopyFile(args[0], args[1]);
            }
            else if (directoryExists)
            {
                foreach (string dirTree in Directory.EnumerateDirectories(args[0], "*", SearchOption.AllDirectories))
                {
                    string dest = dirTree.Replace(args[0], args[1]);
                    if (!Directory.Exists(dest))
                    {
                        Directory.CreateDirectory(dest);
                        Console.WriteLine($"Made directory {dest}");
                    }
                }

                foreach(string file in Directory.EnumerateFiles(args[0], "*.*", SearchOption.AllDirectories))
                {
                    string dest = file.Replace(args[0], args[1]);
                    CopyFile(file, dest);
                }
            }

#if DEBUG
            Console.ReadKey();
#endif
            return 0;
        }

        /// <summary>
        /// Compares the MD5 hashes of each specified file.
        /// </summary>
        /// <param name="file1location">The path to the first file.</param>
        /// <param name="file2location">The path to the second file.</param>
        /// <returns>True if the hashes are equal, otherwise false.</returns>
        static bool CompareFileMD5(string file1location, string file2location)
        {
            byte[] file1data = File.ReadAllBytes(file1location);
            byte[] file2data = File.ReadAllBytes(file2location);

            byte[] file1hashdata = md5.ComputeHash(file1data);
            byte[] file2hashdata = md5.ComputeHash(file2data);

            BigInteger file1hash = new BigInteger(file1hashdata);
            BigInteger file2hash = new BigInteger(file2hashdata);

            return file1hash == file2hash;
        }

        static void CopyFile(string f1l, string f2l)
        {
            if (File.Exists(f2l))
            {
                if (!CompareFileMD5(f1l, f2l))
                {
                    File.Copy(f1l, f2l, true);
                    Console.WriteLine($"Updated file {f2l}");
                }
                else
                {
                    Console.WriteLine($"No changes made to file {f2l}");
                }
            }
            else
            {
                File.Copy(f1l, f2l);
                Console.WriteLine($"Copied file {f2l}");
            }
        }
    }
}
