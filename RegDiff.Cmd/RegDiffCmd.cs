using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RegDiff.Cmd
{
    internal class RegDiffCmd
    {
        private static void Main(string[] args)
        {
            // validate first 2 files exist
            if (args.Take(2).Where(TestFileExists).Any())
            {
                Environment.Exit(1);
            }

            // validate last 2 paths are valid
            if (args.Skip(2).Take(2).Where(TestValidPath).Any())
            {
                Environment.Exit(1);
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Compares two .reg files, producing 2 output files " + Environment.NewLine +
                                  "suitable for viewing in a diff tool." + Environment.NewLine + Environment.NewLine +
                                  "USAGE: RegDiff baseFile.reg changedFile.reg [baseFileOutput.reg] " +
                                  Environment.NewLine +
                                  "       [changedFileOutput.reg] " + Environment.NewLine);
                Environment.Exit(0);
            }

            List<string> argsList = args.ToList();

            string outFileName;

            switch (args.Length)
            {
                case 2:
                    outFileName = "out" + Path.GetFileName(args[0]);
                    argsList.Add(args[0].Replace(Path.GetFileName(args[0]), outFileName));
                    outFileName = "out" + Path.GetFileName(args[1]);
                    argsList.Add(args[1].Replace(Path.GetFileName(args[1]), outFileName));
                    break;
                case 3:
                    outFileName = "out" + Path.GetFileName(args[1]);
                    argsList.Add(args[1].Replace(Path.GetFileName(args[1]), outFileName));
                    break;
            }

            var regDiff = new Core.RegDiff();

            try
            {
                regDiff.WriteDifferences(argsList[0], argsList[1], argsList[2], argsList[3]);
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }


        private static bool TestFileExists(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine(filePath + " - file not found");
                    return true;
                }
            }
            catch
            {
                Console.WriteLine(filePath + " - invalid path or error accessing file.");
                return true;
            }

            return false;
        }

        private static bool TestValidPath(string filePath)
        {
            try
            {
                // will throw if invalid path
                var fileInfo = new FileInfo(filePath);
            }
            catch (Exception)
            {
                Console.WriteLine(filePath + " - invalid path or error writting file.");
                return true;
            }

            return false;
        }
    }
}