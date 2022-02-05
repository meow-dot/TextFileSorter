using System;
using System.Text.RegularExpressions;

namespace Common
{
    public class FileRow
    {
        private const string StringRegexPattern = "(?<index>[\\d]*)\\.\\s(?<content>.*)";
        private const string RecombinedStringRegexPattern = "(?<content>.*)\\s\\s\\s\\s(?<index>[\\d]*)";

        public int Index { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return $"{Index}. {Content}";
        }

        public string ToRecombinedString()
        {
            return $"{Content}    {Index}";
        }

        public static FileRow CreateFromString(string str)
        {
            try
            {
                var regex = new Regex(StringRegexPattern);
                var match = regex.Match(str);
            
                return new FileRow()
                {
                    Index = Convert.ToInt32(match.Groups["index"].ToString()),
                    Content = match.Groups["content"].ToString(),
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public static string ToRecombinedString(string str)
        {
            try
            {
                var regex = new Regex(StringRegexPattern);
                var match = regex.Match(str);

                return $"{match.Groups["index"].ToString()}    {match.Groups["content"].ToString()}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static FileRow CreateFromString2(string str)
        {
            try
            {
                var indexOfPoint = str.IndexOf(". ");
                var index = Convert.ToInt32(str[..indexOfPoint]);
                var content = str[(indexOfPoint + 2)..];

                return new FileRow()
                {
                    Index = index,
                    Content = content,
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public static FileRow CreateFromString3(string str)
        {
            try
            {
                var indexOfPoint = str.IndexOf(".");
                var index = Convert.ToInt32(str[..indexOfPoint]);
                var content = str[(indexOfPoint + 2)..];

                return new FileRow()
                {
                    Index = index,
                    Content = content,
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static FileRow CreateFromRecombinedString(string str)
        {
            try
            {
                var regex = new Regex(RecombinedStringRegexPattern);
                var match = regex.Match(str);

                return new FileRow()
                {
                    Index = Convert.ToInt32(match.Groups["index"].ToString()),
                    Content = match.Groups["content"].ToString(),
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}