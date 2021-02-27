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
                myBook.ConvertToHtml();
                myBook.SaveAsText("C:\\Books\\Temp\\Output.txt");
                myBook.SaveAsHtml("C:\\Books\\Temp");
            }
        }
    }
}
