using System;
using System.IO;
using System.Reflection;

namespace Text2Html
{
    class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Syntax: frompath topath");
                return 1;
            }
            Console.WriteLine($"From: {args[0]}");
            Console.WriteLine($"To  : {args[1]}");
            Console.WriteLine();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string cssText = GetStringResource("Resources.ebookstyle.css", assembly);
            int fileCount = 0;
            int changedCount = 0;
            foreach (string filename in Directory.GetFiles(args[0], "*.txt"))
            {
                fileCount++;
                Console.Write($"\r{fileCount}");
                Ebook myBook = new();
                myBook.LoadTextFile(filename);
                if (myBook.SaveAsHtml(args[1], cssText))
                {
                    Console.Write("\r");
                    Console.WriteLine(filename);
                    changedCount++;
                }
            }
            Console.Write("\r");
            if (changedCount > 0)
            {
                Console.WriteLine();
            }
            Console.WriteLine($"Files found  : {fileCount}");
            Console.WriteLine($"Files changed: {changedCount}");
            Console.WriteLine();
            Console.Write("Press enter to continue...");
            Console.ReadLine();
            return 0;
        }

        private static string GetStringResource(string resourceName, Assembly assembly)
        {
            using (Stream resourceStream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName))
            {
                if (resourceStream == null)
                {
                    return null;
                }
                using (StreamReader reader = new(resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
