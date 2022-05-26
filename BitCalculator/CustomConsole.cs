using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DevTools
{
    internal class CustomConsole
    {
        public static bool readingConsole = false; //Staticbool that shows for async methods whether we are currently reading text from the console
        static string retString = ""; //Buffer for the readline function

        public static char[,] GetWrittenText()
        {
            var width = Console.WindowWidth;
            char[,] result = new char[width, retString.Length / width + 1];
            for (int y = 0; y < result.GetLength(1); ++y) //Iterate through rows
            {
                for (int x = 0; x < width; ++x) //Iterate through letters in row
                {
                    if (x+width*y >= retString.Length)
                    {
                        return result;
                    }
                    result[x, y] = retString[x+width*y]; //Add the position
                }
            }
            return result;
        }
        public static int startline;
        public static string ReadLineOrEsc()
        {
            startline = Console.CursorTop;
            retString = "";
            readingConsole = true;
            int writeIDX = 0;

            do
            {
                ConsoleKeyInfo readKeyResult = Console.ReadKey(true);

                if (readKeyResult.Key == ConsoleKey.Enter) //Return the buffer
                {
                    Console.WriteLine();
                    readingConsole = false;
                    //Console.CursorTop = startline;
                    return retString;
                }

                if (readKeyResult.Key == ConsoleKey.LeftArrow) //Move left
                {
                    writeIDX--;
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                }
                else if (readKeyResult.Key == ConsoleKey.RightArrow) //Mofe right
                {
                    if (retString.Length > writeIDX)//Not at end yet?
                    {
                        writeIDX++;
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                }
                else if (readKeyResult.Key == ConsoleKey.UpArrow || readKeyResult.Key == ConsoleKey.DownArrow || readKeyResult.Key == ConsoleKey.Delete || readKeyResult.Key == ConsoleKey.Tab ||
                         readKeyResult.Key == ConsoleKey.LeftWindows || readKeyResult.Key == ConsoleKey.RightWindows || readKeyResult.Key == ConsoleKey.Escape)
                {
                    //Do nothing, just dont run other functions

                    //Delete does not work in this application as it moves the cursor uncrontrollably
                    //To avoid this error, we just don't cater for it
                }

                // handle backspace
                else if (readKeyResult.Key == ConsoleKey.Backspace)
                {
                    if (writeIDX != 0) //Don't delete characters before the first one
                    {
                        retString = retString.Remove(writeIDX - 1, 1); //Remove current char from the buffer
                        ChangeUserTextColourLive(retString); //Change user text colour
                        try
                        {
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop); //Move the cursor to its new position
                        }
                        catch
                        {
                            Console.SetCursorPosition(Console.CursorLeft + Console.WindowWidth - 1, Console.CursorTop - 1); //Move the cursor to its new position
                        }
                        writeIDX--; //Update current writeIDX
                    }
                }
                else
                {
                    if (retString.Length <= writeIDX)//Writing next character?
                    {
                        retString += readKeyResult.KeyChar; //Add to the buffer
                        if (Console.WindowWidth == Console.CursorLeft + 1) //At end of line?
                        {
                            Console.CursorTop++; //Go to the next line
                            Console.CursorLeft = 0; //Do not put text behind the header
                        }
                        else
                        {
                            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop); //Move the cursor right
                        }
                        writeIDX++;
                    }
                    else if (writeIDX >= 0)//We have moved the idx?
                    {
                        retString = retString.Insert(writeIDX, readKeyResult.KeyChar.ToString());
                        writeIDX++;
                        if (Console.WindowWidth == Console.CursorLeft + 1) //At end of line?
                        {
                            Console.CursorTop++; //Go to the next line
                            Console.CursorLeft = 0; //Do not put text behind the header
                        }
                        else
                        {
                            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop); //Move the cursor right
                        }
                    }
                    ChangeUserTextColourLive(retString); //Change the colour of the users text
                }
                if (Console.CursorLeft <= 2 && Console.CursorTop == startline)
                {
                    Console.CursorLeft = 3;
                    writeIDX = 0;
                }
            }
            while (true);
        }
        public static bool NetworkingPrint(string toprint)
        {
            if (readingConsole) //Are we currently reading user input?
            {
                var left = Console.CursorLeft;
                Console.CursorLeft = 0;
                ClearCurrentConsoleLine();
                Colorful.Console.WriteLine(toprint, Color.FromArgb(184, 186, 255));
                Colorful.Console.Write("-->", Color.FromArgb(10, 181, 158)); //Header for text

                PrintColour(retString, false, false);
                Console.CursorLeft = left; //Reset the left to its original position
            }
            else
            {
                Colorful.Console.WriteLine(toprint, Color.FromArgb(184, 186, 255));
            }
            return false;
        }

        static Size consoleSize = new Size(50,30);

        static System.Timers.Timer checkTimer = new System.Timers.Timer();
        public static void SetupConsole()
        {
            Console.SetWindowSize(150,30);
            checkTimer.Interval = 10;
            checkTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimerTick);
            checkTimer.Start();

            Colorful.Console.BackgroundColor = Color.FromArgb(0, 16, 29); //Change the background colour to the snazzy blue

            Colorful.Console.Clear();
            Colorful.Console.OutputEncoding = Encoding.Unicode;
            Colorful.Console.WriteAsciiStyled("Dev Tools 2022", new Colorful.StyleSheet(Color.FromArgb(122, 224, 255)));
            Colorful.Console.WriteLine("Type help to show all functions", Color.FromArgb(122, 224, 255));
            Colorful.Console.ForegroundColor = Color.FromArgb(10, 181, 158);
            
        }

        public static void TimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Console.WindowWidth != consoleSize.Width) //Console size has been changed?
            {
                startline = Console.CursorTop - GetWrittenText().GetLength(1)+1;
                consoleSize.Width = Console.WindowWidth;
            }
        }

        struct FuncLocation
        {
            public int start;
            public int end;
            public string name;

            public FuncLocation(int start, int end, string name)
            {
                this.start = start;
                this.end = end;
                this.name = name;
            }
        }       
        /// <summary>
        /// An extension method for Colourful.Console.Writeline()
        /// </summary>
        /// <param name="vlower"></param>
        /// <param name="workings"></param>
        /// <param name="isBin"></param>
        public static void PrintColour(string v, bool isBin = false, bool writeline = true)
        {
            var vlower = v.ToLower();

            //Checking for functions
            #region functionCheck
            var AllFunctions = File.ReadAllLines(Program.FuncFilePath);
            List<FuncLocation> funclocations = new List<FuncLocation>();
            foreach (var s in File.ReadAllLines(Program.FuncFilePath))
            {
                if (s == "")
                {
                    continue;
                }
                if (s == "SYSTEM FUNCTIONS:")
                {
                    break;
                }
                var name = s.Split('(')[0];
                if (vlower.Contains(name) && name.Length >= 2) //Name is in the users input?
                {
                    foreach (var idx in vlower.AllIndexs(name))
                    {
                        if (idx == 0 || !char.IsLetter(vlower[idx - 1])) //Is this the start of the word?
                        {
                            if (idx + name.Length < vlower.Length && !char.IsLetter(vlower[idx + name.Length]))//Is this the end of a word?
                            {
                                funclocations.Add(new FuncLocation(vlower.IndexOf(name), vlower.ClosingBracket(vlower.IndexOf(name) + name.Length + 1), name));
                            }
                        }
                    }
                }
            }

            bool atsystem = false;
            List<FuncLocation> sysfunclocations = new List<FuncLocation>();
            foreach (var s in File.ReadAllLines(Program.FuncFilePath))
            {
                if (s == "SYSTEM FUNCTIONS:")
                {
                    atsystem = true;
                    continue;
                }
                if (!atsystem)
                {
                    continue;
                }
                var name = s.Split('(')[0];
                if (vlower.Contains(name) && name.Length >= 1) //Name is in the users input?
                {
                    foreach (var idx in vlower.AllIndexs(name))
                    {
                        if (idx == 0 || !char.IsLetter(vlower[idx - 1])) //Is this the start of the word?
                        {
                            if (idx + name.Length >= vlower.Length || !char.IsLetter(vlower[idx + name.Length]))//Is this the end of a word?
                            {
                                sysfunclocations.Add(new FuncLocation(idx, idx + name.Length, name));
                            }
                        }
                    }
                }
            }

            List<FuncLocation> variableLocations = new List<FuncLocation>();
            foreach (var s in File.ReadAllLines(Program.DataFilePath))
            {
                if (!s.Contains(',')) //No comma in the line?
                {
                    File.WriteAllText(Program.DataFilePath, ""); //Clear the file
                    Program.expectingError = true;
                    throw new Exception("Variables file corrupted. File cleared");
                }
                var name = s.Split(',')[0];
                if (vlower.Contains(name)) //Name is in the users input?
                {
                    foreach (var idx in vlower.AllIndexs(name))
                    {
                        variableLocations.Add(new FuncLocation(idx, idx + name.Length, name));
                    }
                }
            }
            foreach (var name in Variables.tempVariables.Keys)
            {
                if (vlower.Contains(name)) //Name is in the users input?
                {
                    foreach (var idx in vlower.AllIndexs(name))
                    {
                        if (idx == 0 || !char.IsLetter(vlower[idx - 1])) //Is this the start of the word?
                        {
                            if (idx + name.Length >= vlower.Length || !char.IsLetter(vlower[idx + name.Length]))//Is this the end of a word?
                            {
                                variableLocations.Add(new FuncLocation(idx, idx + name.Length, name));
                            }
                        }
                    }
                }
            }
            foreach (var name in Variables.networkingVariables.Keys)
            {
                if (vlower.Contains(name)) //Name is in the users input?
                {
                    foreach (var idx in vlower.AllIndexs(name))
                    {
                        if (idx == 0 || !char.IsLetter(vlower[idx - 1])) //Is this the start of the word?
                        {
                            if (idx + name.Length >= vlower.Length || !char.IsLetter(vlower[idx + name.Length]))//Is this the end of a word?
                            {
                                variableLocations.Add(new FuncLocation(idx, idx + name.Length, name));
                            }
                        }
                    }
                }
            }

            List<FuncLocation> speechLocations = new List<FuncLocation>();
            bool lookingForSpeech = false;
            int lastidx = -1;
            for (int i = 0; i < v.Length; ++i)
            {
                char c = v[i];
                if (c == '\"') //In speechmarks?
                {
                    if (lookingForSpeech)
                    {
                        lookingForSpeech = false;
                        speechLocations.Add(new FuncLocation(lastidx, i + 1, ""));
                        lastidx = -1;
                    }
                    else
                    {
                        lastidx = i;
                        lookingForSpeech = true;
                    }
                }
            }
            if (lastidx != -1) //Is there a lonely speech mark?
            {
                speechLocations.Add(new FuncLocation(lastidx, v.Length, "")); //All text after that speech mark is marked as text   
            }
            #endregion
            funclocations = funclocations.OrderBy(f => f.start).ToList();
            sysfunclocations = sysfunclocations.OrderBy(f => f.start).ToList();
            variableLocations = variableLocations.OrderBy(f => f.start).ToList();
            speechLocations = speechLocations.OrderBy(f => f.start).ToList();
            //Order the functions so that the ones that appear first are at the start of the list

            if (isBin)
            {
                foreach (var c in v)
                {
                    if (c == '0')
                    {
                        Colorful.Console.Write(c, Color.FromArgb(90, 255, 183));
                    }
                    else
                    {
                        Colorful.Console.Write(c, Color.FromArgb(10, 181, 158));
                    }
                } //Binary font
            }
            else
            {
                bool isyellow = false;
                bool printingFunction = false;
                bool printingsysFunction = false;
                bool printingDefine = false;
                bool printingSpeech = false;
                FuncLocation current = new FuncLocation(int.MaxValue, int.MaxValue, "");
                FuncLocation currentsys = new FuncLocation(int.MaxValue, int.MaxValue, "");
                FuncLocation currentdefine = new FuncLocation(int.MaxValue, int.MaxValue, "");
                FuncLocation currentspeech = new FuncLocation(int.MaxValue, int.MaxValue, "");
                for (int i = 0; i < v.Length; i++)
                {
                    if (currentsys.end == i)
                    {
                        printingsysFunction = false;
                    }
                    if (currentdefine.end == i)
                    {
                        printingDefine = false;
                    }
                    if (currentspeech.end == i)
                    {
                        printingSpeech = false;
                    }

                    if (funclocations.Count != 0 && funclocations[0].start == i) //Are we at the start idx of a function?
                    {
                        current = funclocations[0];
                        printingFunction = true;
                        funclocations.RemoveAt(0); //Remove it from the list
                    }
                    if (sysfunclocations.Count != 0 && sysfunclocations[0].start == i) //Are we at the start idx of a system function?
                    {
                        currentsys = sysfunclocations[0];
                        printingsysFunction = true;
                        sysfunclocations.RemoveAt(0); //Remove it from the list
                    }
                    if (variableLocations.Count != 0 && variableLocations[0].start == i) //Are we at the start idx of a system function?
                    {
                        currentdefine = variableLocations[0];
                        printingDefine = true;
                        variableLocations.RemoveAt(0); //Remove it from the list
                    }
                    if (speechLocations.Count != 0 && speechLocations[0].start == i) //Are we at the start idx of a system function?
                    {
                        currentspeech = speechLocations[0];
                        printingSpeech = true;
                        speechLocations.RemoveAt(0); //Remove it from the list
                    }

                    char c = v[i];

                    if (printingFunction) //Are we printing out a function?
                    {
                        if (i < current.start + current.name.Length) //Are we in the name of the function?
                        {
                            Colorful.Console.Write(c, Color.FromArgb(168, 0, 149));
                        }
                        else if (i == current.start + current.name.Length && c == '(') //Opening bracket?
                        {
                            Colorful.Console.Write(c, Color.FromArgb(255, 255, 255));
                        }
                        else if (i == current.end && c == ')') //Closing bracket?
                        {
                            Colorful.Console.Write(c, Color.FromArgb(255, 255, 255));
                        }
                        else
                        {
                            Colorful.Console.Write(c, Color.FromArgb(10, 181, 158));
                        }
                    }
                    else if (printingsysFunction)
                    {
                        Colorful.Console.Write(c, Color.FromArgb(247, 255, 161));
                    }
                    else if (printingSpeech)
                    {
                        Colorful.Console.Write(c, Color.Beige);
                    }
                    else if (printingDefine)
                    {
                        Colorful.Console.Write(c, Color.FromArgb(168, 0, 149));
                    }
                    else
                    {
                        if (isyellow && !c.IsOperator() && c != ' ')
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
                        else if (c.IsOperator() || c == ' ')
                        {
                            isyellow = false;
                            Colorful.Console.Write(c, Color.FromArgb(130, 253, 255));
                        }
                        else
                        {
                            Colorful.Console.Write(c, Color.FromArgb(10, 181, 158));
                        }
                    }
                    if (current.end == i) //Are we at the end of a function?
                    {
                        printingFunction = false;
                    }
                }
            }
            if (writeline)
            {
                Colorful.Console.WriteLine();
            }
            Program.lastprint = v;
        }
        public static void ChangeUserTextColour(string userinput)
        {
            var x = Console.CursorLeft;
            var y = Console.CursorTop;
            y--; //Get the row above us
            Console.SetCursorPosition(x, y);
            Console.WriteLine(); //Clear previous line
            Console.SetCursorPosition(x, y);

            Colorful.Console.Write("-->", Color.FromArgb(255, 181, 158)); //Re-print the header

            PrintColour(userinput);
        }
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
        public static void ChangeUserTextColourLive(string userinput)
        {
            Console.CursorVisible = false;
            var x = Console.CursorLeft;
            var y = Console.CursorTop;

            var arry = GetWrittenText();
            for (int i = startline; i <= y+1; ++i)
            {
                Console.SetCursorPosition(0, i);
                ClearCurrentConsoleLine();
            }


            Console.SetCursorPosition(0, startline);
            Colorful.Console.Write("-->", Color.FromArgb(10, 181, 158)); //Header for text

            Console.SetCursorPosition(3, startline);

            PrintColour(userinput, false, false);
            Console.SetCursorPosition(x, y);
            Console.CursorVisible = true;
        }
        public static void PrintError(string errorMessage)
        {
            Colorful.Console.WriteLine(errorMessage, Color.Red);
        }
        enum PrintType
        {
            Comment,
            Command,
        }
        struct Comment
        {
            public string s;
            public PrintType printType;

            public Comment(string s, PrintType printType)
            {
                this.s = s;
                this.printType = printType;
            }
        }
        /// <summary>
        /// Descriptions are marked with ///
        /// This function prints the descriptions to the console
        /// 
        /// Mark with ** where you want things to be recognised as normal commands
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <returns></returns>
        public static string ShowDescription(string sINPUT)
        {
            sINPUT = Regex.Replace(sINPUT, @"<new>", "\n"); //Replace all \n with new lines

            if (sINPUT.Contains(@"///"))
            {
                string comment = sINPUT.Substring(sINPUT.IndexOf(@"///") + 3); //Find the end of the first ///. +3 to get to the end of the "///"
                List<Comment> toprint = new List<Comment>();
                string buffer = "";
                bool lookingForAsterix = false;
                for (int i = 0; i < comment.Length; i++)
                {
                    char c = comment[i];
                    if (c == '\\') //Is it a \
                                   //Uses \\, but is really looking for \
                    {
                        toprint.Add(new Comment(buffer, PrintType.Comment)); //Add this as being normal
                        buffer = "";
                        break; //UNTESTED IMPROVEMENT
                    }
                    else if (c == '*') //Asterix shows we want to print out as code
                    {
                        if (lookingForAsterix) //Are we looking for the ending asterix?
                        {
                            toprint.Add(new Comment(buffer, PrintType.Command)); //Add this as being code
                            lookingForAsterix = false;
                            buffer = "";
                        }
                        else
                        {
                            toprint.Add(new Comment(buffer, PrintType.Comment)); //Add this as being normal
                            lookingForAsterix = true;
                            buffer = "";
                        }
                    }
                    else
                    {
                        buffer += c;
                    }
                }
                foreach (var c in toprint)
                {
                    switch (c.printType)
                    {
                        case PrintType.Comment:
                            Colorful.Console.Write(c.s, Color.Beige);
                            break;
                        case PrintType.Command:
                            PrintColour(c.s, false, false);
                            break;
                    }
                }
                Console.WriteLine();
            }
            return sINPUT;
        }

        public static void WriteAscii(string text)
        {
            Colorful.Console.WriteAsciiStyled(text, new Colorful.StyleSheet(Color.FromArgb(122, 224, 255)));
        }


        #region printDecimals
        /// <summary>
        /// Prints a double decimal values binary output
        /// </summary>
        /// <param name="input"></param>
        public static void PrintDouble(string input)
        {
            var res = Regex.Replace(input.RemoveSpaces(), "\n", ""); //Remove all spaces, new lines or blanks from string


            for (int i = 0; i < 64; ++i)
            {
                if (i == 0)
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(255, 255, 255));
                }
                else if (i <= 11)
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(0, 255, 10));
                }
                else
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(255, 100, 100));
                }
                if ((i + 1) % 8 == 0 && i != 0)
                {
                    Colorful.Console.Write('\n');
                }
                else
                {
                    Colorful.Console.Write(' ');
                }
            }
            Colorful.Console.WriteLine("This colour is the mantissa", Color.FromArgb(255, 100, 100));
            Colorful.Console.WriteLine("This colour is the exponent", Color.FromArgb(0, 255, 10));
            Colorful.Console.WriteLine("This colour is the sign", Color.FromArgb(255, 255, 255));
        }
        /// <summary>
        /// Prints a float decimal values binary output
        /// </summary>
        /// <param name="input"></param>
        public static void PrintFloat(string input)
        {
            var res = Regex.Replace(input.RemoveSpaces(), "\n", "");
            for (int i = 0; i < 32; ++i)
            {
                if (i == 0)
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(255, 255, 255));
                }
                else if (i <= 9)
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(0, 255, 10));
                }
                else
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(255, 100, 100));
                }
                if ((i + 1) % 8 == 0 && i != 0)
                {
                    Colorful.Console.Write('\n');
                }
                else
                {
                    Colorful.Console.Write(' ');
                }
            }
            //Colorful.Console.BackgroundColor = Color.FromArgb(0, 16, 29);
            Colorful.Console.WriteLine("This colour is the mantissa", Color.FromArgb(255, 100, 100));
            Colorful.Console.WriteLine("This colour is the exponent", Color.FromArgb(0, 255, 10));
            Colorful.Console.WriteLine("This colour is the sign", Color.FromArgb(255, 255, 255));
        }
        #endregion

        /// <summary>
        /// Prints Hex in a snazzy colour
        /// </summary>
        /// <param name="v"></param>
        public static void PrintHex(string v)
        {
            Colorful.Console.WriteLine(v.Insert(0, "#"), Color.FromArgb(234, 255, 0));
        }

    }
}
