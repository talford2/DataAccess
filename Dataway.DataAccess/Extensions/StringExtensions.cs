using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Dataway.DataAccess.Extensions
{
    public static class StringExtensions
    {
        public static int GetCharCount(this string search, char character)
        {
            return search.Count(c => c == character);
        }

        public static string[] Split(this string value, string splitString)
        {
            return value.Split(new string[] { splitString }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitKeepingBrackets(this string value, string splitString, bool trimParts)
        {
            List<String> parts = new List<string>();
            parts.Add("");
            int index = 0;
            int nestedDept = 0;

            int charIndex = 0;
            foreach (char character in value)
            {
                if (character == '(')
                {
                    nestedDept++;
                }
                if (character == ')')
                {
                    nestedDept--;
                }

                if (nestedDept == 0)
                {
                    if (value.Length > charIndex + splitString.Length)
                    {
                        if (splitString == value.Substring(charIndex, splitString.Length))
                        {
                            parts.Add("");
                            index++;
                        }
                    }
                }

                parts[index] += character;
                charIndex++;
            }

            for (int i = 0; i < parts.Count; i++)
            {
                parts[i] = parts[i].TrimStart(splitString.ToCharArray());

                if (trimParts)
                {
                    parts[i] = parts[i].Trim();
                }
            }

            return parts.ToArray();
        }
    }
}
