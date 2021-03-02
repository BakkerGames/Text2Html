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

        private string _headerLine;
        private List<string> _metadata;
        private List<string> _sections;
        private List<ImageLink> _images;
        private List<Footnote> _footnotes;

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
            // prepare storage locations
            _metadata = new List<string>();
            _sections = new List<string>();
            _images = new List<ImageLink>();
            _footnotes = new List<Footnote>();
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
                    if (line.StartsWith("<title") || line.StartsWith("<meta"))
                    {
                        _metadata.Add(line);
                        continue;
                    }
                    inHeader = false;
                }
                // look for new section headings
                if (!string.IsNullOrEmpty(line) &&
                    !line.StartsWith("\t") &&
                    !line.StartsWith("<image"))
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
            // look for images
            _images = FindImages(_sections);
            // look for footnotes
            _footnotes = FindFootnotes(_sections);
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

        public void SaveAsHtml(string pathname, string cssText)
        {
            if (string.IsNullOrEmpty(pathname))
            {
                throw new SystemException("Pathname is blank");
            }
            _htmlFolder = GetHtmlFolderName(_baseFilename);
            // create all directories
            try
            {
                if (!Directory.Exists(pathname))
                {
                    Directory.CreateDirectory(pathname);
                }
                if (!Directory.Exists(Path.Combine(pathname, _htmlFolder)))
                {
                    Directory.CreateDirectory(Path.Combine(pathname, _htmlFolder));
                }
                if (!Directory.Exists(Path.Combine(pathname, _htmlFolder , "_css")))
                {
                    Directory.CreateDirectory(Path.Combine(pathname, _htmlFolder, "_css"));
                }
                if (_images != null && _images.Count > 0)
                {
                    if (!Directory.Exists(Path.Combine(pathname, _htmlFolder, "images")))
                    {
                        Directory.CreateDirectory(Path.Combine(pathname, _htmlFolder, "images"));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SystemException($"Cannot create directory\r\n{ex.Message}");
            }
            // build *.html table of contents document
            int sectionNumber = 0;
            using (StreamWriter writer = File.CreateText(Path.Combine(pathname, _htmlFolder + ".html")))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                foreach (string line in _metadata)
                {
                    writer.WriteLine(line);
                }
                //TODO ### changed for debugging, change back
                //writer.WriteLine($"<link href={_htmlFolder}\\\"_css\\ebookstyle.css\" rel=\"stylesheet\" type=\"text/css\">");
                writer.WriteLine($"<link href=\"_css\\ebookstyle.css\" rel=\"stylesheet\" type=\"text/css\">");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
                writer.WriteLine("<h1>Table of Contents</h1>");
                writer.WriteLine("<p style=\"text-indent:0pt\">");
                sectionNumber = 0;
                foreach (string sectionText in _sections)
                {
                    string sectionTitle;
                    if (sectionText.Contains("\n"))
                    {
                        sectionTitle = sectionText[0..sectionText.IndexOf("\n")];
                    }
                    else
                    {
                        sectionTitle = sectionText;
                    }
                    if (sectionTitle.StartsWith(":") ||
                        sectionTitle.StartsWith("+"))
                    {
                        sectionTitle = sectionTitle[1..];
                    }
                    sectionTitle = TransformText.FixHtmlChars(sectionTitle);
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
                using (StreamWriter writer = File.CreateText(Path.Combine(pathname, _htmlFolder, $"part{sectionNumber:0000}.html")))
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
                    writer.Write(TransformText.ConvertText2Html(sectionText, sectionNumber, _images, _footnotes));
                    writer.WriteLine("</body>");
                    writer.WriteLine("</html>");
                    sectionNumber++;
                }
            }
            // create CSS file
            try
            {
                if (!File.Exists(Path.Combine(pathname, _htmlFolder, "_css","ebookstyle.css")))
                {
                    File.WriteAllText(Path.Combine(pathname, _htmlFolder, "_css","ebookstyle.css"), cssText);
                }
            }
            catch (Exception ex)
            {
                throw new SystemException($"Cannot create CSS file\r\n{ex.Message}");
            }
            // copy images to new folder
            try
            {
                if (_images != null && _images.Count > 0)
                {
                    foreach (ImageLink obj in _images)
                    {
                        if (File.Exists(Path.Combine(_basePath, obj.ImageFilename)) &&
                            !File.Exists(Path.Combine(pathname, _htmlFolder, obj.NewImageFilename)))
                        {
                            File.Copy(Path.Combine(_basePath, obj.ImageFilename),
                                      Path.Combine(pathname, _htmlFolder, obj.NewImageFilename));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SystemException($"Error copying images\r\n{ex.Message}");
            }
        }

        private List<Footnote> FindFootnotes(List<string> sections)
        {
            List<Footnote> result = new List<Footnote>();
            int sectionNumber = 0;
            foreach (string sectionText in _sections)
            {
                int pos = 0;
                while (sectionText.IndexOf("<footnote", pos) >= 0)
                {
                    int pos2 = sectionText.IndexOf("<footnote", pos);
                    int pos3 = sectionText.IndexOf("/>", pos2);
                    string tag = sectionText[(pos2 + "<footnote".Length)..pos3].Trim();
                    Footnote fn = new Footnote
                    {
                        FootnoteTag = tag,
                        LinkSectionNumber = sectionNumber,
                        TextSectionNumber = -1
                    };
                    result.Add(fn);
                    pos = pos3 + 2;
                }
                sectionNumber++;
            }
            sectionNumber = 0;
            foreach (string sectionText in _sections)
            {
                int pos = 0;
                while (sectionText.IndexOf("<foottext", pos) >= 0)
                {
                    int pos2 = sectionText.IndexOf("<foottext", pos);
                    int pos3 = sectionText.IndexOf("/>", pos2);
                    string tag = sectionText[(pos2 + "<foottext".Length)..pos3].Trim();
                    foreach (Footnote fn in result)
                    {
                        if (fn.FootnoteTag == tag)
                        {
                            fn.TextSectionNumber = sectionNumber;
                            break;
                        }
                    };
                    pos = pos3 + 2;
                }
                sectionNumber++;
            }
            return result;
        }

        private List<ImageLink> FindImages(List<string> sections)
        {
            List<ImageLink> result = new List<ImageLink>();
            int sectionNumber = 0;
            int imageNumber = 0;
            foreach (string sectionText in _sections)
            {
                int pos = 0;
                while (sectionText.IndexOf("<image", pos) >= 0)
                {
                    int pos2 = sectionText.IndexOf("<image", pos);
                    int pos3 = sectionText.IndexOf(">", pos2);
                    string filename = sectionText[(pos2 + "<image".Length)..pos3].Trim();
                    if (filename.StartsWith("="))
                    {
                        filename = filename[1..].Trim();
                    }
                    if (filename.EndsWith("/"))
                    {
                        filename = filename[0..^1].Trim();
                    }
                    bool found = false;
                    foreach (ImageLink obj in result)
                    {
                        if (obj.ImageFilename == filename)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        string extension = filename[filename.LastIndexOf(".")..];
                        ImageLink imageObj = new ImageLink
                        {
                            ImageFilename = filename,
                            NewImageFilename = $"images\\{imageNumber:00000}{extension}"
                        };
                        result.Add(imageObj);
                        imageNumber++;
                    }
                    pos = pos3 + 1;
                }
                sectionNumber++;
            }
            return result;
        }

        private static string GetHtmlFolderName(string baseFilename)
        {
            StringBuilder result = new StringBuilder();
            bool needUnderscore = false;
            foreach (char c in baseFilename)
            {
                if (c == '\'') continue; // ignore single quotes
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
