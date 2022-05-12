using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DevTools
{
    class Program
    {
        struct OperatorNum
        {
            public ulong _firstNum;
            public ulong _secondNum;
            public char _operator;

            public OperatorNum(ulong firstNum, ulong secondNum, char @operator)
            {
                _firstNum = firstNum;
                _secondNum = secondNum;
                _operator = @operator;
            }
        }   
        static ulong lastInput = 0ul;
        static bool defaultFlipVal = false;
        static void Main(string[] args)
        {
            CheckDirectories(); //See if the file storing directories exist, if not, then create them
            SetupConsole();

            bool first = true;
            while (true)
            {
                try
                {
                    Colorful.Console.Write("-->", Color.FromArgb(10, 181, 158)); //Header for text
                    string userInput = "";
                    if (args.Length != 0 && first) //Are we opening a file?
                    {
                        PrintColour("Opening file: " + args[0]);
                        var extension = args[0].Split('.')[1]; //Get the file extension from the filepath
                        if (extension != "dcode")
                        {
                            PrintColour("Unrecognized file extension: " + extension); //Only open .dcode files
                        }
                        else
                        {
                            string x = RemoveLineBreaks(File.ReadAllText(args[0])); //Read the file in
                            DoMainMethod(x); //Run the functions in the file
                        }
                        first = false; //This is no longer the first loop, set first to false
                    }
                    else
                    {
                        userInput = Colorful.Console.ReadLine();
                        DoMainMethod(userInput);
                    }
                }
                catch (Exception e)
                {
                    Colorful.Console.WriteLine("INVALID", Color.FromArgb(255, 10, 10));
                    Colorful.Console.WriteLine(e.Message, Color.FromArgb(255, 10, 10));
                    //Colorful.Console.WriteLine(e.StackTrace, Color.FromArgb(255, 10, 10));
                }
            }
        }

        private static void SetupConsole()
        {
            Colorful.Console.BackgroundColor = Color.FromArgb(0, 16, 29); //Change the background colour to the snazzy blue

            Colorful.Console.Clear();
            Colorful.Console.OutputEncoding = Encoding.Unicode;
            Colorful.Console.WriteAsciiStyled("Dev Tools 2022", new Colorful.StyleSheet(Color.FromArgb(122, 224, 255)));
            Colorful.Console.WriteLine("Type help to show all functions", Color.FromArgb(122, 224, 255));
            Colorful.Console.ForegroundColor = Color.Magenta;
        }

        /// <summary>
        /// Checks to see if directories are valid. re-creates files if nessecary
        /// </summary>
        private static void CheckDirectories()
        {
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
            if (!File.Exists(DataFilePath))
            {
                File.CreateText(DataFilePath);
            }
            if (!File.Exists(WorkingsFilePath))
            {
                File.CreateText(WorkingsFilePath);
            }
            if (!File.Exists(FuncFilePath))
            {
                File.CreateText(FuncFilePath);
            }

            try
            {
                if (File.ReadAllText(WorkingsFilePath) == "")
                {
                    File.WriteAllText(WorkingsFilePath, printWorkings.ToString());
                }
                printWorkings = bool.Parse(File.ReadAllText(WorkingsFilePath));
            }
            catch
            {
                Colorful.Console.WriteLine("DO NOT EDIT SYSTEM FILES. WORKINGS FILE HAS BEEN RESET");
            }
        }

        private static string RemoveLineBreaks(string v)
        {
            return Regex.Replace(Regex.Replace(v, "\n", ""), "\r", "");
        }

        private static string RemoveSpaces(string input)
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
                    if (c != ' ')
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
        public static void DoMainMethod(string userINPUT)
        {
            if (!userINPUT.Contains("loop") && !userINPUT.Contains("#defunc")) 
                //Only run multiple functions if function is not a loop or defining a function
            {
                foreach (var s in userINPUT.Split(';')) //Split up the different user commands
                {
                    MainMethod(s); //Run the main method on them
                }
            }
            else
            {
                if (userINPUT.IndexOf(";") != 0 && userINPUT.IndexOf(";") < userINPUT.IndexOf("loop") && !userINPUT.Contains("#def"))
                    //Run the functions that are called before the loop. After the loop statement, all semicolons are assumed to be inside it
                {
                    var beforeLoop = userINPUT.Substring(0,userINPUT.IndexOf("loop"));
                    foreach (var str in beforeLoop.Split(';'))
                    {
                        MainMethod(str);
                    }
                    userINPUT = userINPUT.Substring(beforeLoop.Length); //Remove the previous statements from the userinputs
                }
                MainMethod(userINPUT); //Run the mainmethod, with the changed length, or unmodified if there were no previous statements
            }
        }
        /// <summary>
        /// Write snazzy text in the console that is big
        /// </summary>
        /// <param name="text">text to print</param>
        public static void WriteAscii(string text)
        {
            Colorful.Console.WriteAsciiStyled(text, new Colorful.StyleSheet(Color.FromArgb(122, 224, 255)));
        }
        public static void MainMethod(string userINPUT)
        {
            userINPUT = RemoveSpaces(userINPUT);
            userINPUT = RemoveComments(userINPUT); //Remove all 

            #region uservariables
            if (userINPUT.Length >= 8 && userINPUT.Substring(0,7) == "#define") //Are we defining a variable?
            {
                DefineVariable(userINPUT); //Define the veriable with the users input
                return;
            }
            if (userINPUT.Length >= 8 && userINPUT.Substring(0,7) == "#defunc")
            {
                DefineFunction(userINPUT); //Define a function with the new input
                return;
            }
            if (userINPUT.Length >= 8 && userINPUT.Substring(0, 8) == "#delfunc")
            {
                DeleteFunction(userINPUT.Substring(8)); //Delete the function
                return;
            }
            if (userINPUT.Length >= 4 && userINPUT.Substring(0, 4) == "#del")
            {
                DeleteVariable(userINPUT.Substring(4)); //Delete the variable
                return;
            }

            //Display/show variables
            if (userINPUT == "showfunc") //Show the user defined functions
            {
                PrintColour(File.ReadAllText(FuncFilePath));
                return;
            }
            if (userINPUT.ToLower() == "dv") //Display all the user defined variables
            {
                foreach (var i in File.ReadAllLines(DataFilePath)) //Iterate through all the lines
                {
                    string copy = Regex.Replace(i, ",", " = "); //Replace the csv style commas with more user friendly " = "
                    PrintColour(copy, false); //Print the new variable
                }
                return;
            }
            if (userINPUT.ToLower() == "dtv") //Display all the user defined temp variables
            {
                foreach (var i in tempVariables) //Iterate through all the variables
                {
                    PrintColour(string.Format("{0} = {1}", i.Key, i.Value), false); //Print them to the screen
                }
                return;
            }
            #endregion


            if (userINPUT == "exit" || userINPUT == "quit") //close the app?
            {
                Environment.Exit(0);
            }
            if (userINPUT.Length >= 5 && userINPUT.Substring(0,5) == "help-") //Show help for specific function. Specified after the -
            {
                PrintDescription(userINPUT.Substring(5)); //Print the help
                return;
            }
            var resetworkings = false;
            if (userINPUT.Length >= 3 && userINPUT.Substring(0,2) == "nw") //User wants to print with no workings?
            {
                printWorkings = false; //Stop printing workings
                userINPUT = userINPUT.Substring(2); //remove the "nw" from the userinput string
                if (!resetworkings)
                {
                    resetworkings = true; //Change the workings value back to normal when we are done
                }
            }
            if (userINPUT == "alg") //Generate algebra
            {
                GenerateAlgebra(); //DOES NOT WORK. REQUIRES IMPLEMENTATION
                return;
            }

            userINPUT = ReplaceVariables(userINPUT); //Remove all custom user variables from the string


            if (userINPUT.Length >= 4 && userINPUT.Substring(0,4) == "loop") //User wants to do a loop?
            {
                DoLoopFunc(userINPUT); //Do the loop, then exit
                return;
            }
            #region showdecimals
            //Show value as decimal
            if (userINPUT.Length >= 4 && userINPUT.Substring(0,4) == "doub") //User wants to show value as double
            {
                PrintDouble(DoubleToBin(double.Parse(userINPUT.Substring(4)))); //Print the new double value
                double userDoubleInput = double.Parse(userINPUT.Substring(4));
                PrintColour("Closest conversion: " + userDoubleInput.ToString(), true); //Show the conversion
                string bitconv = Convert.ToString(BitConverter.DoubleToInt64Bits(userDoubleInput), 2);
                lastInput = Convert.ToUInt64(bitconv, 2); //Convert the double into a ulong to change last input
                return;
            }
            if (userINPUT.Length >= 5 && userINPUT.Substring(0,5) == "float") //User wants to show value as float
            {
                PrintFloat(FloatToBin(float.Parse(userINPUT.Substring(5)))); //Print the new float value
                float userFloatInput = float.Parse(userINPUT.Substring(5));
                PrintColour("Closest conversion: " + userFloatInput.ToString(), true); //Show the conversion
                string bitconv = Convert.ToString(BitConverter.SingleToInt32Bits(userFloatInput), 2);
                lastInput = Convert.ToUInt64(bitconv, 2); //Convert the double into a ulong to change last input
                return;
            }

            //Show previous bitset as decimal
            if (userINPUT == "adv") //Show previous bitset as a double
            {
                PrintDouble(DoubleToBin(BitConverter.Int64BitsToDouble((long)lastInput)));
                PrintColour("Double is: " + BitConverter.Int64BitsToDouble((long)lastInput), false);
                return;
            }
            if (userINPUT == "afv") //Show previous bitset as a float value
            {
                int lastinput__int = int.Parse(lastInput.ToString());
                float int32bits = BitConverter.ToSingle(BitConverter.GetBytes(lastinput__int));
                PrintFloat(FloatToBin(int32bits));
                PrintColour("Float is: " + BitConverter.Int32BitsToSingle(int.Parse(lastInput.ToString())), false);
                return;
            }
            #endregion


            if (userINPUT == "dt") //User wants to see the current date/time
            {
                PrintColour(DateTime.Now.ToString(), false); //Print the date/time
                return;
            }
            if (ModifyVariables(userINPUT)) //Is the user modifying variables that already exist
                //This function automatically deals with it, so we just need to finish
            {
                return;
            }
            userINPUT = ReplaceTempVariables(userINPUT); //Remove temporarily defined variables
            string replaced = ReplaceTempVariables(userINPUT, 'v', lastInput.ToString()); //Define a new variable 'v' as the last result
            if(replaced != userINPUT) //Is the new value different to the old value. Used to stop infinite recursive loop
            {
                PrintColour(userINPUT + "-->" + replaced, true); //Show the user the change
                userINPUT = replaced; //Modify the user input to be the old input
            }
            if (userINPUT.Length >= 3 && userINPUT.Substring(0,3) == "var")
            {
                DefineTempVariable(userINPUT.Substring(3)); //Define a new temporary variable with the users input
                return;
            }
            bool noprint = false;
            if (userINPUT.Length >= 2 && userINPUT.Substring(0,2) == "np") //Does the user not want to print the binary value of the final result?
            {
                noprint = true; //Tell the binary printer NOT to print
                userINPUT = userINPUT.Substring(2); //Remove the string "np" from the userINPUT
            }
            if (userINPUT.Length >= 4 && userINPUT.Substring(0,4) == "hrgb") //Does the user want to convert a hex value into rgb
                //Returns rgb(255,255,255) for #ffffff
            {
                userINPUT = userINPUT.Substring(4); //Remove the "hrgb" from the calculation
                HEX_to_RGB(userINPUT); //Convert the hex value into rgb and print the result
                return;
            }
            string toreplace = RemoveBooleanStatements(userINPUT); //Remove the boolean statements from the users input
            //For example 2+2==4?x:y
            if (toreplace != userINPUT) //Have boolean statements been implemented in the users script?
            {
                return; //Result has already been dealt with. Return
            }
            if (userINPUT.Length >= 5 && userINPUT.Substring(0,5) == "asci(") //Does the user want to draw ascii art
            {
                WriteAscii(userINPUT.Substring(5, userINPUT.Length-6)); //Remove the final bracket from the asci statement
                return;
            }
            if (userINPUT.Length >= 6 && userINPUT.Substring(0,6) == "asib(") //Does the user want to draw snazzy binary ascii art
            {
                BinaryNumASCI.PrintConverted(userINPUT.Substring(6, userINPUT.Length - 7)); //Remove the final bracket from the asci statement
                return;
            }
            userINPUT = RemoveHex(userINPUT); //Look for hex the user has typed and replace it with its int value
            if (userINPUT.ToLower() == "help") //Pretty damn well self explanatory
            {
                PrintHelp();
                return;
            }
            if (userINPUT.ToLower() == "pnw") //Change the default value for printing workings or not
            {
                printWorkings = !printWorkings;
                return;
            }
            if (userINPUT.ToLower() == "fnpw") //Change the value for printing workings of not.. Write it to a file
            {
                printWorkings = !printWorkings;
                File.WriteAllText(WorkingsFilePath, printWorkings.ToString());
                return;
            }
            if (userINPUT.ToLower() == "cv") //Delete all variables
            {
                File.WriteAllText(DataFilePath, "");
                return;
            }

            userINPUT = RemoveBinary(userINPUT); //User can define binary with b_. Replace this with its integer value
            if (userINPUT.Length >= 4 && userINPUT.Substring(0,3) == "avg") //User wants to get average number of a set
            {
                PrintColour("Average is: " + Average(userINPUT));
                return;
            }
            bool flipped = defaultFlipVal; //Flip binary output variable
            //Shows whether binary will be *left to right* or *right to left*

            if (userINPUT == "rf") //User wants to change the default flip value
            {
                defaultFlipVal = !defaultFlipVal;
                return;
            }
            bool is32bit = false;
            bool is16bit = false;
            bool is8bit = false;
            if (userINPUT.Length >= 3 && userINPUT.Substring(0,3) == "ati") //Weird math thingy. Description in the PrintAti() function
            {
                PrintAti(userINPUT);
                return;
            }
            if (userINPUT.Length >= 1 && userINPUT[0] == 'f') //Flipping the binary result?
            {
                flipped = true; //Change the flipped value to true so that when we print binary later, we know what to do
                userINPUT = userINPUT.Substring(1); //Remove the 'f' from the string
                PrintColour("Printing flipped...", true); //Inform the user that the binary outcome is being flipped
            }

            userINPUT = RemoveTrig(userINPUT); //Remove the trig functions from the users input

            if (userINPUT.Length >= 1 && userINPUT[0] == 'i') //User wants to show binary value as 32i (32 bit uint)
            {
                is32bit = true; //Tell the binary printer to print only 32 bits
                userINPUT = userINPUT.Substring(1); //Remove the i from the thing being printed
            }
            else if (userINPUT.Length >= 1 && userINPUT[0] == 's') //User wants to show binary value as 16s (16 bit ushort)
            {
                is16bit = true; //Tell the binary printer to only print 16 bits
                userINPUT = userINPUT.Substring(1); //Remove the s from the thing being printed
            }
            else if (userINPUT.Length >= 1 && userINPUT[0] == 'b') //User wants to show binary value as 8b (8 bit byte)
            {
                is8bit = true; //Tell the binary printer to only print 8 bits
                userINPUT = userINPUT.Substring(1); //Remove the b from the thing being printed
            }

            else if (userINPUT.Length >= 2 && userINPUT[0] == 'h') //User wants to show as hexadecimal?
            {
                userINPUT = userINPUT.Substring(1); //Remove the h from the start
                PrintHex(ulong.Parse(userINPUT).ToString("X2").ToLower()); //Print the hex value of the users input
                return;
            }
            if (userINPUT.Length >= 2 && userINPUT.Substring(0,2) == "#_") //Converting hex value into ulong?
            {
                userINPUT = userINPUT.Substring(2);
                PrintColour(ulong.Parse(userINPUT, System.Globalization.NumberStyles.HexNumber).ToString(), false); 
                //Convert from hex to ulong, and print the result
                return;
            }
            var doumMathResult = RemoveDoubleMath(userINPUT); //Remove all 'doum' stuff
            if (userINPUT != doumMathResult) //Did we do doum math?
            {
                //Do the doub print thing

                userINPUT = doumMathResult;
                PrintDouble(DoubleToBin(double.Parse(userINPUT)));
                PrintColour("Closest conversion: " + double.Parse(userINPUT).ToString(), true);
                double d = double.Parse(userINPUT);
                string bitconv = Convert.ToString(BitConverter.DoubleToInt64Bits(d), 2);
                lastInput = Convert.ToUInt64(bitconv, 2);
                return;
            }
            char chosenType = 'u';
            if (is32bit)
            {
                chosenType = 'i';
            }
            else if (is16bit)
            {
                chosenType = 's';
            }
            else if (is8bit)
            {
                chosenType = 'b';
            } //Change the chosen output type depending on the specified bit length
            //Default is 64 bit ulong

            userINPUT = RemoveBrackets(userINPUT, chosenType);
            string booleans = CheckForBooleans(userINPUT, chosenType); //Remove boolean conditions (==,!=,<,>)
            if (booleans == "true" || booleans == "false")
            {
                PrintColour(booleans, false); //Only do bool math, don't process following characters
                return;
            }
            userINPUT = BitCalculate(userINPUT, chosenType);
            ulong.TryParse(userINPUT, out ulong input);
            if (!noprint) //Are we printing the binary values?
            {
                if (is32bit) //Print as 32 bit
                {
                    PrintColour(UlongToBin(input, flipped).Substring(66), false, true);
                }
                else if (is16bit) //Print as 16 bit
                {
                    PrintColour(UlongToBin(input, flipped).Substring(101), false, true);
                }
                else if (is8bit) //Print as 8 bit
                {
                    PrintColour(UlongToBin(input, flipped).Substring(118), false, true);
                }
                else //Print as ulong
                {
                    if (userINPUT == "") //Process blank
                    {
                        PrintColour("No value entered", false);
                        return;
                    }
                    PrintColour(UlongToBin(input, flipped), false, true);
                }
            }
            lastInput = input; //Assign lastinput
            if (resetworkings) //Are we resetting the modified printworkings value
            {
                printWorkings = !printWorkings; //Reset static variable
            }
        }
        /// <summary>
        /// Weird math thingy
        /// </summary>
        /// <param name="userINPUT"></param>
        /// Gets all values for the different letters of the alphabet
        /// A=1
        /// B=2
        /// C=3
        /// .....
        /// Then multiplies them together
        /// AB = 2
        /// ABC = 6
        /// ABCD = 24
        /// CD = 12
        /// DB = 8
        /// etc....
        /// I have no idea why I implemented this.....
        private static void PrintAti(string userINPUT)
        {
            userINPUT = userINPUT.Substring(3);
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            ulong total = 1;
            foreach (var c in userINPUT.ToUpper())
            {
                if (alpha.ToList().IndexOf(c) == -1)
                {
                    continue;
                }
                total = total * (ulong)(alpha.ToList().IndexOf(c) + 1);
            }
            Colorful.Console.WriteAscii(string.Format("{0:n0}", total));
        }

        private static string RemoveComments(string sINPUT)
        {
            if (sINPUT.Contains("//") && sINPUT.Contains(@"\\") && !sINPUT.Contains("#defunc"))
            {
                string comment = sINPUT.Substring(sINPUT.IndexOf("//")+2, sINPUT.IndexOf(@"\\")-sINPUT.IndexOf("//")-2);
                Colorful.Console.WriteLine(comment, Color.Beige);
                return RemoveComments(sINPUT.Substring(0,sINPUT.IndexOf("//"))+sINPUT.Substring(sINPUT.IndexOf(@"\\")+2));
            }
            else
            {
                return sINPUT;
            }
        }
        private static string ShowDescription(string sINPUT)
        {
            if (sINPUT.Contains("///"))
            {
                string comment = sINPUT.Substring(sINPUT.IndexOf("///") + 3);
                List<string> toprint = new List<string>();
                string buffer = "";
                foreach (var c in comment)
                {
                    if (c == '\\')
                    {
                        toprint.Add(buffer);
                        buffer = "";
                    }
                    else
                    {
                        if (!(buffer == "" && c == 'n' && toprint.Count() != 0))
                        {
                            buffer += c;
                        }
                    }
                }
                toprint.Add(buffer);
                foreach (var s in toprint) 
                {
                    Colorful.Console.WriteLine(s, Color.Beige);
                }
            }
            return sINPUT;
        }

        private static string RemoveDoubleMath(string sINPUT)
        {
            if (sINPUT.Length >= 4 && sINPUT[0] == 'd' && sINPUT[1] == 'o' && sINPUT[2] == 'u' && sINPUT[3] == 'm')
            {
                sINPUT = sINPUT.Substring(4);
                return DoubleCalculate(DoubleRemoveBrackets(sINPUT));
            }
            return sINPUT;
        }

        private static string RemoveRandom(string sINPUT)
        {
            if (sINPUT.Contains("ran("))
            {
                string buffer = "";
                for (int i = 0; i < sINPUT.Length; i++)
                {
                    char c = sINPUT[i];
                    buffer += c;
                    if (buffer.Contains("ran("))
                    {
                        int nextBracket = ClosingBracket(sINPUT, i+1);
                        string constraints = sINPUT.Substring(i+1, nextBracket-i-1);
                        Random random = new Random();
                        int nextRan = random.Next(int.Parse(constraints.Split(',')[0]), 1+int.Parse(constraints.Split(',')[1]));
                        PrintColour("Random number is: " + nextRan.ToString(), true);
                        string before = sINPUT.Substring(0, i-3);
                        string replace = nextRan.ToString();
                        string after = sINPUT.Substring(nextBracket+1);
                        return RemoveRandom(before+replace+after);
                    }
                }
            }
            return sINPUT;
        }

        private static void PrintDouble(string input)
        {
            //Colorful.Console.BackgroundColor = Color.LightSalmon;
            //Colorful.Console.WriteLine(input, Color.Red);
            //Colorful.Console.BackgroundColor = Color.FromArgb(0, 16, 29);
            var res = Regex.Replace(RemoveSpaces(input), "\n", "");
            for (int i = 0; i < 64; ++i)
            {
                if (i == 0)
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(255,255,255));
                }
                else if (i <= 11)
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(0,255,10));
                }
                else
                {
                    Colorful.Console.Write(res[i], Color.FromArgb(255,100,100));
                }
                if ((i+1)%8 == 0 && i !=0)
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
            Colorful.Console.WriteLine("This colour is the sign"    , Color.FromArgb(255, 255, 255));
        }
        private static void PrintFloat(string input)
        {
            //Colorful.Console.BackgroundColor = Color.LightSalmon;
            //Colorful.Console.WriteLine(input, Color.Red);
            //Colorful.Console.BackgroundColor = Color.FromArgb(0, 16, 29);
            var res = Regex.Replace(RemoveSpaces(input), "\n", "");
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

        private static string RemoveBooleanStatements(string sINPUT)
        {
            if (sINPUT.Contains('?') && !sINPUT.Contains(':')) //Only one condition and no else
            {
                for (int i = 0; i < sINPUT.Length; ++i)
                {
                    char c = sINPUT[i];
                    if (c == '?')
                    {
                        int lastOperatorIDX = LastOperatorIDX(sINPUT, i-1);
                        string condition = RemoveHex(RemoveBrackets(BitCalculate(CheckForBooleans(sINPUT.Substring(lastOperatorIDX, i-lastOperatorIDX), 'u'),'u'), 'u'));
                        if (condition == "true")
                        {
                            string toRun = sINPUT.Substring(i, sINPUT.Length-i).Substring(1);
                            DoMainMethod(toRun);
                        }
                        return "";
                    }
                }
            }
            if (sINPUT.Contains('?') && sINPUT.Contains(':'))
            {
                for (int i = 0; i < sINPUT.Length; ++i)
                {
                    char c = sINPUT[i];
                    if (c == '?')
                    {
                        int lastOperatorIDX = LastOperatorIDX(sINPUT, i - 1);
                        int nextColonIDX = NextColonIDX(sINPUT, i + 1);
                        string condition = RemoveHex(RemoveBrackets(BitCalculate(CheckForBooleans(sINPUT.Substring(lastOperatorIDX, i - lastOperatorIDX), 'u'), 'u'), 'u'));
                        if (condition == "true")
                        {
                            string toRun = sINPUT.Substring(i, nextColonIDX - i).Substring(1);
                            DoMainMethod(toRun);
                        }
                        else
                        {
                            string toRun = sINPUT.Substring(nextColonIDX, sINPUT.Length-nextColonIDX).Substring(1);
                            DoMainMethod(toRun);
                        }
                        return "";
                    }
                }
            }
            return sINPUT;
        }

        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        public static double RadianToDegree(double angle)
        {
            return angle * 180.0 / Math.PI;
        }
        private static string RemoveTrig(string sINPUT)
        {
            if (sINPUT.Contains("sin(") || 
                sINPUT.Contains("arcsin(")||
                sINPUT.Contains("tan(") ||
                sINPUT.Contains("arctan(") ||
                sINPUT.Contains("cos(") ||
                sINPUT.Contains("arccos("))
            {
                string buffer = "";
                for (int i = 0; i < sINPUT.Length; i++)
                {
                    char c = sINPUT[i];
                    buffer += c;
                    if (buffer.Contains("arcsin("))
                    {
                        string fixedval = sINPUT.Substring(0, i - 6);
                        int nextOperaror = ClosingBracket(sINPUT, i + 1);
                        string s = sINPUT.Substring(i + 1, nextOperaror - i - 1);
                        string calcNum = RadianToDegree(Math.Asin(double.Parse(s))).ToString();
                        string afterThat = sINPUT.Substring(nextOperaror + 1, sINPUT.Length - nextOperaror - 1);
                        //PrintColour(string.Format("{0} --> {1}", sINPUT, fixedval + binNum + afterThat));
                        PrintColour(string.Format("arcsin({0}) = {1}", s, calcNum), false);
                        return RemoveTrig(fixedval + calcNum + afterThat);
                    }
                    if (buffer.Contains("arccos("))
                    {
                        string fixedval = sINPUT.Substring(0, i - 6);
                        int nextOperaror = ClosingBracket(sINPUT, i + 1);
                        string s = sINPUT.Substring(i + 1, nextOperaror - i - 1);
                        string calcNum = Math.Round(RadianToDegree(Math.Acos(double.Parse(s)))).ToString();
                        string afterThat = sINPUT.Substring(nextOperaror + 1, sINPUT.Length - nextOperaror - 1);
                        //PrintColour(string.Format("{0} --> {1}", sINPUT, fixedval + binNum + afterThat));
                        PrintColour(string.Format("arccos({0}) = {1}", s, calcNum), false);
                        return RemoveTrig(fixedval + calcNum + afterThat);
                    }
                    if (buffer.Contains("arctan("))
                    {
                        string fixedval = sINPUT.Substring(0, i - 6);
                        int nextOperaror = ClosingBracket(sINPUT, i + 1);
                        string s = sINPUT.Substring(i + 1, nextOperaror - i - 1);
                        string calcNum = (RadianToDegree(Math.Atan(double.Parse(s)))).ToString();
                        string afterThat = sINPUT.Substring(nextOperaror + 1, sINPUT.Length - nextOperaror - 1);
                        //PrintColour(string.Format("{0} --> {1}", sINPUT, fixedval + binNum + afterThat));
                        PrintColour(string.Format("arctan({0}) = {1}", s, calcNum), false);
                        return RemoveTrig(fixedval + calcNum + afterThat);
                    }



                    if (buffer.Contains("sin("))
                    {
                        string fixedval = sINPUT.Substring(0, i - 3);
                        int nextOperaror = ClosingBracket(sINPUT, i + 1);
                        string s = sINPUT.Substring(i + 1, nextOperaror - i - 1);
                        string calcNum = (Math.Sin(DegreeToRadian(double.Parse(s)))).ToString();
                        string afterThat = sINPUT.Substring(nextOperaror+1, sINPUT.Length - nextOperaror-1);
                        //PrintColour(string.Format("{0} --> {1}", sINPUT, fixedval + binNum + afterThat));
                        PrintColour(string.Format("sin({0}) = {1}", s, calcNum), false);
                        return RemoveTrig(fixedval + calcNum + afterThat);
                    }
                    if (buffer.Contains("cos("))
                    {
                        string fixedval = sINPUT.Substring(0, i - 3);
                        int nextOperaror = ClosingBracket(sINPUT, i + 1);
                        string s = sINPUT.Substring(i + 1, nextOperaror - i - 1);
                        string calcNum = (Math.Cos(DegreeToRadian(double.Parse(s)))).ToString();
                        string afterThat = sINPUT.Substring(nextOperaror + 1, sINPUT.Length - nextOperaror - 1);
                        //PrintColour(string.Format("{0} --> {1}", sINPUT, fixedval + binNum + afterThat));
                        PrintColour(string.Format("cos({0}) = {1}", s, calcNum), false);
                        return RemoveTrig(fixedval + calcNum + afterThat);
                    }
                    if (buffer.Contains("tan("))
                    {
                        string fixedval = sINPUT.Substring(0, i - 3);
                        int nextOperaror = ClosingBracket(sINPUT, i + 1);
                        string s = sINPUT.Substring(i + 1, nextOperaror - i - 1);
                        string calcNum = (Math.Tan(DegreeToRadian(double.Parse(s)))).ToString();
                        string afterThat = sINPUT.Substring(nextOperaror + 1, sINPUT.Length - nextOperaror - 1);
                        //PrintColour(string.Format("{0} --> {1}", sINPUT, fixedval + binNum + afterThat));
                        PrintColour(string.Format("tan({0}) = {1}", s, calcNum), false);
                        return RemoveTrig(fixedval + calcNum + afterThat);
                    }
                }
            }
            return sINPUT;
        }

        private static int ClosingBracket(string sINPUT, int v)
        {
            int amountOfOpenBrackets = 1;
            int amountOfClosingBrackets = 0;
            for (int i = v; i < sINPUT.Length; ++i)
            {
                if (sINPUT[i] == '(')
                {
                    amountOfOpenBrackets++;
                }
                if (sINPUT[i] == ')')
                {
                    amountOfClosingBrackets++;
                }
                if (amountOfClosingBrackets == amountOfOpenBrackets)
                {
                    return i;
                }
            }
            return sINPUT.Length - 1;
        }

        private static void DeleteVariable(string input)
        {
            List<string> variables = File.ReadAllLines(DataFilePath).ToList();
            var copy = new List<string>();
            foreach (var v in variables)
            {
                copy.Add(v);
            }
            foreach (var s in variables)
            {
                var v = s.Split(',')[0];
                if (v == input)
                {
                    copy.Remove(s);
                }
            }
            foreach (var v in copy)
            {
                PrintColour(v, true);
            }
            File.WriteAllLines(DataFilePath, copy);
        }

        private static string RemoveBinary(string input)
        {
            char prev = ' ';
            if (input.Contains("b_"))
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = (char)input[i];
                    if (c == '_' && prev == 'b')
                    {
                        string fixedval = input.Substring(0, i-1);
                        int nextOperaror = NextOperatorIDX_NoLetter(input, i + 1);
                        string binNum = Convert.ToUInt64(input.Substring(i + 1, nextOperaror - i - 1), 2).ToString();
                        string afterThat = input.Substring(nextOperaror, input.Length - nextOperaror);
                        PrintColour(string.Format("{0} --> {1}", input, fixedval + binNum + afterThat), true);
                        return RemoveHex(fixedval + binNum + afterThat);
                    }
                    prev = c;
                }
            }
            return input;
        }

        private static string RemoveHex(string input)
        {
            if (input.Length >= 2 && input[1] == '_')
            {
                return input;
            }
            if (input.Contains('#'))
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = (char)input[i];
                    if (c == '#')
                    {
                        string fixedval = input.Substring(0,i);
                        int nextOperaror = NextOperatorIDX_NoLetter(input, i+1);
                        string hexNum = ulong.Parse(input.Substring(i+1, nextOperaror - i-1), System.Globalization.NumberStyles.HexNumber).ToString();
                        string afterThat = input.Substring(nextOperaror, input.Length - nextOperaror);
                        if (printWorkings)
                        {
                            PrintColour(input.Substring(i + 1, nextOperaror - i - 1) + " --> " + hexNum);
                        }
                        return RemoveHex(fixedval+hexNum+afterThat);
                    }
                }
            }
            return input;
        }
        /*static List<string> elements = new List<string>()
        {
            "H",
            "He",
            "Li",
            "Be",
            "B",
            "C",
            "N",
            "O",
            "F",
            "Ne",
            "Na",
            "Mg",
            "Al",
            "Si",
            "P",
            "S",
            "Cl",
            "Ar",
            "K",
            "Ca",
            "Sc",
            "Ti",
            "V",
            "Cr",
            "Mn",
            "Fe",
            "Co",
            "Ni",
            "Cu",
            "Zn",
            "Ga",
            "Ge",
            "As",
            "se",
            "Br",
            "Kr",
            "Rb",
            "Sr",
            "Y",
            "Zr",
            "Nb",
            "Mo",
            "Tc",
            "Ru",
            "Rh",
            "Pd",
            "Ag",
            "Cd",
            "In",
            "Sn",
            "Sb",
            "Te",
            "I",
            "Xe",
            "Cs",
            "Ba",
            "La",
            "Ce",
            "Pr",
            "Nd",
            "Pm",
            "Sm",
            "Eu",
            "Gd",
            "Tb",
            "Dy",
            "Ho",
            "Er",
            "Tm",
            "Yb",
            "Lu",
            "Hf",
            "Ta",
            "W",
            "Re",
            "Os",
            "Ir",
            "Pt",
            "Au",
            "Hg",
            "Tl",
            "Pb",
            "Bi",
            "Po",
            "At",
            "Rn",
            "Fr",
            "Ra",
            "Ac",
            "Th",
            "Pa",
            "U",
            "Np",
            "Pu",
            "Am",
            "Cm",
            "Bk",
            "Cf",
            "Es",
            "Fm",
            "Md",
            "No",
            "Lr",
            "Rf",
            "Db",
            "Sg",
            "Bh",
            "Hs",
            "Mt",
            "Ds",
            "Rg",
            "Cn",
            "Nh",
            "Fl",
            "Mc",
            "Lv",
            "Ts",
            "Og"
        };
        class Compound
        {
            public List<Element> elements;
            public int coEfficient;
            public Compound(int coEfficient)
            {
                this.coEfficient = coEfficient;
                elements = new List<Element>();
            }
            //public static bool operator ==(Compound a, Compound b)
            //{
            //    return a.elements == b.elements && a.coEfficient == b.coEfficient;
            //}
            //public static bool operator !=(Compound a, Compound b)
            //{
            //    return a.elements != b.elements || a.coEfficient != b.coEfficient;
            //}
        }
        class Element
        {
            public string element;
            public Compound compound;
            public int coEfficient
            {
                get
                {
                    return compound.coEfficient;
                }
            }
            public readonly int subscript;
            public int Total
            {
                get
                {
                    if (coEfficient == 0 && subscript == 0)
                    {
                        return 1;
                    }
                    if (coEfficient == 0)
                    {
                        return subscript;
                    }
                    if (subscript == 0)
                    {
                        return coEfficient;
                    }
                    return coEfficient * subscript;
                }
            }
            public Element(string element, Compound compound, int subscript)
            {
                this.element = element;
                this.compound = compound;
                this.compound.elements.Add(this);
                this.subscript = subscript == 0 ? 1 : subscript;
            }
        }
        static string BalanceFormula(string input)
        {
            string[] sides = input.Split("->");
            string[] reactants = sides[0].Split('+');
            string[] products = sides[1].Split('+');

            List<Element> Reactants = new List<Element>();
            List<Element> Products = new List<Element>();

            for (int si = 0; si < products.Length; ++si)
            {
                char last = ' ';
                string curr = products[si];
                string totalNums = "";
                foreach (var c in curr)
                {
                    if (char.IsNumber(c))
                    {
                        totalNums += c;
                    }
                    else
                    {
                        break;
                    }
                }
                int amount = 1;
                int.TryParse(totalNums, out amount);
                if (amount == 0)
                {
                    amount = 1;
                }
                Compound currentCompound = new Compound(amount);
                for (int i = 0; i < products[si].Length; ++i) 
                {
                    if (char.IsUpper(curr[i]) && !char.IsNumber(last))
                    {
                        int nextIDX = NextUpperCase_Number_IDX(i + 1, curr);
                        string letters = curr.Substring(i, nextIDX - i);
                        int nextNum = NextNum(curr, i);
                        Products.Add(new Element(letters, currentCompound, nextNum));
                    }
                    if (char.IsNumber(curr[i]))
                    {
                        int nextIDX = NextUpperCase_Number_IDX(i + 2, curr);
                        string letters = curr.Substring(i+1, nextIDX - i-1);
                        int nextNum = NextNum(curr, i+1);
                        if (letters != "")
                        {
                            Products.Add(new Element(letters, currentCompound, nextNum));
                        }
                    }
                    last = curr[i];
                }
            }
            for (int si = 0; si < reactants.Length; ++si)
            {
                char last = ' ';
                string curr = reactants[si];
                string totalNums = "";
                foreach (var c in curr)
                {
                    if (char.IsNumber(c))
                    {
                        totalNums += c;
                    }
                    else
                    {
                        break;
                    }
                }
                int amount = 1;
                int.TryParse(totalNums, out amount);
                if (amount == 0)
                {
                    amount = 1;
                }
                Compound currentCompound = new Compound(amount);

                for (int i = 0; i < reactants[si].Length; ++i)
                {
                    if (char.IsUpper(curr[i]) && !char.IsNumber(last))
                    {
                        int nextIDX = NextUpperCase_Number_IDX(i + 1, curr);
                        string letters = curr.Substring(i, nextIDX - i);
                        int nextNum = NextNum(curr, i);
                        Reactants.Add(new Element(letters, currentCompound, nextNum));
                    }
                    if (char.IsNumber(curr[i]))
                    {
                        int nextIDX = NextUpperCase_Number_IDX(i + 2, curr);
                        string letters = curr.Substring(i+1, nextIDX - i-1);
                        int nextNum = NextNum(curr, i+1);
                        if (letters != "") 
                        {
                            Reactants.Add(new Element(letters, currentCompound, nextNum));
                        }
                    }
                    last = curr[i];
                }
            }
            if (Reactants.Count() != Products.Count())
            {
                Reactants = RemoveDuplicates(Reactants);
                throw new Exception();
            }
            Reactants = Reactants.OrderBy(e=>e.element).ToList();
            Products = Products.OrderBy(e=>e.element).ToList();
            return BalanceFormula(Reactants, Products);
        }

        private static List<Element> RemoveDuplicates(List<Element> Reactants)
        {
            List<Element> result = new List<Element>();
            foreach (var r in Reactants)
            {
                if (Reactants.Where(re => re.element == r.element).Count() >= 2)
                {
                    result.Add(new Element(r.element, r.compound, r.subscript+1));
                }
            }
            return null;
        }

        private static string BalanceFormula(List<Element> reactants, List<Element> products)
        {
            List<Compound> reactantCompounds = reactants.Select(p => p.compound).Distinct().ToList();
            List<Compound> productCompounds = products.Select(p => p.compound).Distinct().ToList();
            if (IsBalanced(reactants, products))
            {
                return ChemicalEquation(reactants, products, reactantCompounds, productCompounds);
            }
            return "";
        }

        private static string ChemicalEquation(List<Element> reactants, List<Element> products, List<Compound> reactantCompounds, List<Compound> productCompounds)
        {
            string result = "";
            result += reactantCompounds[0].coEfficient == 0 ? 1 : reactantCompounds[0].coEfficient;
            foreach (var reactant in reactantCompounds)
            {
                if (reactant != reactantCompounds[0])
                {
                    result += "+";
                    if (reactant.coEfficient != 1)
                    {
                        result += reactant.coEfficient;
                    }
                }
                foreach (var e in reactant.elements)
                {
                    result += e.element;
                    if (e.subscript != 1)
                    {
                        result += Subscript(e.subscript);
                    }
                }
            }
            result += "->";
            result += productCompounds[0].coEfficient == 0 ? 1 : productCompounds[0].coEfficient;
            foreach (var product in productCompounds)
            {
                if (product != productCompounds[0])
                {
                    result += "+";
                    if (product.coEfficient != 1)
                    {
                        result+=product.coEfficient;
                    }
                }
                foreach (var e in product.elements)
                {
                    result += e.element;
                    if (e.subscript != 1)
                    {
                        result += Subscript(e.subscript);
                    }
                }
            }
            return result;
        }

        private static string Subscript(int subscript)
        {
            string s = subscript.ToString();
            string result = "";
            foreach (var c in s)
            {
                switch (c)
                {
                    case '0':
                        result += '\u2080';
                        break;
                    case '1':
                        result += '\u2081';
                        break;
                    case '2':
                        result += '\u2082';
                        break;
                    case '3':
                        result += '\u2083';
                        break;
                    case '4':
                        result += '\u2084';
                        break;
                    case '5':
                        result += '\u2085';
                        break;
                    case '6':
                        result += '\u2086';
                        break;
                    case '7':
                        result += '\u2087';
                        break;
                    case '8':
                        result += '\u2088';
                        break;
                    case '9':
                        result += '\u2089';
                        break;
                }
            }
            return result;
        }

        private static bool IsBalanced(List<Element> reactants, List<Element> products)
        {
            for (int i = 0; i < reactants.Count; ++i)
            {
                if (reactants[i].Total != products[i].Total)
                {
                    return false;
                }
            }
            return true;
        }
        */
        private static int NextNum(string curr, int startIDX)
        {
            string nums = "";
            bool nextMustBeNum = false;
            for (int i = startIDX; i < curr.Length; ++i)
            {
                if (char.IsNumber(curr[i]))
                {
                    nums += curr[i];
                    nextMustBeNum = true;
                }
                else if (nextMustBeNum)
                {
                    break;
                }
            }
            int result = 0;
            int.TryParse(nums, out result);
            return result;
        }

        public static int NextUpperCase_Number_IDX(int currIDX, string input)
        {
            for (int i = currIDX; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) || char.IsNumber(input[i]))
                {
                    return i;
                }
            }
            return input.Length;
        }
        private static void PrintHex(string v)
        {
            Colorful.Console.WriteLine(v.Insert(0,"#"), Color.FromArgb(234,255,0));
        }
        private static void ConvertHEX(string v)
        {
            if (v[0] == '#')
            {
                v = v.Substring(1);
            }
            PrintColour(ulong.Parse(v, System.Globalization.NumberStyles.HexNumber));
        }
        public static void HEX_to_RGB(string hexVal)
        {
            if (hexVal[0] == '#')
            {
                hexVal = hexVal.Substring(1);
            }
            string result = "rgb(";
            string buffer = "";
            for (int i = 0; i < hexVal.Length; ++i)
            {
                buffer += hexVal[i];
                if (buffer.Length == 2)
                {
                    result += ulong.Parse(buffer, System.Globalization.NumberStyles.HexNumber).ToString();
                    result += ',';
                    buffer = "";
                }
            }
            result = result.Substring(0,result.Length-1);
            result += ");";
            PrintColour(result, false);
        }
        public static string ConvertHEX(ulong input)
        {
            return input.ToString("X2");
        }
        private static void PrintColour(ulong v)
        {
            foreach (var c in v.ToString())
            {
                if (c == '0')
                {
                    Colorful.Console.Write(c, Color.FromArgb(90, 255, 183));
                }
                else
                {
                    Colorful.Console.Write(c, Color.FromArgb(10, 181, 158));
                }
            }
            Colorful.Console.WriteLine();
        }
        public static void PrintColour(string v, bool workings = false, bool isBin = false)
        {
            if (workings && printWorkings == false)
            {
                return;
            }
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
                        //Colorful.Console.WriteLine();
                    }
                }
            }
            else
            {
                bool isyellow = false;
                for (int i = 0; i < v.Length; i++)
                {
                    char c = v[i];
                    if (isyellow && !IsOperator(c) && c != ' ')
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
                    else if (IsOperator(c) || c == ' ')
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
        public static bool printWorkings = true;
        public static void PrintBlindColour(string v, bool isBin = false)
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
                        //Colorful.Console.WriteLine();
                    }
                }
            }
            else
            {
                bool isyellow = false;
                for (int i = 0; i < v.Length; i++)
                {
                    char c = v[i];
                    if (isyellow && !IsOperator(c) && c != ' ')
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
                    else if (IsOperator(c) || c == ' ')
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
        private static bool IsOperator(char c)
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
                _ => false,
            };
        }

        private static string CalculateBitShift(string sINPUT, char chosenType)
        {
            if (sINPUT.Contains("<<"))
            {
                ulong input;
                var strings = sINPUT.Split(new string[] { "<<" }, StringSplitOptions.None);
                if (strings[0] == "")
                {
                    strings[0] = "1";
                }
                var first = 0;
                var second = 0ul;
                int.TryParse(strings[1], out first);
                ulong.TryParse(strings[0], out second);
                input = (second << first);
                PrintColour(string.Format("{0} << {1} = {2}", second, first, input), true);
                return input.ToString();
            }
            else if (sINPUT.Contains(">>"))
            {
                ulong input;
                var strings = sINPUT.Split(new string[] { ">>" }, StringSplitOptions.None);
                if (strings[0] == "")
                {
                    switch (chosenType)
                    {
                        case 'b':
                            strings[0] = (byte.MaxValue/2+1).ToString();
                            break;
                        case 's':
                            strings[0] = (ushort.MaxValue / 2 + 1).ToString();
                            break;
                        case 'i':
                            strings[0] = (uint.MaxValue / 2 + 1).ToString();
                            break;
                        default:
                            strings[0] = (ulong.MaxValue / 2 + 1).ToString();
                            break;
                    }
                }
                var first = 0;
                var second = 0ul;
                int.TryParse(strings[1], out first);
                ulong.TryParse(strings[0], out second);
                input = (second >> first);
                PrintColour(string.Format("{0} >> {1} = {2}", second, first, input), true);
                return input.ToString();
            }
            return "0";
        }
        public static string RemoveBitShift(string input, char chosenType)
        {
            string buffer = "";
            if (input.Contains("<<") || input.Contains(">>"))
            {
                for (int i = 0; i < input.Length; i++)
                {
                    buffer += input[i];
                    if (buffer.Contains("<<") || buffer.Contains(">>"))
                    {
                        buffer = "";
                        int lastOperatorIDX = LastOperatorIDX(input, i - 2);
                        if ((lastOperatorIDX != 0 || IsOperator(input[0])) && i >=2)
                        {
                            ++lastOperatorIDX;
                        } 
                        int nextOperatorIDX = NextOperatorIDX(input, i + 1);
                        string sub = input.Substring(lastOperatorIDX, (nextOperatorIDX - lastOperatorIDX));
                        string result = CalculateBitShift(sub, chosenType);
                        string toReturn = string.Format("{0}{1}{2}", input.Substring(0, lastOperatorIDX), result, input.Substring(nextOperatorIDX, input.Length - nextOperatorIDX));
                        return RemoveBitShift(toReturn, chosenType);
                    }
                }
            }
            return input;
        }
        public static void DoLoopFunc(string loop)
        {
            loop = loop.Substring(4);
            string tocalc = loop.Split(':')[0];
            string s = BitCalculate(RemoveBrackets(tocalc, 'u'), 'u');
            int timesAround = int.Parse(s);
            loop = loop.Substring(tocalc.Length+1);
            for (int i = 0; i < timesAround; ++i)
            {
                string currentLoop = ReplaceTempVariables(loop, 'i', i.ToString());
                DoMainMethod(currentLoop);
            }
        }

        private static string ReplaceTempVariables(string input, char variableName, string variableValue)
        {
            string result = "";
            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];
                if (c == variableName && (input.Length-1 == i || IsOperator(input[i+1])) && (i == 0 || IsOperator(input[i-1])))
                {
                    result += variableValue;
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }
        private static string ReplaceTempVariables(string input, string variableName, string variableValue)
        {
            string result = "";
            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];
                string currentVar = "";
                result += c;
                if (result.Length >= variableName.Length&&!IsOperator(input[i])&&!char.IsDigit(input[i]))
                {
                    currentVar = result.Substring(i-variableName.Length+1);
                }
                if (currentVar == variableName && (input.Length - result.Length == 0 || IsOperator(input[i + 1])) && (i == variableName.Length - 1 || IsOperator(input[i - variableName.Length])))
                {
                    result = result.Substring(0, result.Length - variableName.Length) + input.Substring(i + 1, input.Length - 1 - i);
                    result = result.Insert(i-variableName.Length+1,"("+variableValue+")");
                    PrintColour(variableName + " --> " + variableValue, true);
                    return ReplaceTempVariables(result, variableName, variableValue);
                }
            }
            return result;
        }
        private static bool ContainsVariable(string input, string variableName)
        {
            string result = "";
            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];
                string currentVar = "";
                result += c;
                if (result.Length >= variableName.Length)
                {
                    currentVar = result.Substring(i - variableName.Length + 1);
                }
                if (currentVar == variableName && (input.Length - result.Length == 0 || IsOperator(input[i + 1])) && (i == variableName.Length - 1 || IsOperator(input[i - variableName.Length])))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// See if user is modifying a variable that already exists
        /// </summary>
        /// <param name="userINPUT">users input</param>
        /// <returns></returns>
        public static bool ModifyVariables(string userINPUT)
        {
            bool mainresult = false;
            if (userINPUT.Contains('=') && userINPUT.Substring(0,3) != "var")
            {
                Dictionary<string, string> toreplace = new Dictionary<string, string>();
                List<string> tonull = new List<string>();
                foreach (var pair in tempVariables)
                {
                    if (ContainsVariable(userINPUT, pair.Key))
                    {
                        var variableName = pair.Key;
                        string result = "";
                        bool add = true;
                        for (int i = 0; i < userINPUT.Length; ++i)
                        {
                            char c = userINPUT[i];
                            string currentVar = "";
                            result += c;
                            if (result.Length >= variableName.Length)
                            {
                                currentVar = result.Substring(i - variableName.Length + 1);
                            }
                            if (currentVar == variableName && (userINPUT.Length - result.Length == 0 || (userINPUT[i + 1])=='=') && (i == variableName.Length - 1 || (userINPUT[i - variableName.Length])=='='))
                            {
                                //The last x characters of result are a variable and the next character is an '=' sign
                                var value = BitCalculate(RemoveBrackets(ReplaceTempVariables(userINPUT.Substring(i + 2, userINPUT.Length - i - 2)), 'u'), 'u');
                                if (value != "null")
                                {
                                    toreplace.Add(variableName, value);
                                }
                                else
                                {
                                    tonull.Add(variableName);
                                }
                                mainresult = true;
                                add = false;
                                break;
                                //pair.Value = sINPUT.Substring(i + 1, sINPUT.Length - i - 1);
                            }
                        }
                        if (add)
                        {
                            toreplace.Add(variableName, tempVariables[variableName]);
                        }
                    }
                }
                //tempVariables = toreplace;
                Dictionary<string, string> dresult = new Dictionary<string, string>();
                foreach (var i in toreplace)    
                {
                    dresult.Add(i.Key, i.Value);
                }
                foreach (var i in tempVariables)
                {
                    if (!dresult.ContainsKey(i.Key) && !tonull.Contains(i.Key))
                    {
                        dresult.Add(i.Key, i.Value);
                    }
                }
                tempVariables = dresult;
            }
            return mainresult;
        }
        public static void DoPrint(string sINPUT)
        {
            sINPUT = sINPUT.Substring(6);
            sINPUT = BitCalculate(RemoveBrackets(sINPUT.Substring(0, ClosingBracket(sINPUT, 0)), 'u'), 'u');
            PrintColour(sINPUT, false);
        }
        public static Dictionary<string, string> tempVariables = new Dictionary<string, string>();
        public static void DefineTempVariable(string variable)
        {
            string[] strings = variable.Split('=');
            int equalsIDX = strings[0].Length - 1;
            string value = strings[1];
            string variableName = variable.Substring(0, equalsIDX+1);
            if (variableName.Any(c => !char.IsLetter(c) && c != ' ') || value.Contains(','))
            {
                PrintColour("Invalid", false);
                return;
            }
            tempVariables.Add(variableName, value);
        }
        public static string ReplaceTempVariables(string sINPUT)
        {
            foreach (var pair in tempVariables)
            {
                sINPUT = ReplaceTempVariables(sINPUT, pair.Key, pair.Value);
            }
            return sINPUT;
        }
        private static int NextOperatorIDX(string input, int currIDX)
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
                    char.IsLetter(input[i]))
                {
                    return i;
                }
            }
            return input.Length;
        }
        private static int NextOperatorIDX_NoLetter(string input, int currIDX)
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
        private static int NextColonIDX(string input, int currIDX)
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

        private static int LastOperatorIDX(string input, int currIDX)
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

        public static string DoubleToBin(double input, bool flipped = false)
        {
            string binVal = Convert.ToString(BitConverter.DoubleToInt64Bits(input), 2);
            string result = "";
            for (int i = 0; i < 64-binVal.Length; ++i)
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
        public static string FloatToBin(float input, bool flipped = false)
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
        private static void PrintHelp()
        {
            PrintColour("<<n means print out of 1<<by n:                                Example: <<1");
            PrintColour("a<<n means print out of a<<by n:                               Example: 2<<3");
            PrintColour(">>n means print out of (specified type).maxvalue>>by n:        Example: >>1");
            PrintColour("a>>n means print out of a>>by n:                               Example: 64>>2");
            Console.WriteLine();
            PrintColour("n means print out n in binary:                                 Example: 1");
            PrintColour("b_n means convert (binary)n to a 64 bit unsigned int:          Example: b_1001");
            PrintColour("b_n can be used in conjunction with other operators as well:   Example: #ff * (b_1011<<#e)");
            PrintColour("h n means print out n in hexadecimal:                          Example: h255");
            PrintColour("#n means print out (hex)n in binary:                           Example: #ff");
            PrintColour("#n can be used in junction with other operators:               Example: #ff * (1<<#e)");
            PrintColour("#_n means print out (hex)n in binary:                          Example: #_ff");
            PrintColour("hrgb n means print out (hex)n as rgb colour:                   Example: hrgb#ffffff or hrgbffffff");
            Console.WriteLine();
            PrintColour("f means print out flipped:                                     Example: f2<<3");
            PrintColour("i means print out as 32 bit:                                   Example: i1");
            PrintColour("s means print out as 16 bit:                                   Example: s1");
            PrintColour("b means print out as 8 bit:                                    Example: b1");
            PrintColour("Default is 64 bits");
            Console.WriteLine();
            PrintColour("v means previous value                                         Example: v+1");
            PrintColour("adv means print out previous value as double                   Example: adv");
            PrintColour("bsv means print out previous value as binary                   Example: sv");
            PrintColour("afv means print out previous value as float                    Example: sv");
            PrintColour("rf means to reverse the default flip value.                    Example: rf");
            PrintColour("avg(num1,num2) will print out the average value of a collection of numbers");
            PrintColour("For example: avg(1,2,3,4,5,6,7,8,9) = 5");
            PrintColour("For example: avg(1.2,1.4,1.8,304.566,32.4) = 68.27319");
            Console.WriteLine();
            PrintColour("|n means bitwise or n with previous answer:                    Example: |1");
            PrintColour("&n means bitwise and n with previous answer:                   Example: &1");
            PrintColour("^n means bitwise exor n with previous answer:                  Example ^1");
            Console.WriteLine();
            PrintColour("You can also do normal bitwise operations");
            PrintColour("a|n  means bitwise or a with previous n:                       Example: 5|1");
            PrintColour("a&n  means bitwise and a with previous n:                      Example: 6&4");
            PrintColour("a^n  means bitwise exor a with previous n:                     Example: 7^1");
            Console.WriteLine();
            PrintColour("ran(minval, maxval) will return a random number within min val and max val");
            PrintColour("Minval: inclusive, maxval: inclusive                           Example: 1+ran(5,8)+1");
            PrintColour("This will print out 5,6,7 or 8 as its random numbers an 7,8,9 or 10 as its final result");
            PrintColour("dt will print out the current date and time");
            Console.WriteLine();
            PrintColour("You can use normal operators as well, these include: +,-,/,*");
            PrintColour("As with the bitwise operators if you start a statement with an operator, it will substitute the value on the left of it with the previous answer");
            PrintColour("If you create a negative number, the bits will \"wrap around\", so you will get a large positive number");
            PrintColour("Divide by also does a floor, so 5/2 = 2");
            PrintColour("Bitwise operators are computated last, so 5*2^2=8 instead of 5*2^2=0");
            Console.WriteLine();                                                                        
            PrintColour("#define allows you to define variables                         Example: #define bob = 24");
            PrintColour("#define also allows you to use functions                       Example: #define bob = 6<<2");
            PrintColour("#define can also be used to override system functions          Example: #define f = sv");
            PrintColour("you cannot override #define or #del");
            PrintColour("#del v can be used to delete variables                         Example: #del variable");
            PrintColour("You cannot modify \"#define\" type variables, you can only overwrite them");
            PrintColour("#defunc allows you to define functions");
            PrintColour("Uses the format \'#defunc name(var1,var2,...) function\'");
            PrintColour("In the part labelled \'function\', you write your function, anywhere that you type \'var1\' will be replaced with the variable the user inputs");
            PrintColour("Functions can be fun by doing this: name(1,2)");
            PrintColour("Where name is the name of the function and 1 and 2 are the numbers/strings inputted into the function");
            PrintColour("#delfunc name is used to delete functions called name");
            PrintColour("If you define a function that has already been defined, it will overwrite the previous functions definition");
            PrintColour("showfunc will print out the storage file for the functions, showing the user what the functions do");
            Console.WriteLine();
            PrintColour("You can add a description to a function by adding at the end of the function ///Comment goes here");
            PrintColour("To have description go over multiple lines just use \\n        Example: ///Comments are cool \\n whoa new line");
            PrintColour("You can have comments print out inside a function as well, for example:");
            PrintColour("Just put the thing you want to comment out between // and \\\\:  //Comment goes here\\\\");
            PrintColour("#defunc myfunc(x,y)x*y//Multiplying first\\\\;-y//Then minusing\\\\");
            PrintColour("You can show the description for a user function if you type help-funcname");
            Console.WriteLine();
            PrintColour("You can define temporary variables that will be deleted when you close the console windows");
            PrintColour("var variablename = value is the format for defining variables  Example: var bob = 12");
            PrintColour("You cannot override variables, but you can modify them         Example: var bob = 12; bob = 10");
            PrintColour("To increment a variable, just add something to itself          Example: bob = bob + 1");
            PrintColour("They do not have to be on the same line for you to modify them Example: bob = 10");
            PrintColour("To delete a variable just set the variable name to null        Example: bob = null");
            PrintColour("Variables can be integers or strings");
            PrintColour("dtv means display all temporary variables                      Example: dtv");
            Console.WriteLine();
            PrintColour("a<b will return true if a is smaller than b otherwise false    Example: <<4>15");
            PrintColour("a>b will return true if a is larger than b otherwise false     Example: <<4>15");
            PrintColour("a==b will return true if a equals b otherwise false            Example: <<4==16");
            PrintColour("a!=b will return true if a does not equal b otherwise false    Example: <<4!=16");
            Console.WriteLine();
            PrintColour("You can also do conditional statements. Format is condition==value?action:otheraction");
            PrintColour("An example of this is 4 == 4 ? print(\"four is equal to four\") : print(\"four is not equal to four\")");
            Console.WriteLine();
            PrintColour("Standalone trigenometary functions are also available. You can use sin, cos and tan");
            PrintColour("You can use arcsin, arctan and arccos as well");
            PrintColour("These will not work with other functions");
            Console.WriteLine();
            PrintColour("You can use the print(v) function to print out a variable      Example: print(4<<8)");
            PrintColour("You should use speech marks to print text, this will avoid you running functions that you didn't want to run");
            PrintColour("np (function) will only print out the working for a function and will not print out the result of the function");
            PrintColour("Example: np (<<6*(3-2))|v");
            PrintColour("This is useful in loop statements. e.g. loop 64: np |<<i; i==63?v");
            PrintColour("This will fill the bitboard up with ones and when it gets to the last bit (i == 63) it will print out the bitboard");
            PrintColour("In the same format, you can use nw to not print out any workings");
            PrintColour("You can type pnw to change the default value for this session for printing out workings");
            PrintColour("You can type fnpw to change the default value for all sessions");
            PrintColour("You can use a \"loop\" function to loop over values from one to a specified value");
            PrintColour("These work like for loops, wherever you say \'i\' in your function, that will be replaced with the current iterating \"index\"");
            PrintColour("an example of a loop function is: loop 10: print(i)");
            PrintColour("This will print out all the values from 0 to 9");
            PrintColour("You can seperate functions in with semicolons Example: loop 10: print(i); <<i");
            PrintColour("This will print out i's value and then show you in binary form what happens when you bitshift left 1 by i");
            PrintColour("asci(\"textvalue\") will print out the \"text value\" in large ascii letters, however, if your sentence is too long then some lines will wrap around and it will no longer look like ascii text");
            PrintColour("                                                               Example: asci(\"cool ascii text\")");
            PrintColour("In the same format you can use ascib(value) to print numbers out in a binary font");
            Console.WriteLine();
            PrintColour("Seperating functions with semicolons also works for storing previous variables");
            PrintColour("For example:  5==5?2:4;^3");
            PrintColour("Since 5 ==5 is true, this will print out 2, store that value and then exor it with three, this means that it can be used for changing variables");
            PrintColour("For example: loop 10: i == 5?2:4;^3");
            PrintColour("When i is five, it will exor 3 with two, otherwise it will exor 3 with four");
            Console.WriteLine();
            PrintColour("doub val(double) will print out the double value in binary form");
            PrintColour("float val(float) will print out the float value in binary form");
            Console.WriteLine();
            PrintColour("cv is used to clear all variables                              Example: cv");
            PrintColour("dv is used to display all variables                            Example: dv");
            PrintColour("ati (stringvalue) will print out an alphabet times off all the letters in string value");
            PrintColour("ati ab will mean 1*2 because a = 1, b = 2, c = 3.... etc");
            var ss = File.ReadAllLines(DataFilePath);
            if (ss.Count() != 0) {
                Console.WriteLine();
                PrintColour("          User defined variables          ");

                foreach (var s in ss)
                {
                    PrintColour(Regex.Replace(s, ",", "="));
                }
                PrintColour("          User defined variables          ");
                PrintColour("          User defined functions          ");
                PrintDescription();
                PrintColour("          User defined functions          ");
            }
            Console.WriteLine();
            PrintColour("Order of events: isFlipped, isBitwiseOperator, bitcount, function");
            PrintColour("You may use spaces");
        }
        static string DataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\DevTools";

        static string DataFile = @"\data.txt";
        static string DataFilePath = DataDirectory + DataFile;
                      
        static string WorkingsFile = @"\workings.txt";
        static string WorkingsFilePath = DataDirectory + WorkingsFile;

        static string funcsFile = @"\funcs.txt";
        static string FuncFilePath = DataDirectory + funcsFile;

        static void DefineVariable(string variable)
        {
            string[] strings = variable.Split('=');
            int equalsIDX = strings[0].Length-1;
            string value = strings[1];
            string variableName = variable.Substring(7,equalsIDX-6);
            if (variableName.Any(c=>!char.IsLetter(c) && c != ' ') || value.Contains(','))
            {
                PrintColour("Invalid variable name", false);
                return;
            }
            List<string> contents = File.ReadAllLines(DataFilePath).ToList();
            for (int i = 0; i < contents.Count; i++)
            {
                string s = contents[i];
                if (s.Split(',')[0] == variableName)
                {
                    var ss = s.Split(',');
                    ss[1] = value;
                    contents[i] = ss[0] + ',' + ss[1];
                    File.WriteAllLines(DataFilePath, contents);
                    return;
                }
            }
            contents.Add(string.Format("{0},{1}", variableName, value));
            File.WriteAllLines(DataFilePath, contents);
        }
        static void DefineFunction(string function)
        {
            function = function.Substring("#defunc".Length);

            var prev = File.ReadAllLines(FuncFilePath).ToList();
            var name = function.Substring(0, function.IndexOf('('));
            List<string> toremove = new List<string>();
            foreach (var s in prev)
            {
                var bracketidx = s.IndexOf('(');
                var substr = s.Substring(0,bracketidx);
                if (substr == name) //Already defined function
                {
                    toremove.Add(s);
                }
            }
            foreach (var s in toremove)
            {
                prev.Remove(s);
            }
            File.WriteAllLines(FuncFilePath, prev.ToArray());

            string result = File.ReadAllText(FuncFilePath) + function + "\n";

            File.WriteAllText(FuncFilePath, result);
        }
        static void DeleteFunction(string name)
        {
            var prev = File.ReadAllLines(FuncFilePath).ToList();
            List<string> toremove = new List<string>();
            foreach (var s in prev)
            {
                var bracketidx = s.IndexOf('(');
                var substr = s.Substring(0, bracketidx);
                if (substr == name) //Already defined function
                {
                    toremove.Add(s);
                }
            }
            foreach (var s in toremove)
            {
                prev.Remove(s);
            }
            File.WriteAllLines(FuncFilePath, prev.ToArray());
        }
        static void PrintDescription(string name)
        {
            var prev = File.ReadAllLines(FuncFilePath).ToList();
            foreach (var s in prev)
            {
                var bracketidx = s.IndexOf('(');
                var substr = s.Substring(0, bracketidx);
                if (substr == name) //Already defined function
                {
                    ShowDescription(s);
                }
            }
        }
        static void PrintDescription()
        {
            var prev = File.ReadAllLines(FuncFilePath).ToList();
            foreach (var s in prev)
            {
                PrintColour(s + ":");
                ShowDescription(s);
            }
        }
        public static string ReplaceVariables(string input)
        {
            string i = input;
            foreach (var s in File.ReadAllLines(DataFilePath).OrderByDescending(s=>s.Length))
            {
                if (!s.Contains(','))
                {
                    File.WriteAllText(DataFilePath, "");
                    PrintColour("All variables cleared because of invalid input. DO NOT EDIT THE VARIABLES FILE", false);
                    return "";
                }

                var ss = s.Split(',');
                i = Regex.Replace(i,ss[0], "("+ss[1]+")");
            }
            foreach (var s in File.ReadAllLines(FuncFilePath).OrderByDescending(s => s.Length))
            {
                if (s=="")
                {
                    continue;
                }
                if (!s.Contains('('))
                {
                    File.WriteAllText(FuncFilePath, "");
                    PrintColour("All variables cleared because of invalid input. DO NOT EDIT THE VARIABLES FILE", false);
                    return "";
                }
                var name = s.Split('(')[0];
                if (i.Contains(name))
                {
                    string replacestring = s;
                    int closingBracketidx = ClosingBracket(replacestring, name.Length + 1);
                    replacestring = replacestring.Substring(closingBracketidx + 1);

                    string[] values = i.Substring(name.Length+1, closingBracketidx-name.Length-1).Split(',');
                    string[] names = s.Substring(name.Length + 1, ClosingBracket(s, name.Length + 1) - name.Length - 1).Split(',');
                    Dictionary<string, int> variableValues = new Dictionary<string, int>();
                    //Iterate through here and add the variable values to the variable names
                    //swap out the variable values for the variable names in the function stored file
                    //Replace the function text with the text found in the file

                    for (int idx = 0; idx < values.Length; ++idx)
                    {
                        replacestring = ReplaceTempVariables(replacestring, names[idx], values[idx]);
                    }
                    if (replacestring.Contains("///")) {
                        replacestring = replacestring.Substring(0, replacestring.IndexOf("///"));
                    }
                    i = replacestring;
                    if (i.IndexOf(";") != 0 && (i.IndexOf(";") < i.IndexOf("loop")||i.IndexOf("loop")==-1) && !i.Contains("#def"))
                    {
                        var strs = i;
                        if (i.Contains("loop")) {
                            strs = i.Substring(0, i.IndexOf("loop"));
                        }
                        foreach (var str in strs.Split(';'))
                        {
                            if (str != "") {
                                MainMethod(str);
                            }
                        }
                        i = i.Substring(strs.Length);
                    }
                }
            }
            if (i != input)
            {
                PrintColour(i, true);
            }
            input = RemoveRandom(input);
            return i;
        }
        public static string RemoveAndReplace(int startIDX, int endIDX, string replaceWith, string input)
        {
            return input.Substring(0, startIDX) + replaceWith + input.Substring(endIDX, input.Length - endIDX);
        }
        public static string TextBetween(string input, int startIDX, int endIDX)
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
        public static int NextBracket(string input, int idx)
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
        static string DoubleRemoveBrackets(string s)
        {
            string buffer = "";
            int firstBracketIDX = 0;
            bool addingToBuffer = false;
            for (int i = 0; i < s.Length; ++i)
            {
                var c = s[i];

                if (c == ')')
                {
                    string betweenBrackets = TextBetween(s, firstBracketIDX + 1, i - 1);
                    string total = DoubleCalculate(betweenBrackets);
                    string nextString = "";
                    for (int secondIDX = 0; secondIDX < s.Length; ++secondIDX)
                    {
                        if (secondIDX < firstBracketIDX || secondIDX > i)
                        {
                            nextString += s[secondIDX];
                        }
                        else if (secondIDX == firstBracketIDX)
                        {
                            nextString += total;
                        }
                    }
                    if (nextString.Contains('('))
                    {
                        return DoubleRemoveBrackets(nextString);
                    }
                    else
                    {
                        return nextString;
                    }
                }
                else if (addingToBuffer)
                {
                    buffer += c;
                }
                if (c == '(')
                {
                    if (addingToBuffer == true)
                    {
                        int nextBracketIDX = NextBracket(s, i);
                        if (nextBracketIDX == -1)
                        {
                            throw new Exception();
                        }
                        if (s[nextBracketIDX] == ')') //Is this the last layer of brackets?
                        {
                            string betweenBrackets = TextBetween(s, i + 1, nextBracketIDX - 1);
                            string total = DoubleCalculate(betweenBrackets);
                            string nextString = "";
                            for (int secondIDX = 0; secondIDX < s.Length; ++secondIDX)
                            {
                                if (secondIDX < i || secondIDX > nextBracketIDX)
                                {
                                    nextString += s[secondIDX];
                                }
                                else if (secondIDX == i)
                                {
                                    nextString += total;
                                }
                            }
                            return DoubleRemoveBrackets(nextString);
                        }
                        else
                        {
                            firstBracketIDX = i;
                        }
                    }
                    else
                    {
                        addingToBuffer = true;
                        firstBracketIDX = i;
                    }
                }
            }
            return s;
        }
        static string RemoveBrackets(string s, char chosenType)
        {
            string buffer = "";
            int firstBracketIDX = 0;
            bool addingToBuffer = false;
            for (int i = 0; i < s.Length; ++i)
            {
                var c = s[i];

                if (c == ')')
                {
                    string betweenBrackets = TextBetween(s, firstBracketIDX + 1, i - 1);
                    string total = BitCalculate(betweenBrackets, chosenType);
                    string nextString = "";
                    for (int secondIDX = 0; secondIDX < s.Length; ++secondIDX)
                    {
                        if (secondIDX < firstBracketIDX || secondIDX > i)
                        {
                            nextString += s[secondIDX];
                        }
                        else if (secondIDX == firstBracketIDX)
                        {
                            nextString += total;
                        }
                    }
                    if (nextString.Contains('('))
                    {
                        return RemoveBrackets(nextString, chosenType);
                    }
                    else
                    {
                        return nextString;
                    }
                }
                else if (addingToBuffer)
                {
                    buffer += c;
                }
                if (c == '(')
                {
                    if (addingToBuffer == true)
                    {
                        int nextBracketIDX = NextBracket(s, i);
                        if (nextBracketIDX == -1)
                        {
                            throw new Exception();
                        }
                        if (s[nextBracketIDX] == ')') //Is this the last layer of brackets?
                        {
                            string betweenBrackets = TextBetween(s, i + 1, nextBracketIDX - 1);
                            string total = BitCalculate(betweenBrackets, chosenType);
                            string nextString = "";
                            for (int secondIDX = 0; secondIDX < s.Length; ++secondIDX)
                            {
                                if (secondIDX < i || secondIDX > nextBracketIDX)
                                {
                                    nextString += s[secondIDX];
                                }
                                else if (secondIDX == i)
                                {
                                    nextString += total;
                                }
                            }
                            return RemoveBrackets(nextString, chosenType);
                        }
                        else
                        {
                            firstBracketIDX = i;
                        }
                    }
                    else
                    {
                        addingToBuffer = true;
                        firstBracketIDX = i;
                    }
                }
            }
            return s;
        }
        private static string DoubleRemoveMultiplyDivide(string input)
        {
            if (input.Contains("*") || input.Contains("/"))
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] =='/' || input[i] == '*')
                    {
                        int lastOperatorIDX = LastOperatorIDX(input, i - 2);
                        int nextOperatorIDX = DoubleNextOperatorIDX(input, i + 1);
                        string sub = input.Substring(lastOperatorIDX, (nextOperatorIDX - lastOperatorIDX));
                        string result = DoubleCalculateMultiplyDivide(sub);
                        string toReturn = string.Format("{0}{1}{2}", input.Substring(0, lastOperatorIDX == 0? 0 : lastOperatorIDX+1), result, input.Substring(nextOperatorIDX, input.Length - nextOperatorIDX));
                        return DoubleRemoveMultiplyDivide(toReturn);
                    }
                }
            }
            return input;
        }

        private static int DoubleNextOperatorIDX(string input, int currIDX)
        {
            for (int i = currIDX; i < input.Length; i++)
            {
                if ((input[i] == '<' ||
                    input[i] == '>' ||
                    input[i] == '+' ||
                    input[i] == '-' ||
                    input[i] == '*' ||
                    input[i] == '/' ||
                    input[i] == '^' ||
                    input[i] == '&' ||
                    input[i] == '|' ||
                    char.IsLetter(input[i]))
                    &&(i == 0 | !IsOperator(input[i-1])))
                {
                    return i;
                }
            }
            return input.Length;
        }
        public static double Average(string sINPUT)
        {
            sINPUT = sINPUT.Substring(4);
            sINPUT = sINPUT.Remove(sINPUT.Length - 1, 1);
            var snums = sINPUT.Split(',');
            List<double> doubles = new List<double>();
            foreach (var s in snums)
            {
                doubles.Add(double.Parse(s));
            }
            return doubles.Average();
        }
        private static string RemoveMultiplyDivide(string input)
        {
            if (input.Contains("*") || input.Contains("/"))
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] =='/' || input[i] == '*')
                    {
                        int lastOperatorIDX = LastOperatorIDX(input, i - 2);
                        int nextOperatorIDX = NextOperatorIDX(input, i + 1);
                        string sub = input.Substring(lastOperatorIDX, (nextOperatorIDX - lastOperatorIDX));
                        string result = CalculateMultiplyDivide(sub);
                        string toReturn = string.Format("{0}{1}{2}", input.Substring(0, lastOperatorIDX == 0? 0 : lastOperatorIDX+1), result, input.Substring(nextOperatorIDX, input.Length - nextOperatorIDX));
                        return RemoveMultiplyDivide(toReturn);
                    }
                }
            }
            return input;
        }
        private static string DoubleCalculateMultiplyDivide(string sINPUT)
        {
            if (sINPUT.Contains("*"))
            {
                double input;
                var strings = sINPUT.Split('*');
                if (strings[0] == "")
                {
                    strings[0] = lastInput.ToString();
                }
                double first = 0;
                double second = 0;
                double.TryParse(strings[1], out first);
                double.TryParse(strings[0], out second);
                if (strings[0][0] == '-')
                {
                    double.TryParse(strings[0].Substring(1), out second);
                    second = -second;
                }
                if (strings[1][0] == '-')
                {
                    double.TryParse(strings[1].Substring(1), out first);
                    first = -first;
                }
                input = second * first;
                PrintColour(string.Format("{0} * {1} = {2}", second, first, input), true);
                return input.ToString();
            }
            else if (sINPUT.Contains("/"))
            {
                double input;
                var strings = sINPUT.Split('/');
                if (strings[0] == "")
                {
                    strings[0] = lastInput.ToString();
                }
                double first = 0;
                double second = 0;
                double.TryParse(strings[1], out first);
                double.TryParse(strings[0], out second);
                if (strings[0][0] == '-')
                {
                    double.TryParse(strings[0].Substring(1), out second);
                    second = -second;
                }
                if (strings[1][0] == '-')
                {
                    double.TryParse(strings[1].Substring(1), out first);
                    first = -first;
                }
                input = second / first;
                PrintColour(string.Format("{0} / {1} = {2}", second, first, input), true);
                return input.ToString();
            }
            return "0";
        }
        static string prevanswer = "";
        /// <summary>
        /// DOES NOT WORK
        /// DO NOT IMPLEMENT
        /// </summary>
        private static void GenerateAlgebra()
        {
            Random r = new Random();
            var first = r.Next(-8,8);
            var second = r.Next(-8,8);
            if (first == 0)
            {
                first = 1;
            }
            if (second == 0)
            {
                second = 5;
            }
            if (first+second == 0)
            {
                GenerateAlgebra();
                return;
            }

            int a = 0;
            Random rand = new Random();
            do
            {
                int n = rand.Next(1, 5);
                if (first % n == 0  )
                {
                    a = first / n;
                }
                if (second % n == 0)
                {
                    a = second / n;
                }
            } while (a == 0);
            int c = (second + first) / (a*2);
            PrintColour(string.Format("{0}x² {1} {2}x {3} {4}", a, first + second <= -1 ? "" : "+", first + second, a <= -1 ? "" : "+", a));
            string firstbrackets = "";
            string secondbrackets = "";
            if (first < 0 && second < 0)
            {
                firstbrackets = string.Format("({0}x {1} {2})", GCD(first, a), GCD(second, a) <= -1 ? "" : "+", GCD(second, a));
                secondbrackets = string.Format("({0}x {1} {2})", first / GCD(first, a), second / GCD(second, a) <= -1 ? "" : "+", second / GCD(second, a));
            }
            else
            {
                firstbrackets = string.Format("({0}x {1} {2})", GCD(first, a), GCD(second, a) <= -1 ? "" : "+", GCD(second, a));
                secondbrackets = string.Format("({0}x {1} {2})", first / GCD(first, a), a / GCD(second, a) <= -1 ? "" : "+", a / GCD(second, a));
            }
            PrintColour(firstbrackets+secondbrackets);
        }
        static int GCD(int[] numbers)
        {
            return numbers.Aggregate(GCD);
        }

        static int GCD(int a, int b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }
        private static string CalculateMultiplyDivide(string sINPUT)
        {
            if (sINPUT.Contains("*"))
            {
                ulong input;
                var strings = sINPUT.Split('*');
                if (strings[0] == "")
                {
                    strings[0] = lastInput.ToString();
                }
                var first = 0ul;
                var second = 0ul;
                ulong.TryParse(strings[1], out first);
                ulong.TryParse(strings[0], out second);
                input = second * first;
                PrintColour(string.Format("{0} * {1} = {2}", second, first, input), true);
                return input.ToString();
            }
            else if (sINPUT.Contains("/"))
            {
                ulong input;
                var strings = sINPUT.Split('/');
                if (strings[0] == "")
                {
                    strings[0] = lastInput.ToString();
                }
                var first = 0ul;
                var second = 0ul;
                ulong.TryParse(strings[1], out first);
                ulong.TryParse(strings[0], out second);
                if (first != 0)
                {
                    input = second / first;
                }
                else
                {
                    input = 0;
                }
                PrintColour(string.Format("{0} / {1} = {2}", second, first, input), true);
                return input.ToString();
            }
            return "0";
        }
        /// <summary>
        /// Looks for boolean statements: ==,!=,>,<. Processes their values
        /// RETURNED VALUE IS A STRING. DO NOT PROCESS AS BOOL
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CheckForBooleans(string input, char type)
        {
            if (StringContains(input, '<'))
            {
                var strings = SplitAt(input, '<');
                strings[0] = BitCalculate(strings[0], type);
                strings[1] = BitCalculate(strings[1], type);
                if (ulong.Parse(strings[0])<ulong.Parse(strings[1]))
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            if (StringContains(input, '>'))
            {
                var strings = SplitAt(input, '>');
                strings[0] = BitCalculate(strings[0], type);
                strings[1] = BitCalculate(strings[1], type);
                if (ulong.Parse(strings[0]) > ulong.Parse(strings[1]))
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            if (input.Contains("=="))
            {
                string[] strings=  input.Split(new string[] { "==" }, StringSplitOptions.None); 
                strings[0] = BitCalculate(strings[0], type);
                strings[1] = BitCalculate(strings[1], type);
                if (ulong.Parse(strings[0]) == ulong.Parse(strings[1]))
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            if (input.Contains("!="))
            {
                string[] strings = input.Split(new string[] { "!=" }, StringSplitOptions.None);
                strings[0] = BitCalculate(strings[0], type);
                strings[1] = BitCalculate(strings[1], type);
                if (ulong.Parse(strings[0]) != ulong.Parse(strings[1]))
                {
                    return "true";
                }
                                else
                {
                    return "false";
                }
            }
            return input;
        }
        private static List<string> SplitAt(string input, char v)
        {
            List<string> result = new List<string>();
            string buffer = "";
            bool nextCantBeV = false;
            foreach (var c in input)
            {
                if (c == v && nextCantBeV)
                {
                    nextCantBeV = false;
                    buffer += c;
                    continue;
                }
                if (nextCantBeV)
                {
                    result.Add(RemoveLast(buffer));
                    buffer = "";
                    nextCantBeV = false;
                    buffer += c;
                    continue;
                }
                if (c == v && buffer.Length != 0 && buffer[buffer.Length - 1] != v)
                {
                    nextCantBeV = true;
                }
                buffer += c;
            }
            result.Add(buffer);
            return result;
        }
        private static string RemoveLast(string buffer)
        {
            if (buffer == "")
            {
                return "";
            }
            List<char> chars = buffer.ToArray().ToList();
            chars.RemoveAt(chars.Count()-1);
            return new string(chars.ToArray());
        }
        private static bool StringContains(string input, char v)
        {
            string buffer = "";
            bool nextCantBeV = false;
            foreach (var c in input)
            {
                if (c==v && nextCantBeV)
                {
                    nextCantBeV = false;
                    buffer += c;
                    continue;
                }
                if (nextCantBeV)
                {
                    return true;
                }
                if (c == v && buffer.Length != 0 && buffer[buffer.Length-1] != v)
                {
                    nextCantBeV = true;
                }
                buffer += c;
            }
            return false;
        }
        private static string BitCalculate(string input, char chosenType)
        {
            var sINPUT = RemoveMultiplyDivide(RemoveBitShift(input, chosenType));

            if (sINPUT.Contains('&') || 
                sINPUT.Contains('|') || 
                sINPUT.Contains('^') || 
                sINPUT.Contains('+') || 
                sINPUT.Contains('-'))
            {
                string firstUlong = "";
                int firstUlongStart_IDX = 0;
                bool firstTime = true;
                string secondUlong = "";
                char _operator = ' ';
                bool operatorSet = false;
                for (int i = 0; i < sINPUT.Length; i++)
                {
                    char c = sINPUT[i];
                    if (char.IsNumber(c))
                    {
                        //is it a number
                        if (!operatorSet)
                        {
                            firstUlong += c;
                            if (firstTime)
                            {
                                firstTime = false;
                                firstUlongStart_IDX = i;
                            }
                        }
                        else
                        {
                            secondUlong += c;
                        }
                    }
                    else if (c == '|' || c == '&' || c == '^' || c == '+' || c == '-')
                    {
                        if (secondUlong != "")
                        {
                            ulong result = 0ul;
                            if (firstUlong == "")
                            {
                                firstUlong = lastInput.ToString();
                                sINPUT = sINPUT.Insert(0, firstUlong);
                                //firstUlongStart_IDX = 1;
                            }
                            switch (_operator)
                            {
                                case '&':
                                    result = ulong.Parse(firstUlong) & ulong.Parse(secondUlong);
                                    break;
                                case '|':
                                    result = ulong.Parse(firstUlong) | ulong.Parse(secondUlong);
                                    break;
                                case '^':
                                    result = ulong.Parse(firstUlong) ^ ulong.Parse(secondUlong);
                                    break;
                                case '+':
                                    result = ulong.Parse(firstUlong) + ulong.Parse(secondUlong);
                                    break;
                                case '-':
                                    result = ulong.Parse(firstUlong) - ulong.Parse(secondUlong);
                                    break;
                            }
                            PrintColour(sINPUT + " = " + RemoveAndReplace(firstUlongStart_IDX, i, result.ToString(), sINPUT), true);
                            return BitCalculate(RemoveAndReplace(firstUlongStart_IDX, i, result.ToString(), sINPUT), chosenType);
                        }
                        operatorSet = true;
                        _operator = c;
                    }
                    else
                    {
                        if (secondUlong != "")
                        {
                            ulong result = 0ul;
                            if (firstUlong == "")
                            {
                                firstUlong = lastInput.ToString();
                                sINPUT = sINPUT.Insert(0, firstUlong);
                                //firstUlongStart_IDX = 1;
                            }
                            switch (_operator)
                            {
                                case '&':
                                    result = ulong.Parse(firstUlong) & ulong.Parse(secondUlong);
                                    break;
                                case '|':
                                    result = ulong.Parse(firstUlong) | ulong.Parse(secondUlong);
                                    break;
                                case '^':
                                    result = ulong.Parse(firstUlong) ^ ulong.Parse(secondUlong);
                                    break;
                                case '+':
                                    result = ulong.Parse(firstUlong) + ulong.Parse(secondUlong);
                                    break;
                                case '-':
                                    result = ulong.Parse(firstUlong) - ulong.Parse(secondUlong);
                                    break;
                            }
                            PrintColour(sINPUT + " = " + RemoveAndReplace(firstUlongStart_IDX, i, result.ToString(), sINPUT), true);
                            return BitCalculate(RemoveAndReplace(firstUlongStart_IDX, i, result.ToString(), sINPUT), chosenType);
                        }
                        firstUlong = "";
                        operatorSet = false;
                        secondUlong = "";
                    }
                }
                if (secondUlong != "")
                {
                    ulong result = 0ul;
                    if (firstUlong == "")
                    {
                        firstUlong = lastInput.ToString();
                        sINPUT = sINPUT.Insert(0, firstUlong);
                        //firstUlongStart_IDX = 1;
                    }
                    ulong first = ulong.Parse(firstUlong);
                    ulong second = ulong.Parse(secondUlong);
                    switch (_operator)
                    {
                        case '&':
                            result = ulong.Parse(firstUlong) & ulong.Parse(secondUlong);
                            break;
                        case '|':
                            result = ulong.Parse(firstUlong) | ulong.Parse(secondUlong);
                            break;
                        case '^':
                            result = first ^ second;
                            break;
                        case '+':
                            result = ulong.Parse(firstUlong) + ulong.Parse(secondUlong);
                            break;
                        case '-':
                            result = ulong.Parse(firstUlong) - ulong.Parse(secondUlong);
                            break;
                    }
                    PrintColour(sINPUT + " = " + RemoveAndReplace(firstUlongStart_IDX, sINPUT.Length, result.ToString(), sINPUT), true);
                    return BitCalculate(RemoveAndReplace(firstUlongStart_IDX, sINPUT.Length, result.ToString(), sINPUT), chosenType);
                }
            }
            return sINPUT;
        }
        private static string DoubleCalculate(string input)
        {
            input = Regex.Replace(input, "--", "+");
            var sINPUT = DoubleRemoveMultiplyDivide(input);
            if (sINPUT.Where(c=>c=='-').Count() == 1 && !sINPUT.Any(c=>c=='+') && sINPUT[0] == '-')
            {
                return sINPUT;
            }
            if (sINPUT.Contains('+') ||
                sINPUT.Contains('-'))
            {
                string firstUlong = "";
                int firstUlongStart_IDX = 0;
                bool firstTime = true;
                string secondUlong = "";
                char _operator = ' ';
                bool operatorSet = false;
                for (int i = 0; i < sINPUT.Length; i++)
                {
                    char c = sINPUT[i];
                    if (char.IsNumber(c) || c == '.')
                    {
                        //is it a number
                        if (!operatorSet)
                        {
                            firstUlong += c;
                            if (firstTime)
                            {
                                firstTime = false;
                                firstUlongStart_IDX = i;
                            }
                        }
                        else
                        {
                            secondUlong += c;
                        }
                    }
                    else if (c == '+' || c == '-')
                    {
                        if (secondUlong != "")
                        {
                            double result = 0ul;
                            if (firstUlong == "" && _operator == '-')//negative number
                            {
                                operatorSet = true;
                                firstUlong = '-' + secondUlong;
                                secondUlong = "";
                                continue;
                            }
                            if (firstUlong == "")
                            {
                                firstUlong = lastInput.ToString();
                                sINPUT = sINPUT.Insert(0, firstUlong);
                                //firstUlongStart_IDX = 1;
                            }
                            double first = double.Parse(firstUlong);
                            double second = double.Parse(secondUlong);
                            switch (_operator)
                            {
                                case '+':
                                    result = first + second;
                                    break;
                                case '-':
                                    result = first - second;
                                    break;
                            }
                            PrintColour(sINPUT + " = " + RemoveAndReplace(firstUlongStart_IDX, i, result.ToString(), sINPUT), true);
                            return DoubleCalculate(RemoveAndReplace(firstUlongStart_IDX, i, result.ToString(), sINPUT));
                        }
                        operatorSet = true;
                        _operator = c;
                    }
                    else
                    {
                        if (secondUlong != "")
                        {
                            double result = 0ul;
                            if (firstUlong == "")
                            {
                                firstUlong = lastInput.ToString();
                                sINPUT = sINPUT.Insert(0, firstUlong);
                                //firstUlongStart_IDX = 1;
                            }
                            double first = double.Parse(firstUlong);
                            double second = double.Parse(secondUlong);
                            switch (_operator)
                            {
                                case '+':
                                    result = first + second;
                                    break;
                                case '-':
                                    result = first - second;
                                    break;
                            }
                            PrintColour(sINPUT + " = " + RemoveAndReplace(firstUlongStart_IDX, i, result.ToString(), sINPUT), true);
                            return DoubleCalculate(RemoveAndReplace(firstUlongStart_IDX, i, result.ToString(), sINPUT));
                        }
                        firstUlong = "";
                        operatorSet = false;
                        secondUlong = "";
                    }
                }
                if (secondUlong != "")
                {
                    double result = 0ul;
                    if (firstUlong == "")
                    {
                        firstUlong = lastInput.ToString();
                        sINPUT = sINPUT.Insert(0, firstUlong);
                        //firstUlongStart_IDX = 1;
                    }
                    double first = double.Parse(firstUlong);
                    double second = double.Parse(secondUlong);
                    switch (_operator)
                    {
                        case '+':
                            result = first + second;
                            break;
                        case '-':
                            result = first - second;
                            break;
                    }
                    PrintColour(sINPUT + " = " + RemoveAndReplace(firstUlongStart_IDX, sINPUT.Length, result.ToString(), sINPUT), true);
                    return DoubleCalculate(RemoveAndReplace(firstUlongStart_IDX, sINPUT.Length, result.ToString(), sINPUT));
                }
            }
            return sINPUT;
        }

        public static string UlongToBin(ulong input, bool flipped)
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
    }
}