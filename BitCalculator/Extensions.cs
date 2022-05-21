using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevTools
{
    public static class Extensions
    {
        public static bool BeginsWith(this string s, string start)
        {
            if(s == "")
            {
                return false;
            }

            if (s.Length >= start.Length && s.Substring(0,start.Length) == start)
            {
                return true;
            }

            if (s[0] == '(') //Starts with can include if it starts with a bracket, then the term
            {
                s = s.Substring(1);
                return s.BeginsWith(start);
            }
            return false;
        }

        public static int IndexOfCondition(this string s)
        {
            List<int> idxs = new List<int>();

            idxs.Add(s.IndexOf("=="));
            idxs.Add(s.IndexOf("!="));
            idxs.Add(s.IndexOf(">"));
            idxs.Add(s.IndexOf("<"));

            return idxs.OrderBy(t=>t).Where(i=>i>=0).First();
        }

        public static bool IsConditionary(this char c)
        {
            if (c== '=' || c== '<' || c== '>' || c== '!')
            {
                return true;
            }
            return false;
        }

        public static string[] SplitAtFirst(this string s, char c)
        {
            if (!s.Contains(c))
            {
                return new string[1] { s};
            }

            var after = s.Substring(s.IndexOf(c)+1);
            var before = s.Substring(0,s.IndexOf(c));

            return new string[] {before,after};
        }
        public static List<int> AllIndexs(this string s, string lookfor)
        {
            List<int> result = new List<int>();

            string buffer = "";
            for (int i = 0; i < s.Length; ++i)
            {
                buffer += s[i];
                if (buffer.Contains(lookfor)) //Have we just reached the string
                {
                    buffer = ""; //Clear the buffer
                    result.Add(i-lookfor.Length+1); //Return the start idx of the string we are looking for
                }
            }

            return result;
        }
    }
}
