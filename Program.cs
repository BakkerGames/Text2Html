using System;

namespace Text2Html
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
                Ebook myBook = new Ebook();
                myBook.LoadTextFile(arg);
                myBook.SaveAsText("C:\\Temp\\Output.txt");
            }
        }
    }
}
