﻿using System.Text;

namespace Text2Html
{
    public static class TransformText
    {
        public static string ConvertText2Html(string source, int sectionNumber)
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
                    if (lineNumber < 2)
                    {
                        continue;
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
                    lineNew = $"<h3>{lineNew}</h3>";
                    result.AppendLine(lineNew);
                    continue;
                }
                lineNew = lineNew[1..]; // remove \t
                lineNew = FixHtmlChars(lineNew);
                lineNew = FixHtmlPatterns(lineNew);
                if (lineNew == "***")
                {
                    lineNew = $"&nbsp;</p><p class=\"scenebreak\">* * *</p><p>&nbsp;";
                }
                if (lineNew.StartsWith("^^"))
                {
                    lineNew = $"<p class=\"break\"><b><big>{lineNew[2..]}</big></b></p>";
                }
                else if (lineNew.StartsWith("^"))
                {
                    lineNew = $"<p class=\"break\"><b>{lineNew[1..]}</b></p>";
                }
                else if (lineNew.StartsWith("\t"))
                {
                    lineNew = $"<p class=\"quoteblock\">{lineNew[1..]}</p>";
                }
                else
                {
                    lineNew = $"<p>{lineNew}</p>";
                }
                result.AppendLine(lineNew);
            }
            return result.ToString();
        }

        private static string FixHtmlChars(string line)
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

        private static string FixHtmlPatterns(string line)
        {
            string lineNew = line;
            if (lineNew.Contains("\\\"")) lineNew = lineNew.Replace("\\\"", "\"");
            if (lineNew.Contains("\\'")) lineNew = lineNew.Replace("\\'", "'");
            if (lineNew.Contains("' \"")) lineNew = lineNew.Replace("' \"", "'&nbsp;\"");
            if (lineNew.Contains("\" '")) lineNew = lineNew.Replace("\" '", "\"&nbsp;'");
            return lineNew;
        }
    }
}
