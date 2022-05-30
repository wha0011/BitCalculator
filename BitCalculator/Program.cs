using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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
        public static ulong lastInput = 0ul;
        public static bool lastWasDouble = false;

        static bool defaultFlipVal = false;
        public static string lastprint;
        static void Main(string[] args)
        {
            CheckDirectories(); //See if the file storing directories exist, if not, then create them
            CustomConsole.SetupConsole();

            bool first = true;
            while (true)
            {
                try
                {
                    Colorful.Console.Write("-->", Color.FromArgb(10, 181, 158)); //Header for text
                    string userInput = "";
                    if (args.Length != 0 && first) //Are we opening a file?
                    {
                        CustomConsole.PrintColour("Opening file: " + args[0]); //Inform the user
                        var extension = args[0].Split('.')[1]; //Get the file extension from the filepath
                        if (extension != "dcode")
                        {
                            CustomConsole.PrintColour("Unrecognized file extension: " + extension); //Only open .dcode files
                        }
                        else
                        {
                            string x = File.ReadAllText(args[0]).RemoveLineBreaks(); //Read the file in
                            DoMainMethod(x); //Run the functions in the file
                        }
                        first = false; //This is no longer the first loop, set first to false
                    }
                    else
                    {
                        userInput = CustomConsole.ReadLineOrEsc(); //Custom readline method to read text
                        DoMainMethod(userInput); //Run users command
                    }
                }
                catch (Exception e)
                {
                    //Catch any error

                    Colorful.Console.WriteLine(e.Message, Color.FromArgb(255, 10, 10));
                    if (!expectingError) //IF this error was thrown externally of the application, show stack trace for debugging purposes (still in beta)
                    {
                        Colorful.Console.WriteLine(e.StackTrace, Color.FromArgb(255, 10, 10));
                    }
                    expectingError = false;
                }
            }
        }

        public static bool expectingError; //Set this to true, and it means the application is throwing a custom error
        //This will not print out a stack traces

        public const string VERSION = "v1.1.4";

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
            if (!File.Exists(VersionFilePath))
            {
                File.CreateText(VersionFilePath);
            }
            if (!File.Exists(FuncFilePath))
            {
                File.CreateText(FuncFilePath);
            }

            try
            {
                string prevVersion = File.ReadAllText(VersionFilePath);

                if (prevVersion != VERSION) //Is this a new install?
                {
                    var userVars = Variables.UserVariables(); //Record the users variables
                    File.WriteAllText(FuncFilePath, Help.DEFAULTFUNCS); //Write the new help
                    var fileText = File.ReadAllLines(FuncFilePath); //Read the new help

                    userVars.AddRange(fileText.ToList()); //Add the users variables back

                    File.WriteAllLines(FuncFilePath, userVars); //Write the data to a file

                    File.WriteAllText(VersionFilePath, VERSION); //Write the new version
                }

                if (File.ReadAllText(FuncFilePath) == "")
                {
                    File.WriteAllText(FuncFilePath, Help.DEFAULTFUNCS);
                }
            }
            catch
            {
                Colorful.Console.WriteLine("DO NOT EDIT SYSTEM FILES. WORKINGS FILE HAS BEEN RESET");
            }
        }

        public static void DoMainMethod(string userINPUT, bool removeSpaces = true)
        {
            if (userINPUT.BeginsWith("help-")) //Show help for specific function. Specified after the -
            {
                Variables.PrintDescription(userINPUT.Substring(5)); //Print the help
                return;
            }
            if (userINPUT.ToLower() == "unittest")
            {
                foreach (var test in UnitTest.unitTests)
                {
                    test.Test();
                }
                return;
            }
            if (!userINPUT.Contains("loop") && !userINPUT.Contains("#defunc"))
            //Only run multiple functions if function is not a loop or defining a function
            {
                foreach (var s in userINPUT.Split(';')) //Split up the different user commands
                {
                    MainMethod(s, removeSpaces); //Run the main method on them
                }
            }
            else
            {
                if (userINPUT.IndexOf(";") != 0 && userINPUT.IndexOf(";") < userINPUT.IndexOf("loop") && !userINPUT.Contains("#def"))
                //Run the functions that are called before the loop. After the loop statement, all semicolons are assumed to be inside it
                {
                    var beforeLoop = userINPUT.Substring(0, userINPUT.IndexOf("loop"));
                    foreach (var str in beforeLoop.Split(';'))
                    {
                        if (str.RemoveSpaces() == "")
                        {
                            continue;
                        }
                        MainMethod(str, removeSpaces);
                    }
                    userINPUT = userINPUT.Substring(beforeLoop.Length); //Remove the previous statements from the userinputs
                }
                MainMethod(userINPUT, removeSpaces); //Run the mainmethod, with the changed length, or unmodified if there were no previous statements
            }
        }
        /// <summary>
        /// Write snazzy text in the console that is big
        /// </summary>
        /// <param name="text">text to print</param>
        public static bool noprint = false;
        public static bool modifyLastOutput = false;
        public static void MainMethod(string userINPUT, bool removeSpaces = true)
        {
            noprint = false;

            if (removeSpaces)
            {
                userINPUT = userINPUT.Trim();
                //userINPUT = userINPUT.RemoveSpaces();
                userINPUT = userINPUT.RemoveComments();
            }
            #region uservariables
            if (userINPUT.BeginsWith("#define")) //Are we defining a variable?
            {
                Variables.DefineVariable(userINPUT); //Define the veriable with the users input
                return;
            }
            if (userINPUT.BeginsWith("#defunc"))
            {
                Variables.DefineFunction(userINPUT); //Define a function with the new input
                return;
            }
            if (userINPUT.BeginsWith("#delfunc"))
            {
                userINPUT = userINPUT.RemoveSpaces();
                Variables.DeleteFunction(userINPUT.Substring(8)); //Delete the function
                return;
            }
            if (userINPUT.BeginsWith("#del"))
            {
                userINPUT = userINPUT.RemoveSpaces();
                Variables.DeleteVariable(userINPUT.Substring(4)); //Delete the variable
                return;
            }

            if (userINPUT.BeginsWith("copy"))
            {
                userINPUT = userINPUT.Substring(4); //Remove the copy from the string
                string[] filepaths = userINPUT.Split(" : ");
                for (int i = 0; i < 2; ++i)
                {
                    filepaths[i] = filepaths[i].RemoveSpaces(); //We can't remove spaces earlier, because we need to check for spaces around the ':'
                }

                if (!Directory.Exists(filepaths[1])) //Does destination directory not exist?
                {
                    try
                    {
                        Directory.CreateDirectory(filepaths[1]);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        expectingError = true;
                        throw new Exception("Access denied. Try running as administrator");
                    }
                }
                //Now we know that a destination filepath exists

                if (Directory.Exists(filepaths[0])) //Does origin path exist?
                {

                }
                else if (File.Exists(filepaths[0])) //Is it a singular file?
                {
                    if (!filepaths[1].EndsWith("\\")) //Is a filename specified?
                    {
                        File.Copy(filepaths[0], filepaths[1]); //Just do a normal copy
                    }
                    else //Filename not specified? Use original filename
                    {
                        File.Copy(filepaths[0], filepaths[1] + filepaths[0].Split('\\').Last());
                    }
                }
            }

            userINPUT = RemoveX(userINPUT);
            if (userINPUT.ToUpper() == "CLOSE_CONDITION_PROCESSED") //Boolean condition has already been processed. Exit the loop
            {
                return;
            }

            //Display/show variables
            if (userINPUT == "showfunc") //Show the user defined functions
            {
                CustomConsole.PrintColour(Variables.UserVariables().AsString());
                return;
            }
            if (userINPUT == "ipconfig") //Show the user defined functions
            {
                Networking.PrintIPData();
                return;
            }
            if (userINPUT == "dv") //Display all the user defined variables
            {
                foreach (var i in File.ReadAllLines(DataFilePath)) //Iterate through all the lines
                {
                    string copy = Regex.Replace(i, ",", " = "); //Replace the csv style commas with more user friendly " = "
                    CustomConsole.PrintColour(copy); //Print the new variable
                }
                return;
            }
            if (userINPUT == "dtv") //Display all the user defined temp variables
            {
                foreach (var i in Variables.tempVariables) //Iterate through all the variables
                {
                    CustomConsole.PrintColour(string.Format("{0} = {1}", i.Key, i.Value)); //Print them to the screen
                }
                return;
            }
            #endregion


            if (userINPUT == "exit" || userINPUT == "quit") //close the app?
            {
                Environment.Exit(0);
            }
            if (userINPUT.BeginsWith("alg")) //Generate algebra
            {
                userINPUT = userINPUT.Substring(4);
                userINPUT = userINPUT.Substring(0,userINPUT.Length-1); //Remove brackets
                int[] nums;
                try
                {
                    nums = userINPUT.Split(',').Select(s => int.Parse(s)).ToArray();
                }
                catch
                {
                    expectingError = true;
                    throw new Exception("Arguments must be numbers");
                }
                if (nums.Length != 3)
                {
                    expectingError = true;
                    throw new Exception("Expected 2 commas, recieved " + nums.Where(c=>c==',').Count());
                }
                Algebra.FactoriseCrissCross(nums[0], nums[1], nums[2]);
                return;
            }
            if (userINPUT.BeginsWith("mkdir"))
            {
                userINPUT = userINPUT.Substring(5); //Remove the mkdir from the string
                if (!Directory.Exists(userINPUT))
                {
                    try
                    {
                        Directory.CreateDirectory(userINPUT);
                        CustomConsole.PrintColour("Successfully created directory: " + userINPUT);
                        return;
                    }
                    catch(UnauthorizedAccessException e)
                    {
                        expectingError = true;
                        throw new Exception("Access denied. Try running as administrator");
                    }
                }
                else
                {
                    expectingError = true;
                    throw new Exception("Directory already exists");
                }
            }

            if (userINPUT.BeginsWith("ping")) //User wants to ping a server?
            {
                Networking.PingHost(userINPUT.Substring(4));
                Networking.PingHost(userINPUT.Substring(4)); //Ping twice
                return;
            }


            if (userINPUT.BeginsWith("loop")) //User wants to do a loop?
            {
                DoLoopFunc(userINPUT); //Do the loop, then exit
                return;
            }
            #region showdecimals
            //Show value as decimal
            if (userINPUT.BeginsWith("doub")) //User wants to show value as double
            {
                CustomConsole.PrintDouble(double.Parse(userINPUT.Substring(4)).AsBinary()); //Print the new double value
                double userDoubleInput = double.Parse(userINPUT.Substring(4));
                CustomConsole.PrintColour("Closest conversion: " + userDoubleInput.ToString()); //Show the conversion
                string bitconv = Convert.ToString(BitConverter.DoubleToInt64Bits(userDoubleInput), 2);
                lastInput = Convert.ToUInt64(bitconv, 2); //Convert the double into a ulong to change last input
                return;
            }
            if (userINPUT.BeginsWith("float")) //User wants to show value as float
            {
                CustomConsole.PrintFloat(float.Parse(userINPUT.Substring(5)).AsBinary()); //Print the new float value
                float userFloatInput = float.Parse(userINPUT.Substring(5));
                CustomConsole.PrintColour("Closest conversion: " + userFloatInput.ToString()); //Show the conversion
                string bitconv = Convert.ToString(BitConverter.SingleToInt32Bits(userFloatInput), 2);
                lastInput = Convert.ToUInt64(bitconv, 2); //Convert the double into a ulong to change last input
                return;
            }

            //Show previous bitset as decimal
            if (userINPUT == "adv") //Show previous bitset as a double
            {
                CustomConsole.DoubleRePrint(BitConverter.Int64BitsToDouble((long)lastInput).AsBinary());
                CustomConsole.PrintColour("Double is: " + BitConverter.Int64BitsToDouble((long)lastInput));
                lastWasDouble = true; //Process next inputs math as double
                return;
            }
            if (userINPUT == "afv") //Show previous bitset as a float value
            {
                int lastinput__int;
                try
                {
                    lastinput__int = int.Parse(lastInput.ToString());
                }
                catch
                {
                    expectingError = true;
                    throw new Exception("Value too large");
                }
                float int32bits = BitConverter.ToSingle(BitConverter.GetBytes(lastinput__int));
                CustomConsole.FloatRePrint(int32bits.AsBinary());
                CustomConsole.PrintColour("Float is: " + BitConverter.Int32BitsToSingle(int.Parse(lastInput.ToString())));
                return;
            }
            #endregion

            if (userINPUT == "dt") //User wants to see the current date/time
            {
                CustomConsole.PrintColour(DateTime.Now.ToString()); //Print the date/time
                return;
            }
            if (Variables.ModifyVariables(userINPUT)) //Is the user modifying variables that already exist
                                            //This function automatically deals with it, so we just need to finish
            {
                return;
            }
            if (userINPUT.BeginsWith("var"))
            {
                Variables.DefineTempVariable(userINPUT.Substring(3)); //Define a new temporary variable with the users input
                return;
            }
            if (userINPUT.BeginsWith("np")) //Does the user not want to print the binary value of the final result?
            {
                noprint = true; //Tell the binary printer NOT to print
                userINPUT = userINPUT.Substring(2); //Remove the string "np" from the userINPUT
            }
            if (userINPUT.BeginsWith("hrgb")) //Does the user want to convert a hex value into rgb
                                                                              //Returns rgb(255,255,255) for #ffffff
            {
                userINPUT = userINPUT.Substring(4); //Remove the "hrgb" from the calculation
                HEX_to_RGB(userINPUT); //Convert the hex value into rgb and print the result
                return;
            }
            if (userINPUT.BeginsWith("asci")) //Does the user want to draw ascii art
            {
                try
                {
                    userINPUT = Bitmath.RemoveBrackets(userINPUT);
                }
                catch
                {

                }
                CustomConsole.WriteAscii(userINPUT.Substring(4, userINPUT.Length - 4)); //Remove the final bracket from the asci statement
                return;
            }
            if (userINPUT.BeginsWith("basci")) //Does the user want to draw snazzy binary ascii art
            {
                try
                {
                    userINPUT = Bitmath.RemoveBrackets(userINPUT);
                }
                catch
                {

                }

                BinaryNumASCI.PrintConverted(userINPUT.Substring(5, userINPUT.Length - 5)); //Remove the final bracket from the asci statement
                return;
            }
            if (userINPUT.ToLower() == "help") //Pretty damn well self explanatory
            {
                PrintHelp();
                return;
            }
            if (userINPUT.ToLower() == "cv") //Delete all variables
            {
                File.WriteAllText(DataFilePath, "");
                return;
            }

            if (userINPUT.BeginsWith("avg")) //User wants to get average number of a set
            {
                CustomConsole.PrintColour("Average is: " + Bitmath.Average(userINPUT));
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
            if (userINPUT.BeginsWith("ati")) //Weird math. Description in the PrintAti() function
            {
                PrintAti(userINPUT);
                return;
            }
            if (userINPUT.BeginsWith("nslookup")) //User wants to find IP of a server
            {
                string addresses = "";
                foreach (var address in Dns.GetHostEntry(userINPUT.Substring(8)).AddressList)
                {
                    addresses += address;
                    addresses += ',';
                }
                addresses = addresses.Substring(0,addresses.Length-1); //Remove the final comma
                CustomConsole.NetworkingPrint("Server IP: " + addresses);
                return;
            }
            if (userINPUT.BeginsWith("factors("))
            {
                userINPUT = userINPUT.Substring(8);
                userINPUT = userINPUT.Substring(0, userINPUT.Length - 1);

                string toprint = "";
                int n;
                try
                {
                    n = int.Parse(userINPUT);
                }
                catch
                {
                    expectingError = true;
                    throw new Exception(string.Format("'{0}' is not a number", userINPUT));
                }
                foreach (var factor in Algebra.GetFactors(n))
                {
                    toprint += factor;
                    toprint += ',';
                }
                toprint = toprint.Substring(0, toprint.Length - 1);

                CustomConsole.PrintColour(toprint);
                return;
            }
            if (userINPUT.BeginsWith("f")) //Flipping the binary result?
            {
                flipped = true; //Change the flipped value to true so that when we print binary later, we know what to do
                userINPUT = userINPUT.Substring(1); //Remove the 'f' from the string
                CustomConsole.PrintColour("Printing flipped..."); //Inform the user that the binary outcome is being flipped
            }

            if (userINPUT.BeginsWith("sqrt"))
            {
                userINPUT = userINPUT.Substring(5);
                userINPUT = userINPUT.Substring(0, userINPUT.Length - 1);
                userINPUT = Bitmath.DoubleCalculate(userINPUT);
                double num = 0;
                try
                {
                    num = double.Parse(userINPUT);
                }
                catch
                {
                    expectingError = true;
                    throw new Exception("Input must be a valid decimal");
                }
                MainMethod("doum " + Math.Sqrt(num)); //Run again to process the 'doum'
                return;
            }

            if (userINPUT.BeginsWith("i")) //User wants to show binary value as 32i (32 bit uint)
            {
                is32bit = true; //Tell the binary printer to print only 32 bits
                userINPUT = userINPUT.Substring(1); //Remove the i from the thing being printed
            }
            else if (userINPUT.BeginsWith("s")) //User wants to show binary value as 16s (16 bit ushort)
            {
                is16bit = true; //Tell the binary printer to only print 16 bits
                userINPUT = userINPUT.Substring(1); //Remove the s from the thing being printed
            }
            else if (userINPUT.BeginsWith("b")) //User wants to show binary value as 8b (8 bit byte)
            {
                is8bit = true; //Tell the binary printer to only print 8 bits
                userINPUT = userINPUT.Substring(1); //Remove the b from the thing being printed
            }

            else if (userINPUT.BeginsWith("h")) //User wants to show as hexadecimal?
            {
                userINPUT = userINPUT.Substring(1); //Remove the h from the start
                CustomConsole.PrintHex(ulong.Parse(userINPUT).ToString("X2").ToLower()); //Print the hex value of the users input
                return;
            }
            if (userINPUT.BeginsWith("#_")) //Converting hex value into ulong?
            {
                userINPUT = userINPUT.Substring(2);
                CustomConsole.PrintColour(ulong.Parse(userINPUT, System.Globalization.NumberStyles.HexNumber).ToString());
                //Convert from hex to ulong, and print the result
                return;
            }
            if (userINPUT.BeginsWith("doum")) //Check if the user wants to do doum math
            {
                userINPUT = userINPUT.Substring(4);
                if (userINPUT[0].IsOperator()) //Doing operation on v?
                {
                    if (lastWasDouble)
                    {
                        string last = BitConverter.Int64BitsToDouble((long)lastInput).ExactDecimal();

                        userINPUT = userINPUT.Insert(0,last);
                    }
                    else
                    {
                        userINPUT = userINPUT.Insert(0, lastInput.ToString());
                    }
                    modifyLastOutput = true;
                }
                userINPUT = Bitmath.DoubleCalculate(userINPUT); //Calculate the result

                //Print the value as a double
                if (!noprint)
                {
                    if (modifyLastOutput) //Modifying last output? We will need to print on last line
                    {
                        CustomConsole.DoubleRePrint(double.Parse(userINPUT).AsBinary());
                    }
                    else
                    {
                        CustomConsole.PrintDouble(double.Parse(userINPUT).AsBinary());
                    }
                }
                CustomConsole.PrintColour("Closest conversion: " + double.Parse(userINPUT).ToString());
                double d = double.Parse(userINPUT);

                string bitconv = Convert.ToString(BitConverter.DoubleToInt64Bits(d), 2);
                lastInput = Convert.ToUInt64(bitconv, 2);
                lastWasDouble = true;
                modifyLastOutput = false;
                return;
            }

            char chosenType = 'u';
            if (is32bit)
            {
                chosenType = 'i';
                CustomConsole.PrintColour("Printing as 32 bit int...");
            }
            else if (is16bit)
            {
                chosenType = 's';
                CustomConsole.PrintColour("Printing as 16 bit short...");
            }
            else if (is8bit)
            {
                chosenType = 'b';
                CustomConsole.PrintColour("Printing as 8 bit byte...");
            } //Change the chosen output type depending on the specified bit length
              //Default is 64 bit ulong

            userINPUT = Bitmath.RemoveBrackets(userINPUT);
            string booleans = CheckForBooleans(userINPUT); //Remove boolean conditions (==,!=,<,>)
            if (booleans == "true" || booleans == "false")
            {
                CustomConsole.PrintColour(booleans); //Only do bool math, don't process following characters
                return;
            }
            if (Bitmath.BitCalculate(userINPUT) != userINPUT)
            {
                userINPUT = Bitmath.BitCalculate(userINPUT);
                if (!noprint)
                {
                    CustomConsole.PrintColour(userINPUT); //Only print out the answer if there has been a calculation
                }
            }
            if (!ulong.TryParse(userINPUT, out ulong input))
            {
                expectingError = true;
                throw new Exception(string.Format("'{0}' is not a number", userINPUT));
            }

            if (modifyLastOutput) //Are we modifying what was previously printed?
            {
                CustomConsole.RePrint(input.AsBinary(flipped), true);
                CustomConsole.PrintColour("-->"+userINPUT);
                lastInput = input; //Assign lastinput
                modifyLastOutput = false;
                return;
            }

            if (!noprint) //Are we printing the binary values?
            {
                if (is32bit) //Print as 32 bit
                {
                    CustomConsole.PrintColour(input.AsBinary(flipped).Substring(67), true);
                }
                else if (is16bit) //Print as 16 bit
                {
                    CustomConsole.PrintColour(input.AsBinary(flipped).Substring(101),true);
                }
                else if (is8bit) //Print as 8 bit
                {
                    CustomConsole.PrintColour(input.AsBinary(flipped).Substring(118), true);
                }
                else //Print as ulong
                {
                    if (userINPUT == "") //Process blank
                    {
                        return;
                    }
                    CustomConsole.PrintColour(input.AsBinary(flipped), true);
                }
            }
            else
            {
                CustomConsole.PrintColour(input.ToString()); //Instead of printing a binary result, print out the result as plain text
            }
            lastWasDouble = false;
            lastInput = input; //Assign lastinput
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
        /// This is not designed to be a USEFUL tool, more just something to play around with
        static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(); //Static string of all the letters in the alphabet
        private static void PrintAti(string userINPUT)
        {
            userINPUT = userINPUT.Substring(3);
            ulong total = 1;
            foreach (var c in userINPUT.ToUpper())
            {
                if (c == '(' || c== ')')
                {
                    continue; //Ignore all brackets
                }
                if (alphabet.ToList().IndexOf(c) == -1) //Not in alphabet? Continue to next element
                {
                    Colorful.Console.WriteLine(string.Format("Character: {0} not in alphabet. Disregarded in calculation", c)
                        , Color.FromArgb(255, 10, 10));
                    continue;
                }
                total *= (ulong)(alphabet.ToList().IndexOf(c) + 1); //Multiply equals by the next element
            }
            Colorful.Console.WriteAscii(string.Format("{0:n0}", total)); //Print result as snazzy asci text
        }
        public static string RemoveX(string userINPUT)
        {
            userINPUT = userINPUT.AddSpaces(); //Add spaces around the operators to help with variable replacement

            userINPUT = Variables.ReplaceTempVariables(userINPUT);
            
            userINPUT = RemoveHex(userINPUT);
            userINPUT = RemoveBinary(userINPUT);
            userINPUT = Variables.ReplaceVariables(userINPUT);

            userINPUT = Bitmath.RemoveTrig(userINPUT);
            userINPUT = Bitmath.RemoveLog(userINPUT);

            userINPUT = RemoveBooleanStatements(userINPUT);

            return userINPUT;
        }
        /// <summary>
        /// Runs a custom system function ran()
        /// Generates a random number between the first num, and the second num
        /// Looks for the position of ran( in the string, then looks for ) and splits the two numbers
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <returns></returns>
        public static string RemoveRandom(string sINPUT)
        {
            sINPUT = sINPUT.RemoveSpaces();
            if (sINPUT.Contains("ran(")) //Is the ran function in the users input
            {
                string buffer = "";
                for (int i = 0; i < sINPUT.Length; i++) //Iterate through the string
                {
                    char c = sINPUT[i]; //Using for instead of foreach to access the 'i' variable
                    buffer += c;
                    if (buffer.Contains("ran(")) //The moment the buffer contains ran
                                                 //i is the index of the (
                    {
                        int nextBracket = sINPUT.ClosingBracket(i + 1); //Find index of the closing brackets
                        string constraints = sINPUT.Substring(i + 1, nextBracket - i - 1); //Remove ran( and the closing brackets
                        //We are now left with two numbers and a comma

                        Random random = new Random();
                        string[] nums = constraints.Split(',');
                        nums[0] = Bitmath.RemoveBrackets(Bitmath.BitCalculate(nums[0]));
                        nums[1] = Bitmath.RemoveBrackets(Bitmath.BitCalculate(nums[1])); //User may have variables or functions declared here. Check for these

                        int nextRan = random.Next(int.Parse(nums[0]), 1 + int.Parse(nums[1])); //+1 because max val is INCLUSIVE
                        CustomConsole.PrintColour("Random number is: " + nextRan.ToString());

                        //Rebuild the string
                        string before = sINPUT.Substring(0, i - 3); //Get the prev string value up until the ran(
                        string replace = nextRan.ToString(); //Replace the ran(x,y) with the random value
                        string after = sINPUT.Substring(nextBracket + 1); //Get the index of the trailing bracket
                        return RemoveRandom(before + replace + after);
                    }
                }
            }
            return sINPUT;
        }


        /// <summary>
        /// Removes boolean questions
        /// 4==4?3:2
        /// Replaces this entire statement with the new result
        /// REQUIRES FIXING
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <returns></returns>
        private static string RemoveBooleanStatements(string sINPUT)
        {
            if (sINPUT.Contains('?') && !sINPUT.Contains(':')) //Only one condition and no else
            {
                for (int i = 0; i < sINPUT.Length; ++i)
                {
                    char c = sINPUT[i];
                    if (c == '?')
                    {
                        int lastOperatorIDX = sINPUT.LastNegOperatorIDX(i - 1);
                        string after = sINPUT.Substring(sINPUT.NextOperatorIDX_NoBrackets(i));
                        string inputCondition = sINPUT.Substring(lastOperatorIDX + 1, i - lastOperatorIDX - 1);

                        string conditionResult;
                        conditionResult = RemoveHex(Bitmath.RemoveBrackets(Bitmath.BitCalculate(CheckForBooleans(inputCondition))));
                        
                        CustomConsole.PrintColour(String.Format("{0} is {1}", inputCondition, conditionResult));
                        if (conditionResult == "true")
                        {
                            string result = sINPUT.Substring(sINPUT.IndexOf('?') + 1, sINPUT.NextOperatorIDX_NoBrackets(i) - sINPUT.IndexOf('?') - 1); //Space between the ? and the : is the final condition
                            string before = sINPUT.Substring(0, sINPUT.LastOperatorIDX(sINPUT.IndexOfCondition() - 1) + 1);
                            if (sINPUT[sINPUT.NextOperatorIDX(0)].IsConditionary()) //First operator is the boolean statement?
                            {
                                before = "";
                            }
                            DoMainMethod(before + result + after, false);
                        }
                        else
                        {
                            string result = "0";
                            string before = sINPUT.Substring(0, sINPUT.LastOperatorIDX(sINPUT.IndexOfCondition() - 1) + 1);
                            if (sINPUT[sINPUT.NextOperatorIDX(0)].IsConditionary()) //First operator is the boolean statement?
                            {
                                before = "";
                            }
                            DoMainMethod(before + result + after, false);
                        }
                        return "CLOSE_CONDITION_PROCESSED";
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
                        int lastOperatorIDX = sINPUT.LastNegOperatorIDX(i - 1);
                        int nextColonIDX = sINPUT.NextColonIDX(i + 1);
                        string after = sINPUT.Substring(sINPUT.NextOperatorIDX_NoBrackets(nextColonIDX));

                        string conditionResult = "";
                        string inputCondition = sINPUT.Substring(lastOperatorIDX + 1, i - lastOperatorIDX - 1);
                        conditionResult = RemoveHex(Bitmath.RemoveBrackets(Bitmath.BitCalculate(CheckForBooleans(inputCondition))));
                       
                        CustomConsole.PrintColour(String.Format("{0} is {1}", inputCondition, conditionResult));


                        if (conditionResult == "true")
                        {
                            string result = sINPUT.Substring(sINPUT.IndexOf('?')+1, sINPUT.IndexOf(':')-sINPUT.IndexOf('?')-1); //Space between the ? and the : is the final condition
                            int lastoperatoridx = sINPUT.LastOperatorIDX(sINPUT.IndexOfCondition() - 1);
                            string before = sINPUT.Substring(0, lastOperatorIDX + 1);
                            if (sINPUT[sINPUT.NextOperatorIDX(0)].IsConditionary()) //First operator is the boolean statement?
                            {
                                before = "";
                            }
                            DoMainMethod(before+result+after, false);
                        }
                        else
                        {
                            string result = sINPUT.Substring(sINPUT.IndexOf(':') + 1, sINPUT.NextOperatorIDX_NoBrackets(sINPUT.IndexOf(':')) - sINPUT.IndexOf(':') - 1); //Space between the ? and the : is the final condition
                            string before = sINPUT.Substring(0, sINPUT.LastOperatorIDX(sINPUT.IndexOfCondition() - 1) + 1);
                            if (sINPUT[sINPUT.NextOperatorIDX(0)].IsConditionary()) //First operator is the boolean statement?
                            {
                                before = "";
                            }
                            DoMainMethod(before + result + after, false);
                        }
                        return "CLOSE_CONDITION_PROCESSED";
                    }
                }
            }
            return sINPUT;
        }

        /// <summary>
        /// Replaces binary as defined by b_010101  with its corresponding int value
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string RemoveBinary(string input)
        {
            char prev = ' ';
            if (input.Contains("b_")) //Is there binary to remove?
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = (char)input[i];
                    if (prev == 'b' && c == '_') //Are we at the start of the binary??
                    {
                        string fixedval = input.Substring(0, i - 1); //The statement that came previously to the binary num

                        int nextOperaror = input.NextOperatorIDX_NoLetter(i + 1); //Find the index of the next operator so that we know when the binary statement ends
                        string binarystring = input.Substring(i + 1, nextOperaror - i - 1).RemoveSpaces();
                        string binNum = Convert.ToUInt64(binarystring, 2).ToString(); //Find the binary num, convert it to a uint64

                        string afterThat = input.Substring(nextOperaror, input.Length - nextOperaror); //Find the trailing characters
                        CustomConsole.PrintColour(string.Format("{0} --> {1}", input, fixedval + binNum + afterThat)); //Show the user what has been replaced
                        return RemoveBinary(fixedval + binNum + afterThat); //There may be more binary to find, so look for that
                    }
                    prev = c;
                }
            }
            return input;
        }
        /// <summary>
        /// Removes hex from the users input
        /// Comes in the form #ffffff
        /// So far this works, so eh, not gonna document it for now
        /// Will proabbly not be involved in the new features being created
        /// 
        /// 'PROBABLY'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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
                        string fixedval = input.Substring(0, i);
                        int nextOperaror = input.NextOperatorIDX_NoLetter(i + 1);
                        string hexNum = ulong.Parse(input.Substring(i + 1, nextOperaror - i - 1), System.Globalization.NumberStyles.HexNumber).ToString();
                        string afterThat = input.Substring(nextOperaror, input.Length - nextOperaror);

                        CustomConsole.PrintColour("#"+input.Substring(i + 1, nextOperaror - i - 1) + " --> " + hexNum);
                        return RemoveHex(fixedval + hexNum + afterThat);
                    }
                }
            }
            return input;
        }
        /// <summary>
        /// Converts hex to rgb
        /// You type in hrgb #ffffff
        /// An voila, it converts it into rgb(255,255,255)
        /// NOT to be used in conjunction with other things
        /// </summary>
        /// <param name="hexVal"></param>
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
            result = result.Substring(0, result.Length - 1);
            result += ");";
            CustomConsole.PrintColour(result);
        }
        public static void DoLoopFunc(string loop)
        {
            loop = loop.Substring(5); //Remove the loop( from the start
            string[] args = loop.Split(')')[0].Split(',');
            int bottomRange;
            int topRange;

            try
            {
                bottomRange = int.Parse(Bitmath.BitCalculate(args[0]));
                topRange    = int.Parse(Bitmath.BitCalculate(args[1]));
            }
            catch
            {
                expectingError = true;
                throw new Exception(string.Format("Loop did not contain valid range", loop));
            }
            loop = loop.AddSpaces();
            loop = loop.Substring(loop.IndexOf(':')+1);
            loop = loop.AddSpaces();
            for (int i = bottomRange; i < topRange; ++i)
            {
                string currentLoop = Variables.ReplaceTempVariables(loop, "i", i.ToString());
                DoMainMethod(currentLoop);
            }
        }

        public static void DoNetworkingOperation(Networking n, string operation)
        {
            if (operation.BeginsWith("send")) //User wants to send data?
            {
                string data = operation.Substring(5);
                data = data.Substring(0,data.Length-1); //Remove send( and )
                n.Send(data);
            }
        }
        private static void PrintHelp()
        {
            WriteHelp("Welcome to DevTools 2022");
            WriteHelp("Below listed are the available functions you can use");
            WriteHelp("To get data on how to use the function, just type *help-functionname*");
            Console.WriteLine();
            CustomConsole.PrintColour("loop");
            CustomConsole.PrintColour("#define");
            CustomConsole.PrintColour("#defunc");
            CustomConsole.PrintColour("#delfunc");
            CustomConsole.PrintColour("#del");
            CustomConsole.PrintColour("showfunc");
            CustomConsole.PrintColour("dv");
            CustomConsole.PrintColour("dtv");
            CustomConsole.PrintColour("exit");
            CustomConsole.PrintColour("quit");
            CustomConsole.PrintColour("ran");
            CustomConsole.PrintColour("alg");
            CustomConsole.PrintColour("factors");
            CustomConsole.PrintColour("sqrt");
            CustomConsole.PrintColour("v");
            CustomConsole.PrintColour("doub");
            CustomConsole.PrintColour("float");
            CustomConsole.PrintColour("adv");
            CustomConsole.PrintColour("afv");
            CustomConsole.PrintColour("dt");
            CustomConsole.PrintColour("var");
            CustomConsole.PrintColour("np");
            CustomConsole.PrintColour("hrgb");
            CustomConsole.PrintColour("asci");
            CustomConsole.PrintColour("basci");
            CustomConsole.PrintColour("cv");
            CustomConsole.PrintColour("avg");
            CustomConsole.PrintColour("f");
            CustomConsole.PrintColour("rf");
            CustomConsole.PrintColour("ati");
            CustomConsole.PrintColour("i");
            CustomConsole.PrintColour("s");
            CustomConsole.PrintColour("b");
            CustomConsole.PrintColour("h");
            CustomConsole.PrintColour("#_");
            CustomConsole.PrintColour("b_");
            CustomConsole.PrintColour("doum");
            CustomConsole.PrintColour("booleans");
            CustomConsole.PrintColour("bitmath");
            CustomConsole.PrintColour("trig");
            CustomConsole.PrintColour("log");
            CustomConsole.PrintColour("ipconfig");
            CustomConsole.PrintColour("open");
            CustomConsole.PrintColour("tcp_client");
            CustomConsole.PrintColour("tcp_server");
            CustomConsole.PrintColour("udp_client");
            CustomConsole.PrintColour("udp_server");
            CustomConsole.PrintColour("send");
            CustomConsole.PrintColour("nslookup");
            CustomConsole.PrintColour("ping");
            CustomConsole.PrintColour("mkdir");
            CustomConsole.PrintColour("");
            WriteHelp("You can also type in math equations using math operators *,/,+,-");
        }
        public static void WriteHelp(string s)
        {
            CustomConsole.ShowDescription(s.Insert(0,@"///") + @"\\\");
        }
        static readonly string DataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\DevTools";

        static readonly string DataFile = @"\data.txt";
        public readonly static string DataFilePath = DataDirectory + DataFile;
                      
        static readonly string VersionFile = @"\version.txt";
        public readonly static string VersionFilePath = DataDirectory + VersionFile;

        static readonly string funcsFile = @"\funcs.txt";
        public readonly static string FuncFilePath = DataDirectory + funcsFile;
        
        /// <summary>
        /// Looks for boolean statements: ==,!=,>,<. Processes their values
        /// RETURNED VALUE IS A STRING. DO NOT PROCESS AS BOOL
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CheckForBooleans(string input)
        {
            if (input.Contains('<') && !input.Contains("<<"))
            {
                var strings = input.Split('<');
                strings[0] = Bitmath.BitCalculate(Bitmath.RemoveBrackets(strings[0]));
                strings[1] = Bitmath.BitCalculate(Bitmath.RemoveBrackets(strings[1]));
                if (ulong.Parse(strings[0])<ulong.Parse(strings[1]))
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            if (input.Contains('>') && !input.Contains(">>"))
            {
                var strings = input.Split('>');
                strings[0] = Bitmath.BitCalculate(strings[0]);
                strings[1] = Bitmath.BitCalculate(strings[1]);
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
                strings[0] = Bitmath.BitCalculate(Bitmath.RemoveBrackets(strings[0]));
                strings[1] = Bitmath.BitCalculate(Bitmath.RemoveBrackets(strings[1]));
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
                strings[0] = Bitmath.BitCalculate(Bitmath.RemoveBrackets(strings[0]));
                strings[1] = Bitmath.BitCalculate(Bitmath.RemoveBrackets(strings[1]));
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
    }
}