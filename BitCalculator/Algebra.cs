using System;
using System.Collections.Generic;
using System.Text;

namespace DevTools
{
    internal class Algebra
    {
        static IEnumerable<int> GetFactors(int n)
        {
            for (int i = 1; i <= Math.Abs(n); ++i)
            {
                if (n % i == 0)
                {
                    yield return i;
                    yield return -i;
                }
            }
        }
        public static string FactoriseCrissCross(int first, int second, int c, bool write = true)
        {
            foreach (var factor in GetFactors(first))
            {
                int topleft = factor;
                int bottomleft = first / factor;
                foreach (var factor2 in GetFactors(c))
                {
                    int topright = factor2;
                    int bottomright = c / factor2;
                    if (bottomleft * topright + topleft * bottomright == second)
                    {
                        string co1 = topleft == 1 ? "" : topleft.ToString();
                        string co2 = bottomleft == 1 || bottomleft == -1 ? "" : bottomleft.ToString();

                        co1 = topleft == -1 ? "-" : co1;
                        co2 = bottomleft == -1 ? "-" : co2;
                        if (write == true)
                        {
                            string s_topleft = topleft.ToString();
                            string s_topright = topright.ToString();
                            s_topright = s_topright[0] == '-' ? s_topright : s_topright.Insert(0, "+");

                            string s_bottomleft = bottomleft.ToString();
                            string s_bottomright = bottomright.ToString();
                            s_bottomright = s_bottomright[0] == '-' ? s_bottomright : s_bottomright.Insert(0, "+");


                            CustomConsole.PrintColour(string.Format("({0}x{1})({2}x{3})", s_topleft, s_topright, s_bottomleft, s_bottomright));
                        }
                        /**
                         * 3    5
                         *   --
                         * 4    5
                         * */
                        return string.Format("({0}x {4} {1})({2}x {5} {3})", co1, Math.Abs(topright), co2, Math.Abs(bottomright), topright >= 0 ? "+" : "-", bottomright >= 0 ? "+" : "-");
                    }
                }
            }
            Program.expectingError = true;
            throw new Exception("Could not factorize");
        }

    }
}
