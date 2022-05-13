using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DevTools
{
    static class BinaryNumASCI
    {
        public static string[] NO_0 = new string[] {"00111100" ,
                                    "01000010" ,
                                    "01000010" ,
                                    "01000010" ,
                                    "00111100"};

        public static string[] NO_1 = new string[] {"00011000" ,
                                    "00101000" ,
                                    "00001000" ,
                                    "00001000" ,
                                    "00111100"};

        public static string[] NO_2 = new string[] {"00111100" ,
                                    "01000100" ,
                                    "00011000" ,
                                    "00100000" ,
                                    "01111100" };

        public static string[] NO_3 = new string[] {"00111100" ,
                                    "00000010" ,
                                    "00001100" ,
                                    "00000010" ,
                                    "00111100" };

        public static string[] NO_4 = new string[] {"00001100" ,
                                    "00010100" ,
                                    "00100100" ,
                                    "01111111" ,
                                    "00000100" };

        public static string[] NO_5 = new string[] {"01111110" ,
                                    "01000000" ,
                                    "01111110" ,
                                    "00000010" ,
                                    "01111110"};

        public static string[] NO_6 = new string[] {"01111110" ,
                                    "01000000" ,
                                    "01111110" ,
                                    "01000010" ,
                                    "01111110" };

        public static string[] NO_7 = new string[] {"01111110" ,
                                    "00000100" ,
                                    "00001000" ,
                                    "00010000" ,
                                    "00100000" };

        public static string[] NO_8 = new string[] {"01111110" ,
                                    "01000010" ,
                                    "01111110" ,
                                    "01000010" ,
                                    "01111110" };

        public static string[] NO_9 = new string[] {"00111110" ,
                                    "00100010" ,
                                    "00111110" ,
                                    "00000010" ,
                                    "00000010" };

        public static string[] Minus = new string[] {"00000000" ,
                                     "00000000" ,
                                     "01111110" ,
                                     "00000000" ,
                                     "00000000" };

        public static string[] Decimal = new string[] {"000000000" ,
                                       "000000000" ,
                                       "000000000" ,
                                       "000111000" ,
                                       "000111000"};

        public static void PrintConverted(string input)
        {
            List<string[]> asciiLetters = new List<string[]>();
            foreach (var c in input)
            {
                switch (c)
                {
                    case '0':
                        asciiLetters.Add(NO_0);
                        break;
                    case '1':
                        asciiLetters.Add(NO_1);
                        break;
                    case '2':
                        asciiLetters.Add(NO_2);
                        break;
                    case '3':
                        asciiLetters.Add(NO_3);
                        break;
                    case '4':
                        asciiLetters.Add(NO_4);
                        break;
                    case '5':
                        asciiLetters.Add(NO_5);
                        break;
                    case '6':
                        asciiLetters.Add(NO_6);
                        break;
                    case '7':
                        asciiLetters.Add(NO_7);
                        break;
                    case '8':
                        asciiLetters.Add(NO_8);
                        break;
                    case '9':
                        asciiLetters.Add(NO_9);
                        break;
                    case '.':
                        asciiLetters.Add(Decimal);
                        break;
                    case '-':
                        asciiLetters.Add(Minus);
                        break;
                }
            }
            string[] results = new string[5];
            foreach (var sarray in asciiLetters)
            {
                results[0] += sarray[0];
                results[1] += sarray[1];
                results[2] += sarray[2];
                results[3] += sarray[3];
                results[4] += sarray[4];
            }
            results[0] += '\n';
            results[1] += '\n';
            results[2] += '\n';
            results[3] += '\n';
            results[4] += '\n';
            string toprint = "";
            foreach (var s in results)
            {
                toprint += s;
            }
            PrintRedAndWhite(toprint, true);
        }
        public static void PrintRedAndWhite(string v, bool isBin = false)
        {
            if (isBin)
            {
                foreach (var c in v)
                {
                    if (c == '0')
                    {
                        Colorful.Console.Write(c, Color.FromArgb(255, 0, 0));
                    }
                    else
                    {
                        Colorful.Console.Write(c, Color.FromArgb(255, 255, 255));
                    }
                }
            }
            else
            {
                bool isyellow = false;
                for (int i = 0; i < v.Length; i++)
                {
                    char c = v[i];
                    if (isyellow && !Program.IsOperator(c) && c != ' ')
                    {
                        Colorful.Console.Write(c, Color.FromArgb(234, 255, 0));
                        continue;
                    }

                    if (char.IsNumber(c))
                    {
                        Colorful.Console.Write(c, Color.FromArgb(6, 153, 255));
                    }
                    else if (c == '#')
                    {
                        isyellow = true;
                        Colorful.Console.Write(c, Color.FromArgb(234, 255, 0));
                    }
                    else if (Program.IsOperator(c) || c == ' ')
                    {
                        isyellow = false;
                        Colorful.Console.Write(c, Color.FromArgb(130, 253, 255));
                    }
                    else
                    {
                        Colorful.Console.Write(c, Color.FromArgb(10, 181, 158));
                    }
                }
            }
            Colorful.Console.WriteLine();
        }

    }
}
