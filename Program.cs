using System;
using System.IO;
using System.Reflection;

namespace Text2Html
{
    class Program
    {
        public static void Main(string[] args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string cssText = GetStringResource("Resources.ebookstyle.css", assembly);
            foreach (string filename in Directory.GetFiles(args[0], "*.txt"))
            {
                Console.WriteLine(filename);
                Ebook myBook = new Ebook();
                myBook.LoadTextFile(filename);
                myBook.SaveAsHtml(args[1], cssText);
            }
        }

        private static string GetStringResource(string resourceName, Assembly assembly)
        {
            using (Stream resourceStream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName))
            {
                if (resourceStream == null)
                {
                    return null;
                }
                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
