using System;
using System.IO;

namespace Text2Html
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string filename in Directory.GetFiles(args[0], "*.txt"))
            {
                Console.WriteLine(filename);
                Ebook myBook = new Ebook();
                myBook.LoadTextFile(filename);
                myBook.SaveAsHtml(args[1]);
            }
        }
    }
}
