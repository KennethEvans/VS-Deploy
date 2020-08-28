#define verbose
using System;
using System.IO;

namespace Deploy {

    class Program {
        private static readonly string DEPLOY_DIR = @"C:\Deploy\";
        private static readonly string TAB = "  ";
        static void Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("Must specify $(ProjectFileName) $(BinDir)");
                return;
            }
#if verbose || false
            int nArgs = 0;
            foreach (string arg in args) {
                Console.WriteLine("argument " + nArgs + ": " + arg);
                nArgs++;
            }
#endif
            string projectFileName = args[0];
            string binDir = args[1];
            int index = projectFileName.IndexOf(".csproj");
            if (index < 0) {
                Console.WriteLine("Is not a C# project");
                return;
            }
            if (!binDir.EndsWith(@"Release\")) {
                Console.WriteLine("Is not a Framework Release build");
                return;
            }
            string name = projectFileName.Substring(0, index);
            string deployName = DEPLOY_DIR + name;
            Console.WriteLine("name=" + name);
            Console.WriteLine("deployName=" + deployName);
            if (!Directory.Exists(deployName)) {
                Console.WriteLine("Deploy directory does not exist: " + deployName);
                return;
            }
            if (!Directory.Exists(binDir)) {
                Console.WriteLine("Release directory does not exist: " + binDir);
                return;
            }

#if verbose || false
            Console.WriteLine("Files in " + deployName);
            listAllFiles(deployName, 0);
#endif

            Console.WriteLine("Files in " + binDir);
            listAllFiles(binDir, 0);

            Console.WriteLine("Removing files from " + deployName);
            try {
                deleteAllFiles(deployName);
            } catch (Exception ex) {
                Console.WriteLine("Error removing files: " + ex.Message);
                Console.WriteLine();
                Console.WriteLine("Aborted");
                return;
            }

            Console.WriteLine("Copying files from " + binDir);
            try {
                copyAllFiles(binDir, binDir, deployName);
            } catch (Exception ex) {
                Console.WriteLine("Error copying files: " + ex.Message);
                Console.WriteLine();
                Console.WriteLine("Aborted");
                return;
            }

#if verbose
            Console.WriteLine("Files in " + deployName);
            listAllFiles(deployName, 0);
#endif

            Console.WriteLine();
            Console.WriteLine("All Done");
        }

        private static void listAllFiles(string parent, int level) {
            string[] files = Directory.GetFiles(parent);
            string[] dirs = Directory.GetDirectories(parent);
            string tabs = string.Concat(System.Linq.Enumerable.Repeat(TAB, level));
            foreach (string dir in dirs) {
                Console.WriteLine(tabs + dir);
                listAllFiles(dir, level + 1);
            }
            foreach (string file in files) {
                Console.WriteLine(tabs + file);
            }
        }

        private static void deleteAllFiles(string parent) {
            string[] files = Directory.GetFiles(parent);
            string[] dirs = Directory.GetDirectories(parent);
            foreach (string dir in dirs) {
                deleteAllFiles(dir);
                Directory.Delete(dir);
            }
            foreach (string file in files) {
                File.Delete(file);
            }
        }

        private static void copyAllFiles(string parent, string parentName, string destName) {
            string parentName0 = parentName;
            if (parentName.EndsWith(@"\")) {
                parentName0 = parentName.Substring(0, parentName.Length - 1);
            }
            string destName0 = destName;
            if (destName.EndsWith(@"\")) {
                destName0 = destName.Substring(0, parentName.Length - 1);
            }
            string[] files = Directory.GetFiles(parent);
            string[] dirs = Directory.GetDirectories(parent);
            string newName;
            foreach (string dir in dirs) {
                newName = dir.Replace(parentName0, destName0);
#if verbose
                Console.WriteLine("CreateDirectory: " + newName);
#endif
                Directory.CreateDirectory(newName);
                copyAllFiles(dir, parentName, destName);
            }
            foreach (string file in files) {
                newName = file.Replace(parentName0, destName0);
#if verbose
                Console.WriteLine("Copy From: " + file);
                Console.WriteLine("     To:   " + newName);
#endif 
                File.Copy(file, newName);
            }
        }
    }
}
