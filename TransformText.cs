using System;
using System.Collections.Generic;
using System.Text;

namespace Text2Html
{
    public static class TransformText
    {
        public static string ConvertText2Html(string source, int sectionNumber, List<ImageLink> images, List<Footnote> footnotes)
        {
            if (string.IsNullOrEmpty(source))
            {
                return "";
            }
            StringBuilder result = new StringBuilder();
            string[] sectionLines = source.Split('\n');
            int lineNumber = -1;
            foreach (string line in sectionLines)
            {
                lineNumber++;
                string lineNew = line;
                if (sectionNumber == 0)
                {
                    if (lineNumber == 0)
                    {
                        continue;
                    }
                    if (lineNumber == 1)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            continue; // skip if blank
                        }
                        // add blank line if not blank
                        lineNew = "<p>&nbsp;</p>";
                        result.AppendLine(lineNew);
                    }
                    if (string.IsNullOrEmpty(lineNew))
                    {
                        lineNew = "<p>&nbsp;</p>";
                        result.AppendLine(lineNew);
                        continue;
                    }
                    lineNew = lineNew.Replace("\t^^", "");
                    if (lineNumber == 2)
                    {
                        lineNew = $"<div id=\"start\"><h1><u>{lineNew}</u></h1></div>";
                    }
                    else
                    {
                        lineNew = $"<p class=\"title\"><b>{lineNew}</b></p>";
                    }
                    lineNew = FixAngleBrackets(lineNew);
                    result.AppendLine(lineNew);
                    continue;
                }
                if (string.IsNullOrEmpty(lineNew))
                {
                    lineNew = "<p>&nbsp;</p>";
                    result.AppendLine(lineNew);
                    continue;
                }
                if (lineNumber == 0)
                {
                    if (lineNew.StartsWith(":"))
                    {
                        lineNew = $"<h6>{lineNew[1..]}</h6>";
                    }
                    else if (lineNew.StartsWith("+"))
                    {
                        lineNew = $"<h3>{lineNew[1..]}</h3>";
                    }
                    else
                    {
                        lineNew = $"<h3>{lineNew}</h3>";
                    }
                    lineNew = FixHtmlChars(lineNew);
                    result.AppendLine(lineNew);
                    continue;
                }
                if (lineNew.StartsWith("\t"))
                {
                    lineNew = lineNew[1..];
                }
                lineNew = FixHtmlChars(lineNew);
                lineNew = FixHtmlPatterns(lineNew);
                lineNew = FixFootnotes(lineNew, footnotes);
                if (string.IsNullOrEmpty(lineNew))
                {
                    continue;
                }
                if (lineNew == "***" || lineNew == "* * *")
                {
                    if (!result.ToString().EndsWith("<p>&nbsp;</p>\r\n"))
                    {
                        result.Append("<p>&nbsp;</p>");
                    }
                    else
                    {
                        result.Length -= 2; // remove \r\n
                    }
                    lineNew = $"<p class=\"scenebreak\">* * *</p><p>&nbsp;</p>";
                }
                else if (lineNew.StartsWith("^^"))
                {
                    lineNew = $"<p class=\"break\"><b><big>{lineNew[2..]}</big></b></p>";
                }
                else if (lineNew.StartsWith("]"))
                {
                    lineNew = $"<p class=\"blockright\">{lineNew[1..]}</p>";
                }
                else if (lineNew.StartsWith("^"))
                {
                    lineNew = $"<p class=\"break\"><b>{lineNew[1..]}</b></p>";
                }
                else if (lineNew.StartsWith("|") && !lineNew.StartsWith("||"))
                {
                    lineNew = $"<p class=\"noindent\">{lineNew[1..]}</p>";
                }
                else if (lineNew.StartsWith("%"))
                {
                    //TODO Cluster books need "\%" for duocirc quote lines
                    lineNew = $"<p class=\"outdent\">{lineNew[1..]}</p>";
                }
                else if (lineNew.StartsWith("<table") ||
                         lineNew.StartsWith("<th>") ||
                         lineNew.StartsWith("<tr>") ||
                         lineNew.StartsWith("<td>") ||
                         lineNew.StartsWith("</table>") ||
                         lineNew.StartsWith("</th>") ||
                         lineNew.StartsWith("</tr>") ||
                         lineNew.StartsWith("</td>") ||
                         lineNew.StartsWith("<hr"))
                { 
                    // no change to lineNew for some tags
                }
                else if (lineNew.StartsWith("\t"))
                {
                    if (lineNew.StartsWith("\t|"))
                    {
                        lineNew = $"<p class=\"quoteblockflush\">{lineNew[2..]}</p>";
                    }
                    else if (lineNew.StartsWith("\t\t\t\t"))
                    {
                        lineNew = $"<p class=\"quoteblock4\">{lineNew[4..]}</p>";
                    }
                    else if (lineNew.StartsWith("\t\t\t"))
                    {
                        lineNew = $"<p class=\"quoteblock3\">{lineNew[3..]}</p>";
                    }
                    else if (lineNew.StartsWith("\t\t"))
                    {
                        lineNew = $"<p class=\"quoteblock2\">{lineNew[2..]}</p>";
                    }
                    else if (lineNew.StartsWith("\t&nbsp;&nbsp;&nbsp;&nbsp;"))
                    {
                        //TODO ### This is to match TXT2HTML, which is wrong. Remove after testing.
                        lineNew = $"<p class=\"quoteblock\">{lineNew[25..]}</p>";
                    }
                    else
                    {
                        lineNew = $"<p class=\"quoteblock\">{lineNew[1..]}</p>";
                    }
                }
                else
                {
                    lineNew = $"<p>{lineNew}</p>";
                }
                while (lineNew.Contains("<image"))
                {
                    int pos2 = lineNew.IndexOf("<image");
                    int pos3 = lineNew.IndexOf(">", pos2);
                    string filename = lineNew[(pos2 + "<image".Length)..pos3].Trim();
                    if (filename.StartsWith("="))
                    {
                        filename = filename[1..].Trim();
                    }
                    if (filename.EndsWith("/"))
                    {
                        filename = filename[0..^1].Trim();
                    }
                    if (filename.StartsWith("\"") && filename.EndsWith("\""))
                    {
                        filename = filename[1..^1].Trim();
                    }
                    bool found = false;
                    foreach (ImageLink obj in images)
                    {
                        if (obj.ImageFilename == filename)
                        {
                            lineNew = lineNew[0..pos2] +
                                      $"<img src=\"{obj.NewImageFilename}\"></img>" +
                                      lineNew[(pos3 + 1)..];
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        throw new SystemException($"Image not found: {line}");
                    }
                }
                if (lineNew.StartsWith("<p><img ") && lineNew.EndsWith("></img></p>") && !lineNew[4..^11].Contains('>'))
                {
                    lineNew = $"<div style=\"text-align:center\">{lineNew[3..^4]}</div>";
                }
                lineNew = FixAngleBrackets(lineNew);
                result.AppendLine(lineNew);
            }
            return result.ToString();
        }

        private static string FixAngleBrackets(string lineNew)
        {
            if (!lineNew.Contains("<") && !lineNew.Contains(">"))
            {
                return lineNew;
            }
            StringBuilder result = new StringBuilder();
            StringBuilder tag = new StringBuilder();
            bool inTag = false;
            for (int i = 0; i < lineNew.Length; i++)
            {
                char c = lineNew[i];
                if (c != '<' && c != '>')
                {
                    if (inTag)
                    {
                        tag.Append(c);
                    }
                    else
                    {
                        result.Append(c);
                    }
                    continue;
                }
                if (c == '<')
                {
                    if (inTag)
                    {
                        result.Append("&lt;"); // start of tag was not real tag
                        result.Append(tag);
                        tag.Clear();
                        // still inTag
                    }
                    else
                    {
                        inTag = true;
                    }
                }
                else // '>'
                {
                    if (inTag)
                    {
                        result.Append('<');
                        result.Append(tag);
                        result.Append('>');
                        tag.Clear();
                        inTag = false;
                    }
                    else
                    {
                        result.Append("&gt;");
                    }
                }
            }
            if (inTag)
            {
                result.Append("&lt;"); // start of tag was not real tag
                result.Append(tag);
                tag.Clear();
            }
            return result.ToString();
        }

        public static string FixHtmlChars(string line)
        {
            string lineNew = line;
            if (lineNew.Contains("€")) lineNew = lineNew.Replace("€", "&euro;");
            if (lineNew.Contains("‚")) lineNew = lineNew.Replace("‚", "&sbquo;");
            if (lineNew.Contains("ƒ")) lineNew = lineNew.Replace("ƒ", "&fnof;");
            if (lineNew.Contains("„")) lineNew = lineNew.Replace("„", "&bdquo;");
            if (lineNew.Contains("…")) lineNew = lineNew.Replace("…", "&hellip;");
            if (lineNew.Contains("†")) lineNew = lineNew.Replace("†", "&dagger;");
            if (lineNew.Contains("‡")) lineNew = lineNew.Replace("‡", "&Dagger;");
            if (lineNew.Contains("ˆ")) lineNew = lineNew.Replace("ˆ", "&circ;");
            if (lineNew.Contains("‰")) lineNew = lineNew.Replace("‰", "&permil;");
            if (lineNew.Contains("Š")) lineNew = lineNew.Replace("Š", "&Scaron;");
            if (lineNew.Contains("‹")) lineNew = lineNew.Replace("‹", "&lsaquo;");
            if (lineNew.Contains("Œ")) lineNew = lineNew.Replace("Œ", "&OElig;");
            if (lineNew.Contains("Ž")) lineNew = lineNew.Replace("Ž", "&#0381;");
            if (lineNew.Contains("‘")) lineNew = lineNew.Replace("‘", "&lsquo;");
            if (lineNew.Contains("’")) lineNew = lineNew.Replace("’", "&rsquo;");
            if (lineNew.Contains("•")) lineNew = lineNew.Replace("•", "&bull;");
            if (lineNew.Contains("–")) lineNew = lineNew.Replace("–", "&#8211;");
            if (lineNew.Contains("˜")) lineNew = lineNew.Replace("˜", "&tilde;");
            if (lineNew.Contains("™")) lineNew = lineNew.Replace("™", "&trade;");
            if (lineNew.Contains("š")) lineNew = lineNew.Replace("š", "&scaron;");
            if (lineNew.Contains("›")) lineNew = lineNew.Replace("›", "&rsaquo;");
            if (lineNew.Contains("œ")) lineNew = lineNew.Replace("œ", "&oelig;");
            if (lineNew.Contains("ž")) lineNew = lineNew.Replace("ž", "&#0382;");
            if (lineNew.Contains("Ÿ")) lineNew = lineNew.Replace("Ÿ", "&Yuml;");
            if (lineNew.Contains("¡")) lineNew = lineNew.Replace("¡", "&iexcl;");
            if (lineNew.Contains("¢")) lineNew = lineNew.Replace("¢", "&cent;");
            if (lineNew.Contains("£")) lineNew = lineNew.Replace("£", "&pound;");
            if (lineNew.Contains("¤")) lineNew = lineNew.Replace("¤", "&curren;");
            if (lineNew.Contains("¥")) lineNew = lineNew.Replace("¥", "&yen;");
            if (lineNew.Contains("¦")) lineNew = lineNew.Replace("¦", "&brvbar;");
            if (lineNew.Contains("§")) lineNew = lineNew.Replace("§", "&sect;");
            if (lineNew.Contains("¨")) lineNew = lineNew.Replace("¨", "&uml;");
            if (lineNew.Contains("©")) lineNew = lineNew.Replace("©", "&copy;");
            if (lineNew.Contains("ª")) lineNew = lineNew.Replace("ª", "&ordf;");
            if (lineNew.Contains("«")) lineNew = lineNew.Replace("«", "&laquo;");
            if (lineNew.Contains("¬")) lineNew = lineNew.Replace("¬", "&not;");
            if (lineNew.Contains("®")) lineNew = lineNew.Replace("®", "&reg;");
            if (lineNew.Contains("¯")) lineNew = lineNew.Replace("¯", "&macr;");
            if (lineNew.Contains("°")) lineNew = lineNew.Replace("°", "&deg;");
            if (lineNew.Contains("±")) lineNew = lineNew.Replace("±", "&plusmn;");
            if (lineNew.Contains("²")) lineNew = lineNew.Replace("²", "&sup2;");
            if (lineNew.Contains("³")) lineNew = lineNew.Replace("³", "&sup3;");
            if (lineNew.Contains("´")) lineNew = lineNew.Replace("´", "&acute;");
            if (lineNew.Contains("µ")) lineNew = lineNew.Replace("µ", "&micro;");
            if (lineNew.Contains("¶")) lineNew = lineNew.Replace("¶", "&para;");
            if (lineNew.Contains("·")) lineNew = lineNew.Replace("·", "&nbsp;"); // use middot for nbsp instead of "&middot;"
            if (lineNew.Contains("¸")) lineNew = lineNew.Replace("¸", "&cedil;");
            if (lineNew.Contains("¹")) lineNew = lineNew.Replace("¹", "&sup1;");
            if (lineNew.Contains("º")) lineNew = lineNew.Replace("º", "&ordm;");
            if (lineNew.Contains("»")) lineNew = lineNew.Replace("»", "&raquo;");
            if (lineNew.Contains("¼")) lineNew = lineNew.Replace("¼", "&frac14;");
            if (lineNew.Contains("½")) lineNew = lineNew.Replace("½", "&frac12;");
            if (lineNew.Contains("¾")) lineNew = lineNew.Replace("¾", "&frac34;");
            if (lineNew.Contains("¿")) lineNew = lineNew.Replace("¿", "&iquest;");
            if (lineNew.Contains("À")) lineNew = lineNew.Replace("À", "&Agrave;");
            if (lineNew.Contains("Á")) lineNew = lineNew.Replace("Á", "&Aacute;");
            if (lineNew.Contains("Â")) lineNew = lineNew.Replace("Â", "&Acirc;");
            if (lineNew.Contains("Ã")) lineNew = lineNew.Replace("Ã", "&Atilde;");
            if (lineNew.Contains("Ä")) lineNew = lineNew.Replace("Ä", "&Auml;");
            if (lineNew.Contains("Å")) lineNew = lineNew.Replace("Å", "&Aring;");
            if (lineNew.Contains("Æ")) lineNew = lineNew.Replace("Æ", "&AElig;");
            if (lineNew.Contains("Ç")) lineNew = lineNew.Replace("Ç", "&Ccedil;");
            if (lineNew.Contains("È")) lineNew = lineNew.Replace("È", "&Egrave;");
            if (lineNew.Contains("É")) lineNew = lineNew.Replace("É", "&Eacute;");
            if (lineNew.Contains("Ê")) lineNew = lineNew.Replace("Ê", "&Ecirc;");
            if (lineNew.Contains("Ë")) lineNew = lineNew.Replace("Ë", "&Euml;");
            if (lineNew.Contains("Ì")) lineNew = lineNew.Replace("Ì", "&Igrave;");
            if (lineNew.Contains("Í")) lineNew = lineNew.Replace("Í", "&Iacute;");
            if (lineNew.Contains("Î")) lineNew = lineNew.Replace("Î", "&Icirc;");
            if (lineNew.Contains("Ï")) lineNew = lineNew.Replace("Ï", "&Iuml;");
            if (lineNew.Contains("Ð")) lineNew = lineNew.Replace("Ð", "&ETH;");
            if (lineNew.Contains("Ñ")) lineNew = lineNew.Replace("Ñ", "&Ntilde;");
            if (lineNew.Contains("Ò")) lineNew = lineNew.Replace("Ò", "&Ograve;");
            if (lineNew.Contains("Ó")) lineNew = lineNew.Replace("Ó", "&Oacute;");
            if (lineNew.Contains("Ô")) lineNew = lineNew.Replace("Ô", "&Ocirc;");
            if (lineNew.Contains("Õ")) lineNew = lineNew.Replace("Õ", "&Otilde;");
            if (lineNew.Contains("Ö")) lineNew = lineNew.Replace("Ö", "&Ouml;");
            if (lineNew.Contains("×")) lineNew = lineNew.Replace("×", "&times;");
            if (lineNew.Contains("Ø")) lineNew = lineNew.Replace("Ø", "&Oslash;");
            if (lineNew.Contains("Ù")) lineNew = lineNew.Replace("Ù", "&Ugrave;");
            if (lineNew.Contains("Ú")) lineNew = lineNew.Replace("Ú", "&Uacute;");
            if (lineNew.Contains("Û")) lineNew = lineNew.Replace("Û", "&Ucirc;");
            if (lineNew.Contains("Ü")) lineNew = lineNew.Replace("Ü", "&Uuml;");
            if (lineNew.Contains("Ý")) lineNew = lineNew.Replace("Ý", "&Yacute;");
            if (lineNew.Contains("Þ")) lineNew = lineNew.Replace("Þ", "&THORN;");
            if (lineNew.Contains("ß")) lineNew = lineNew.Replace("ß", "&szlig;");
            if (lineNew.Contains("à")) lineNew = lineNew.Replace("à", "&agrave;");
            if (lineNew.Contains("á")) lineNew = lineNew.Replace("á", "&aacute;");
            if (lineNew.Contains("â")) lineNew = lineNew.Replace("â", "&acirc;");
            if (lineNew.Contains("ã")) lineNew = lineNew.Replace("ã", "&atilde;");
            if (lineNew.Contains("ä")) lineNew = lineNew.Replace("ä", "&auml;");
            if (lineNew.Contains("å")) lineNew = lineNew.Replace("å", "&aring;");
            if (lineNew.Contains("æ")) lineNew = lineNew.Replace("æ", "&aelig;");
            if (lineNew.Contains("ç")) lineNew = lineNew.Replace("ç", "&ccedil;");
            if (lineNew.Contains("è")) lineNew = lineNew.Replace("è", "&egrave;");
            if (lineNew.Contains("é")) lineNew = lineNew.Replace("é", "&eacute;");
            if (lineNew.Contains("ê")) lineNew = lineNew.Replace("ê", "&ecirc;");
            if (lineNew.Contains("ë")) lineNew = lineNew.Replace("ë", "&euml;");
            if (lineNew.Contains("ì")) lineNew = lineNew.Replace("ì", "&igrave;");
            if (lineNew.Contains("í")) lineNew = lineNew.Replace("í", "&iacute;");
            if (lineNew.Contains("î")) lineNew = lineNew.Replace("î", "&icirc;");
            if (lineNew.Contains("ï")) lineNew = lineNew.Replace("ï", "&iuml;");
            if (lineNew.Contains("ð")) lineNew = lineNew.Replace("ð", "&eth;");
            if (lineNew.Contains("ñ")) lineNew = lineNew.Replace("ñ", "&ntilde;");
            if (lineNew.Contains("ò")) lineNew = lineNew.Replace("ò", "&ograve;");
            if (lineNew.Contains("ó")) lineNew = lineNew.Replace("ó", "&oacute;");
            if (lineNew.Contains("ô")) lineNew = lineNew.Replace("ô", "&ocirc;");
            if (lineNew.Contains("õ")) lineNew = lineNew.Replace("õ", "&otilde;");
            if (lineNew.Contains("ö")) lineNew = lineNew.Replace("ö", "&ouml;");
            if (lineNew.Contains("÷")) lineNew = lineNew.Replace("÷", "&divide;");
            if (lineNew.Contains("ø")) lineNew = lineNew.Replace("ø", "&oslash;");
            if (lineNew.Contains("ù")) lineNew = lineNew.Replace("ù", "&ugrave;");
            if (lineNew.Contains("ú")) lineNew = lineNew.Replace("ú", "&uacute;");
            if (lineNew.Contains("û")) lineNew = lineNew.Replace("û", "&ucirc;");
            if (lineNew.Contains("ü")) lineNew = lineNew.Replace("ü", "&uuml;");
            if (lineNew.Contains("ý")) lineNew = lineNew.Replace("ý", "&yacute;");
            if (lineNew.Contains("þ")) lineNew = lineNew.Replace("þ", "&thorn;");
            if (lineNew.Contains("ÿ")) lineNew = lineNew.Replace("ÿ", "&yuml;");
            // chars by number
            if (lineNew.Contains((char)147)) lineNew = lineNew.Replace(((char)147).ToString(), "&ldquo;");
            if (lineNew.Contains((char)148)) lineNew = lineNew.Replace(((char)148).ToString(), "&rdquo;");
            return lineNew;
        }

        public static string FixHtmlPatterns(string line)
        {
            string lineNew = line;
            if (lineNew.Contains("\\\"")) lineNew = lineNew.Replace("\\\"", "\"");
            if (lineNew.Contains("\\'")) lineNew = lineNew.Replace("\\'", "'");
            if (lineNew.Contains("' \"")) lineNew = lineNew.Replace("' \"", "'&nbsp;\"");
            if (lineNew.Contains("\" '")) lineNew = lineNew.Replace("\" '", "\"&nbsp;'");
            if (lineNew.Contains("&#0;")) lineNew = lineNew.Replace("&#0;", "");
            if (lineNew.Contains("&#32;")) lineNew = lineNew.Replace("&#32;", " ");
            if (lineNew.Contains("&#45;")) lineNew = lineNew.Replace("&#45;", "-");
            if (lineNew.Contains("<overline>")) lineNew = lineNew.Replace("<overline>", "<span style=\"text-decoration: overline;\">");
            if (lineNew.Contains("</overline>")) lineNew = lineNew.Replace("</overline>", "</span>");
            return lineNew;
        }

        private static string FixFootnotes(string line, List<Footnote> footnotes)
        {
            string lineNew = line;
            while (lineNew.Contains("<footnote"))
            {
                int pos2 = lineNew.IndexOf("<footnote");
                int pos3 = lineNew.IndexOf("/>", pos2);
                string tag = lineNew[(pos2 + "<footnote".Length)..pos3].Trim();
                foreach (Footnote fn in footnotes)
                {
                    if (fn.FootnoteTag == tag)
                    {
                        if (fn.LinkSectionNumber == fn.TextSectionNumber)
                        {
                            lineNew = lineNew[0..pos2] +
                                      $"<a id=\"fr{tag}\" href=\"#fn{tag}\">[{tag}]</a>" +
                                      lineNew[(pos3 + 2)..];
                        }
                        else
                        {
                            lineNew = lineNew[0..pos2] +
                                      $"<a id=\"fr{tag}\" href=\"#fn{tag}\">[{tag}]</a>" +
                                      lineNew[(pos3 + 2)..];
                            // TODO ### removed for testing, put back after full compare
                            //lineNew = lineNew[0..pos2] +
                            //          $"<a id=\"fr{tag}\" href=\"part{fn.TextSectionNumber:0000}.html#fn{tag}\">[{tag}]</a>" +
                            //          lineNew[(pos3 + 2)..];
                        }
                        break;
                    }
                };
            }
            while (lineNew.Contains("<foottext"))
            {
                int pos2 = lineNew.IndexOf("<foottext");
                int pos3 = lineNew.IndexOf("/>", pos2);
                string tag = lineNew[(pos2 + "<foottext".Length)..pos3].Trim();
                foreach (Footnote fn in footnotes)
                {
                    if (fn.FootnoteTag == tag)
                    {
                        if (fn.LinkSectionNumber == fn.TextSectionNumber)
                        {
                            lineNew = lineNew[0..pos2] +
                                      $"<a id=\"fn{tag}\" href=\"#fr{tag}\">[{tag}]</a>" +
                                      lineNew[(pos3 + 2)..];
                        }
                        else
                        {
                            lineNew = lineNew[0..pos2] +
                                      $"<a id=\"fn{tag}\" href=\"#fr{tag}\">[{tag}]</a>" +
                                      lineNew[(pos3 + 2)..];
                            // TODO ### removed for testing, put back after full compare
                            // lineNew = lineNew[0..pos2] +
                            //           $"<a id=\"fn{tag}\" href=\"part{fn.LinkSectionNumber:0000}.html#fr{tag}\">[{tag}]</a>" +
                            //           lineNew[(pos3 + 2)..];
                        }
                        break;
                    }
                };
            }
            return lineNew;
        }

    }
}
