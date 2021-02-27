using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Text2Html
{
    public class Ebook
    {
        private string _filename;
        private string _basePath;
        private string _baseFilename;
        private string _htmlFolder;
        private string _htmlFolderPath;

        private string _headerLine;
        private List<string> _metadata;
        private List<string> _sections;

        public void LoadTextFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new SystemException("Filename is blank");
            }
            if (!File.Exists(filename))
            {
                throw new SystemException($"File not found: {filename}");
            }
            // save filename for later use
            _filename = filename;
            _basePath = filename.Substring(0, _filename.LastIndexOf("\\"));
            _baseFilename = filename.Substring(_filename.LastIndexOf("\\") + 1);
            _baseFilename = _baseFilename.Substring(0, _baseFilename.LastIndexOf(".")); // remove extension
            _htmlFolder = GetHtmlFolderName(_baseFilename);
            _htmlFolderPath = Path.Combine(_basePath, _htmlFolder);
            // prepare storage locations
            _metadata = new List<string>();
            _sections = new List<string>();
            // load all file lines
            string[] lines = File.ReadAllLines(_filename);
            // split lines into sections
            StringBuilder sectionText = new StringBuilder();
            int lineNumber = 0;
            int blankLines = 0;
            bool inHeader = true;
            // add all sections/parts/chapters
            foreach (string line in lines)
            {
                lineNumber++;
                if (inHeader)
                {
                    if (lineNumber == 1)
                    {
                        // title - author [- series] - (year)
                        _headerLine = line;
                        // add title page
                        sectionText.Clear();
                        sectionText.Append(":Title Page");
                        string[] headerLinesSplit = _headerLine.
                                                    Replace(" --- ", "—").
                                                    Replace(" -- ", "—").
                                                    Replace(" - ", "—").
                                                    Split('—');
                        foreach (string line1 in headerLinesSplit)
                        {
                            sectionText.Append("\n\n");
                            sectionText.Append("\t^^");
                            sectionText.Append(line1.Trim());
                        }
                        continue;
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    if (line.StartsWith("<"))
                    {
                        _metadata.Add(line);
                        continue;
                    }
                    inHeader = false;
                }
                // look for new section headings
                if (!string.IsNullOrEmpty(line) && !line.StartsWith("\t"))
                {
                    if (sectionText.Length > 0)
                    {
                        _sections.Add(sectionText.ToString());
                    }
                    sectionText.Clear();
                    blankLines = 0;
                }
                // count blank lines, but don't add until needed
                if (string.IsNullOrEmpty(line))
                {
                    blankLines++;
                    continue;
                }
                // use Append('\n') instead of AppendLine(...) for better splitting
                while (blankLines > 0)
                {
                    sectionText.Append('\n');
                    blankLines--;
                }
                if (sectionText.Length > 0)
                {
                    sectionText.Append('\n');
                }
                sectionText.Append(line);
            }
            // add last section when done
            if (sectionText.Length > 0)
            {
                _sections.Add(sectionText.ToString());
            }
        }

        public void ConvertToHtml()
        {
            // TODO ### convert to html
        }

        public void SaveAsText(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new SystemException("Filename is blank");
            }
            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("### HEADER ###");
                writer.WriteLine(_headerLine);
                writer.WriteLine("### METADATA ###");
                foreach (string line in _metadata)
                {
                    writer.WriteLine(line);
                }
                int sectionNumber = 0;
                foreach (string sectionText in _sections)
                {
                    writer.WriteLine($"### SECTION {sectionNumber} ###");
                    string[] sectionLines = sectionText.Split('\n');
                    foreach (string line in sectionLines)
                    {
                        writer.WriteLine(line);
                    }
                    sectionNumber++;
                }
            }
        }

        public void SaveAsHtml(string pathname)
        {
            if (string.IsNullOrEmpty(pathname))
            {
                throw new SystemException("Pathname is blank");
            }
            try
            {
                if (!Directory.Exists(pathname))
                {
                    Directory.CreateDirectory(pathname);
                }
                if (!Directory.Exists(_htmlFolderPath))
                {
                    Directory.CreateDirectory(_htmlFolderPath);
                }
                if (!Directory.Exists(_htmlFolderPath + "\\_css"))
                {
                    Directory.CreateDirectory(_htmlFolderPath + "\\_css");
                }
            }
            catch (Exception ex)
            {
                throw new SystemException($"Cannot create directory\r\n{ex.Message}");
            }
            // build *.html table of contents document
            int sectionNumber = 0;
            using (StreamWriter writer = File.CreateText(Path.Combine(_basePath, _htmlFolder + ".html")))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                foreach (string line in _metadata)
                {
                    writer.WriteLine(line);
                }
                writer.WriteLine($"<link href={_htmlFolder}\\\"_css\\ebookstyle.css\" rel=\"stylesheet\" type=\"text/css\">");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
                writer.WriteLine("<h1>Table of Contents</h1>");
                writer.WriteLine("<p style=\"text-indent:0pt\">");
                sectionNumber = 0;
                foreach (string sectionText in _sections)
                {
                    string sectionTitle = sectionText.Substring(0, sectionText.IndexOf("\n"));
                    if (sectionTitle.StartsWith(":"))
                    {
                        sectionTitle = sectionTitle[1..];
                    }
                    writer.WriteLine($"<a href=\"{_htmlFolder}\\part{sectionNumber:0000}.html\">{sectionTitle}</a><br/>");
                    sectionNumber++;
                }
                writer.WriteLine("</p>");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
            sectionNumber = 0;
            foreach (string sectionText in _sections)
            {
                using (StreamWriter writer = File.CreateText($"{_htmlFolderPath}\\part{sectionNumber:0000}.html"))
                {
                    writer.WriteLine("<html>");
                    writer.WriteLine("<head>");
                    foreach (string line in _metadata)
                    {
                        //TODO ### only write <title> line here
                        // if (line.Contains("<title>"))
                        // {
                            writer.WriteLine(line);
                        // }
                    }
                    writer.WriteLine("<link href=\"_css\\ebookstyle.css\" rel=\"stylesheet\" type=\"text/css\">");
                    writer.WriteLine("</head>");
                    writer.WriteLine("<body>");
                    writer.Write(TransformText.ConvertText2Html(sectionText, sectionNumber));
                    writer.WriteLine("</body>");
                    writer.WriteLine("</html>");
                    sectionNumber++;
                }
            }
        }

        private static string GetHtmlFolderName(string baseFilename)
        {
            StringBuilder result = new StringBuilder();
            bool needUnderscore = false;
            foreach (char c in baseFilename)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    if (needUnderscore)
                    {
                        result.Append('_');
                    }
                    result.Append(c);
                    needUnderscore = false;
                }
                else if (result.Length > 0)
                {
                    needUnderscore = true;
                }
            }
            return result.ToString();
        }
    }
}
