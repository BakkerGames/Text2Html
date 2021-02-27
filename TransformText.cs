using System.Text;

namespace Text2Html
{
    public static class TransformText
    {
        public static string ConvertText2Html(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return "";
            }
            StringBuilder result = new StringBuilder();

            result.Append(source); //TODO ### for testing ###

            return result.ToString();
        }
    }
}
