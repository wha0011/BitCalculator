using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            s = s.RemoveSpaces();

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
        public static int LettersLength(this string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                if (char.IsWhiteSpace(s[i]))
                {
                    return i;
                }
            }
            return s.Length;
        }
        public static string RemoveLineBreaks(this string v)
        {
            return Regex.Replace(Regex.Replace(v, "\n", ""), "\r", "");
        }
        public static string RemoveSpaces(this string input)
        {
            string result = "";
            bool inSpeech = false;
            bool isComment = false;
            string buffer = "";
            for (int i = 0; i < input.Length; ++i)
            {
                if (buffer.Contains(@"///"))
                {
                    result += input.Substring(i);
                    break;
                }
                if (buffer.Contains("//"))
                {
                    isComment = true;
                }
                if (buffer.Contains("\\\\"))
                {
                    isComment = false;
                    buffer = "";
                }
                char c = input[i];
                if (c == '\"')
                {
                    inSpeech = !inSpeech;
                    continue;
                }
                if (!inSpeech && !isComment)
                {
                    if (c != ' ' && c != '\0')
                    {
                        result += c.ToString().ToLower()[0];
                    }
                }
                else
                {
                    result += c;
                }
                buffer += c;
            }
            return result;
        }
        public static int NextBracket(this string input, int idx)
        {
            for (int i = idx + 1; i < input.Length; ++i)
            {
                if (input[i] == '(' || input[i] == ')')
                {
                    return i;
                }
            }
            return -1;
        }
        public static string RemoveLast(this string buffer)
        {
            if (buffer == "")
            {
                return "";
            }
            List<char> chars = buffer.ToArray().ToList();
            chars.RemoveAt(chars.Count() - 1);
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Finds the index of the closing bracket in a string
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <param name="curridx">where to search from in the string</param>
        /// <returns></returns>
        public static int ClosingBracket(this string sINPUT, int curridx)
        {
            int amountOfOpenBrackets = 1; //Assume that whatever is giving this string to us has alredy found an open bracket
            int amountOfClosingBrackets = 0; //We will need to look for the closing bracket
            for (int i = curridx; i < sINPUT.Length; ++i) //Iterate through the string from the curridx
            {
                if (sINPUT[i] == '(') //Is there a set of nested brackets?
                {
                    amountOfOpenBrackets++; //Increase the amount of open brackets, so that we have to find MORE closing brackets
                }
                if (sINPUT[i] == ')')
                {
                    amountOfClosingBrackets++; //One set of brackets has been closed
                }
                if (amountOfClosingBrackets == amountOfOpenBrackets) //Is there the same amount of open brackets as there are closing brackets
                                                                     //This symbolizes that all nested sets of brackets have been found
                {
                    return i; //Return the idx of the last bracket
                }
            }
            return sINPUT.Length - 1; //No closing bracket was found! just return the end of the string
        }

        public static int NextOperatorIDX(this string input, int currIDX)
        {
            for (int i = currIDX; i < input.Length; i++)
            {
                if (input[i] == '<' ||
                    input[i] == '>' ||
                    input[i] == '+' ||
                    input[i] == '-' ||
                    input[i] == '*' ||
                    input[i] == '/' ||
                    input[i] == '^' ||
                    input[i] == '&' ||
                    input[i] == '|' ||
                    input[i] == '=' ||
                    char.IsLetter(input[i]))
                {
                    return i;
                }
            }
            return input.Length;
        }
        public static int NextOperatorIDX_NoLetter(this string input, int currIDX)
        {
            for (int i = currIDX; i < input.Length; i++)
            {
                if (input[i] == '<' ||
                    input[i] == '>' ||
                    input[i] == '+' ||
                    input[i] == '-' ||
                    input[i] == '*' ||
                    input[i] == '/' ||
                    input[i] == '^' ||
                    input[i] == '&' ||
                    input[i] == '|' ||
                    input[i] == ')' ||
                    input[i] == '(' ||
                    input[i] == ',')
                {
                    return i;
                }
            }
            return input.Length;
        }

        public static int NextOperatorIDX_NoBrackets(this string input, int currIDX)
        {
            for (int i = currIDX; i < input.Length; i++)
            {
                if (input[i] == '<' ||
                    input[i] == '>' ||
                    input[i] == '+' ||
                    input[i] == '-' ||
                    input[i] == '*' ||
                    input[i] == '/' ||
                    input[i] == '^' ||
                    input[i] == '&' ||
                    input[i] == '|' ||
                    input[i] == ',' ||
                    char.IsLetter(input[i]))
                {
                    return i;
                }
            }
            return input.Length;
        }
        public static int NextColonIDX(this string input, int currIDX)
        {
            for (int i = currIDX; i < input.Length; i++)
            {
                if (input[i] == ':')
                {
                    return i;
                }
            }
            return input.Length;
        }

        public static int LastOperatorIDX(this string input, int currIDX)
        {
            for (int i = currIDX; i > -1; --i)
            {
                if (input[i] == '<' ||
                    input[i] == '>' ||
                    input[i] == '+' ||
                    input[i] == '-' ||
                    input[i] == '*' ||
                    input[i] == '/' ||
                    input[i] == '^' ||
                    input[i] == '&' ||
                    input[i] == '|' ||
                    char.IsLetter(input[i]))
                {
                    return i;
                }
            }
            return 0;
        }

        public static int LastAsterixIDX(this string input, int currIDX)
        {
            for (int i = currIDX; i > -1; --i)
            {
                if (input[i] == '*')
                {
                    return i;
                }
            }
            return -1;
        }

        public static int LastNegOperatorIDX(this string input, int currIDX)
        {
            for (int i = currIDX; i > -1; --i)
            {
                if (input[i] == '+' ||
                    input[i] == '-' ||
                    input[i] == '*' ||
                    input[i] == '/' ||
                    input[i] == '^' ||
                    input[i] == '&' ||
                    input[i] == '|' ||
                    char.IsLetter(input[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool IsOperator(this char c)
        {
            return c switch
            {
                '+' => true,
                '-' => true,
                '*' => true,
                '/' => true,
                '\\' => true,
                '|' => true,
                '^' => true,
                '&' => true,
                '>' => true,
                '<' => true,
                '=' => true,
                '!' => true,
                '#' => true,
                '(' => true,
                ')' => true,
                ':' => true,
                ';' => true,
                ',' => true,
                _ => false,
            };
        }
        /// <summary>
        /// Function searches for // to lead a comment and \\ to exit one
        /// It prints these to the console and removes them from the input string
        /// 
        /// It searches for the first versions of // and \\
        /// Removes them from the string, then recurses to see if there is a second set
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <returns></returns>
        public static string RemoveComments(this string sINPUT)
        {
            if (sINPUT.Contains(@"//") && sINPUT.Contains(@"\\") && !sINPUT.Contains("#defunc"))
            {
                string comment = sINPUT.Substring(sINPUT.IndexOf(@"//") + 2, sINPUT.IndexOf(@"\\") - sINPUT.IndexOf(@"//") - 2);
                //comment is the substring, +2 is to removing the leading //
                //-2 is to remove the following \\
                Colorful.Console.WriteLine(comment, Color.Beige); //Write the comment to the console
                return RemoveComments(sINPUT.Substring(0, sINPUT.IndexOf(@"//")) + sINPUT.Substring(sINPUT.IndexOf(@"\\") + 2));
                //use recursion to see if there are any more comments
            }
            else
            {
                return sINPUT;
            }
        }
        public static string RemoveAndReplace(this string input, int startIDX, int endIDX, string replaceWith)
        {
            return input.Substring(0, startIDX) + replaceWith + input.Substring(endIDX, input.Length - endIDX);
        }

        public static string TextBetween(this string input, int startIDX, int endIDX)
        {
            string result = "";
            if (endIDX == input.Length)
            {
                endIDX -= 1;
            }
            for (int i = startIDX; i < endIDX + 1; ++i)
            {
                result += input[i];
            }
            return result;
        }

        public static string AsBinary(this ulong input, bool flipped)
        {
            string firstResult = "";
            while (input >= 1)
            {
                ulong remainder = input % 2;
                firstResult = remainder + firstResult;
                input /= 2;
            }
            string secondResult = "";
            for (int i = 64 - firstResult.Length; i > 0; --i)
            {
                secondResult += "0";
            }
            secondResult += firstResult;
            string[] thirdResult = new string[8];
            int currIDX = 0;
            for (int i = 0; i < 64; ++i)
            {
                if (i % 8 == 0 && i != 0)
                {
                    ++currIDX;
                }
                thirdResult[currIDX] += secondResult[i];
                thirdResult[currIDX] += ' ';
            }
            string finalResult = "";
            for (int i = 0; i < thirdResult.Length; ++i)
            {
                if (flipped)
                {
                    finalResult += new string(thirdResult[i].Reverse().ToArray());
                }
                else
                {
                    finalResult += thirdResult[i];
                }
                finalResult += '\n';
            }
            return finalResult;
        }

        public static string AsBinary(this double input, bool flipped = false)
        {
            string binVal = Convert.ToString(BitConverter.DoubleToInt64Bits(input), 2);
            string result = "";
            for (int i = 0; i < 64 - binVal.Length; ++i)
            {
                result += '0';
            }
            result += binVal;
            string[] thirdResult = new string[8];
            int currIDX = 0;
            for (int i = 0; i < 64; ++i)
            {
                if (i % 8 == 0 && i != 0)
                {
                    ++currIDX;
                }
                thirdResult[currIDX] += result[i];
                thirdResult[currIDX] += ' ';
            }
            string finalResult = "";
            for (int i = 0; i < thirdResult.Length; ++i)
            {
                if (flipped)
                {
                    finalResult += new string(thirdResult[i].Reverse().ToArray());
                }
                else
                {
                    finalResult += thirdResult[i];
                }
                finalResult += '\n';
            }
            return finalResult;
        }
        public static string AsBinary(this float input, bool flipped = false)
        {
            string binVal = Convert.ToString(BitConverter.SingleToInt32Bits(input), 2);
            string result = "";
            for (int i = 0; i < 32 - binVal.Length; ++i)
            {
                result += '0';
            }
            result += binVal;
            string[] thirdResult = new string[8];
            int currIDX = 0;
            for (int i = 0; i < 32; ++i)
            {
                if (i % 8 == 0 && i != 0)
                {
                    ++currIDX;
                }
                thirdResult[currIDX] += result[i];
                thirdResult[currIDX] += ' ';
            }
            string finalResult = "";
            for (int i = 0; i < thirdResult.Length; ++i)
            {
                if (flipped)
                {
                    finalResult += new string(thirdResult[i].Reverse().ToArray());
                }
                else
                {
                    finalResult += thirdResult[i];
                }
                finalResult += '\n';
            }
            return finalResult;
        }

        public static string AddSpaces(this string input)
        {
            if (input == "") //Process empty input
            {
                return ""; //Just dont return anything
            }

            string result = " ";
            bool lookingForSpeech = false;
            for (int i = 0; i < input.Length-1; ++i) //Do not add a space on the end of the string
            {
                char c = input[i];
                char nextC = input[i + 1];
                result += c;
                if (c == '\"')
                {
                    lookingForSpeech = !lookingForSpeech;
                }
                if (lookingForSpeech)
                {
                    continue;
                }
                if (c == '#' || nextC == '#' || nextC == ' ' || c == '_' || nextC == '_') //Ignore hex, binary and spaces
                {
                    //Ignore this
                    continue;
                }
                if (!char.IsLetterOrDigit(c) ^ !char.IsLetterOrDigit(nextC)) //Are we an operator, or is the next one an operator
                    //If there are two operators in a row, assume that they are part of one operator, therefore dont add a space
                {
                    result += ' '; //Add spaces before and after anythign that is an operator
                }
            }
            result += input.Last(); //Add the final character from the input to the end of the result string
            result += ' ';
            return result;
        }

        public static string AsString(this List<string> list)
        {
            if (list.Count == 0)
            {
                return "";
            }
            string result = "";
            foreach (var item in list)
            {
                result += item;
                result += '\n';
            }
            result = result.Substring(0,result.Length-1);
            return result;
        }

        public static string ExactDecimal(this double d)
        {
            return d.ToString("0." + new string('#', 339));
        }
    }
}
