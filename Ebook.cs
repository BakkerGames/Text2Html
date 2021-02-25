using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Text2Html
{
    public class Ebook
    {
        private string _filename;

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
            foreach (string line in lines)
            {
                lineNumber++;
                if (inHeader)
                {
                    if (lineNumber == 1)
                    {
                        // title - author [- series] - (year)
                        _headerLine = line;
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
                if (string.IsNullOrEmpty(line))
                {
                    blankLines++;
                    continue;
                }
                while (blankLines > 0)
                {
                    sectionText.AppendLine();
                    blankLines--;
                }
                sectionText.AppendLine(line);
            }
            // add last section when done
            if (sectionText.Length > 0)
            {
                _sections.Add(sectionText.ToString());
            }
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
                    sectionNumber++;
                    writer.WriteLine($"### SECTION {sectionNumber} ###");
                    writer.Write(sectionText); // already ends with eol
                }
            }
        }
    }
}
