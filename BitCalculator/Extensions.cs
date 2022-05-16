using System;
using System.Collections.Generic;
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
    }
}
