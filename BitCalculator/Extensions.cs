using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevTools
{
    public static class Extensions
    {
        public static bool StartsWith(this string s, string start)
        {
            if (s.Length >= start.Length && s.Substring(0,start.Length) == start)
            {
                return true;
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

            return idxs.OrderBy(t=>t).First();
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
    }
}
