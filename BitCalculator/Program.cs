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
        static ulong lastInput = 0ul;
        static bool defaultFlipVal = false;
        public static string lastprint;
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
                        ServerNetworking serverNetworking = new ServerNetworking(7777, NetworkingPrint);
                        ClientNetworking clientNetworking = new ClientNetworking("127.0.0.1", 7777, NetworkingPrint);
                        Thread.Sleep(1000);
                        clientNetworking.Send("Hey server wassup");
                        serverNetworking.SendToAll("Shut up dude");

                        userInput = ReadLineOrEsc();
                        ChangeUserTextColour(userInput);
                        DoMainMethod(userInput);
                    }
                }
                catch (Exception e)
                {
                    Colorful.Console.WriteLine("INVALID", Color.FromArgb(255, 10, 10));
                    Colorful.Console.WriteLine(e.Message, Color.FromArgb(255, 10, 10));
                    Colorful.Console.WriteLine(e.StackTrace, Color.FromArgb(255, 10, 10));
                }
            }
        }

        // returns null if user pressed Escape, or the contents of the line if they pressed Enter.
        private static string ReadLineOrEsc()
        {
            string retString = "";

            int curIndex = 0;
            do
            {
                ConsoleKeyInfo readKeyResult = Console.ReadKey(true);

                // handle Enter
                if (readKeyResult.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return retString;
                }

                // handle backspace
                if (readKeyResult.Key == ConsoleKey.Backspace)
                {
                    if (curIndex > 0)
                    {
                        retString = retString.Remove(retString.Length - 1);
                        Console.Write(readKeyResult.KeyChar);
                        Console.Write(' ');
                        Console.Write(readKeyResult.KeyChar);
                        curIndex--;
                        ChangeUserTextColourLive(retString);
                    }
                }
                else
                {
                    retString += readKeyResult.KeyChar;
                    Console.Write(readKeyResult.KeyChar);
                    curIndex++;
                    ChangeUserTextColourLive(retString);
                }
            }
            while (true);
        }

        private static void SetupConsole()
        {
            Colorful.Console.BackgroundColor = Color.FromArgb(0, 16, 29); //Change the background colour to the snazzy blue

            Colorful.Console.Clear();
            Colorful.Console.OutputEncoding = Encoding.Unicode;
            Colorful.Console.WriteAsciiStyled("Dev Tools 2022", new Colorful.StyleSheet(Color.FromArgb(122, 224, 255)));
            Colorful.Console.WriteLine("Type help to show all functions", Color.FromArgb(122, 224, 255));
            Colorful.Console.ForegroundColor = Color.FromArgb(10, 181, 158);
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
        public static void DoMainMethod(string userINPUT, bool removeSpaces = true)
        {
            if (userINPUT.BeginsWith("help-")) //Show help for specific function. Specified after the -
            {
                PrintDescription(userINPUT.Substring(5)); //Print the help
                return;
            }
            if (userINPUT == "UNITTEST")
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
        public static void WriteAscii(string text)
        {
            Colorful.Console.WriteAsciiStyled(text, new Colorful.StyleSheet(Color.FromArgb(122, 224, 255)));
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
                            s_topright = s_topright[0] == '-' ? s_topright : s_topright.Insert(0,"+");

                            string s_bottomleft = bottomleft.ToString();
                            string s_bottomright = bottomright.ToString();
                            s_bottomright = s_bottomright[0] == '-' ? s_bottomright : s_bottomright.Insert(0, "+");


                            PrintColour(string.Format("({0}x{1})({2}x{3})", s_topleft,s_topright, s_bottomleft, s_bottomright));
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
            return "";
        }
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
        public static bool VariableExists(string name)
        {
            foreach (var line in DefineVariableContents())
            {
                if (line.Split(',')[0] == name) //Does it exist?
                {
                    return true;
                }
            }
            foreach (var line in File.ReadAllLines(FuncFilePath))
            {
                if (line.Split('(')[0] == name) //Does it exist?
                {
                    return true;
                }
            }
            foreach (var variable in tempVariables.Keys)
            {
                if (variable == name) //Does it exist?
                {
                    return true;
                }
            }
            return false;
        }
        static bool noprint = false;
        public static void MainMethod(string userINPUT, bool removeSpaces = true)
        {
            noprint = false;

            if (removeSpaces)
            {
                userINPUT = RemoveSpaces(userINPUT);
                userINPUT = RemoveComments(userINPUT);
            }
            #region uservariables
            if (userINPUT.BeginsWith("#define")) //Are we defining a variable?
            {
                DefineVariable(userINPUT); //Define the veriable with the users input
                return;
            }
            if (userINPUT.BeginsWith("#defunc"))
            {
                DefineFunction(userINPUT); //Define a function with the new input
                return;
            }
            if (userINPUT.BeginsWith("#delfunc"))
            {
                DeleteFunction(userINPUT.Substring(8)); //Delete the function
                return;
            }
            if (userINPUT.BeginsWith("#del"))
            {
                DeleteVariable(userINPUT.Substring(4)); //Delete the variable
                return;
            }
            var resetworkings = false;
            if (userINPUT.BeginsWith("nw")) //User wants to print with no workings?
            {
                printWorkings = false; //Stop printing workings
                userINPUT = userINPUT.Substring(2); //remove the "nw" from the userinput string
                if (!resetworkings)
                {
                    resetworkings = true; //Change the workings value back to normal when we are done
                }
            }

            userINPUT = RemoveX(userINPUT);
             if (userINPUT == "CLOSE_CONDITION_PROCESSED") //Boolean condition has already been processed. Exit the loop
            {
                return;
            }

            //Display/show variables
            if (userINPUT == "showfunc") //Show the user defined functions
            {
                PrintColour(File.ReadAllText(FuncFilePath));
                return;
            }
            if (userINPUT == "ipconfig") //Show the user defined functions
            {
                PrintIPData();
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
            if (userINPUT.StartsWith("alg")) //Generate algebra
            {
                userINPUT = userINPUT.Substring(4);
                userINPUT = userINPUT.Substring(0,userINPUT.Length-1); //Remove brackets
                int[] nums = userINPUT.Split(',').Select(s=>int.Parse(s)).ToArray();
                FactoriseCrissCross(nums[0], nums[1], nums[2]);
                return;
            }

            string replaced = ReplaceTempVariables(userINPUT, 'v', lastInput.ToString()); //Define a new variable 'v' as the last result
            if (replaced != userINPUT) //Is the new value different to the old value. Used to stop infinite recursive loop
            {
                PrintColour(userINPUT + "-->" + replaced, true); //Show the user the change
                userINPUT = replaced; //Modify the user input to be the old input
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
                PrintDouble(DoubleToBin(double.Parse(userINPUT.Substring(4)))); //Print the new double value
                double userDoubleInput = double.Parse(userINPUT.Substring(4));
                PrintColour("Closest conversion: " + userDoubleInput.ToString(), true); //Show the conversion
                string bitconv = Convert.ToString(BitConverter.DoubleToInt64Bits(userDoubleInput), 2);
                lastInput = Convert.ToUInt64(bitconv, 2); //Convert the double into a ulong to change last input
                return;
            }
            if (userINPUT.BeginsWith("float")) //User wants to show value as float
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
            if (userINPUT.BeginsWith("var"))
            {
                try
                {
                    DefineTempVariable(userINPUT.Substring(3)); //Define a new temporary variable with the users input
                }
                catch
                {
                    throw new Exception("Could not define variable.\nThis may be because the variable has already been defined. Read documentation to see how to modify a variable");
                }
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
                    userINPUT = RemoveBrackets(userINPUT, 'u');
                }
                catch
                {

                }
                WriteAscii(userINPUT.Substring(4, userINPUT.Length - 4)); //Remove the final bracket from the asci statement
                return;
            }
            if (userINPUT.BeginsWith("basci")) //Does the user want to draw snazzy binary ascii art
            {
                try
                {
                    userINPUT = RemoveBrackets(userINPUT, 'u');
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
            if (userINPUT.StartsWith("pw")) //Change the default value for printing workings or not
            {
                string value = userINPUT.Substring(3); //Remove start bracket
                value = value.Substring(0,value.Length-1); //Removing ending bracket
                printWorkings = bool.Parse(value);
                return;
            }
            if (userINPUT.ToLower() == "fpw") //Change the value for printing workings of not.. Write it to a file
            {
                string value = userINPUT.Substring(4); //Remove start bracket
                value = value.Substring(0, value.Length - 1); //Removing ending bracket
                printWorkings = bool.Parse(value);
                File.WriteAllText(WorkingsFilePath, printWorkings.ToString());
                return;
            }
            if (userINPUT.ToLower() == "cv") //Delete all variables
            {
                File.WriteAllText(DataFilePath, "");
                return;
            }

            if (userINPUT.BeginsWith("avg")) //User wants to get average number of a set
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
            if (userINPUT.BeginsWith("ati")) //Weird math thingy. Description in the PrintAti() function
            {
                PrintAti(userINPUT);
                return;
            }
            if (userINPUT.BeginsWith("f")) //Flipping the binary result?
            {
                flipped = true; //Change the flipped value to true so that when we print binary later, we know what to do
                userINPUT = userINPUT.Substring(1); //Remove the 'f' from the string
                PrintColour("Printing flipped...", true); //Inform the user that the binary outcome is being flipped
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
                PrintHex(ulong.Parse(userINPUT).ToString("X2").ToLower()); //Print the hex value of the users input
                return;
            }
            if (userINPUT.BeginsWith("#_")) //Converting hex value into ulong?
            {
                userINPUT = userINPUT.Substring(2);
                PrintColour(ulong.Parse(userINPUT, System.Globalization.NumberStyles.HexNumber).ToString(), false);
                //Convert from hex to ulong, and print the result
                return;
            }
            if (userINPUT.BeginsWith("doum")) //Check if the user wants to do doum math
            {
                userINPUT = userINPUT.Substring(4);
                userINPUT = DoubleCalculate(DoubleRemoveBrackets(userINPUT)); //Calculate the result

                //Print the value as a double
                if (!noprint)
                {
                    PrintDouble(DoubleToBin(double.Parse(userINPUT)));
                }
                PrintColour("Closest conversion: " + double.Parse(userINPUT).ToString());
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
                        return;
                    }
                    PrintColour(UlongToBin(input, flipped), false, true);
                }
            }
            else
            {
                PrintColour(input.ToString());
            }
            lastInput = input; //Assign lastinput
            if (resetworkings) //Are we resetting the modified printworkings value
            {
                printWorkings = !printWorkings; //Reset static variable
            }
        }

        private static void PrintIPData()
        {
            Console.Write("Local IP: ");
            NetworkingPrint(GetLocalIPAddress());

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in interfaces)
            {
                Console.Write("Name: ");
                NetworkingPrint(adapter.Name);
                Console.WriteLine(adapter.Description);
                NetworkingPrint(String.Empty.PadLeft(adapter.Description.Length, '='));
                NetworkingPrint("  Interface type .......................... : "+ adapter.NetworkInterfaceType);
                NetworkingPrint("  Operational status ...................... : "+adapter.OperationalStatus);
                string versions = "";

                // Create a display string for the supported IP versions.
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    versions = "IPv4";
                }
                if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                {
                    if (versions.Length > 0)
                    {
                        versions += " ";
                    }
                    versions += "IPv6";
                }
                NetworkingPrint("  IP version .............................. : " +versions);
                Console.WriteLine();
            }
        }
        public static bool NetworkingPrint(string toprint)
        {
            Colorful.Console.WriteLine(toprint, Color.FromArgb(184, 186, 255));
            return false;
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
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
        /// <summary>
        /// Function searches for // to lead a comment and \\ to exit one
        /// It prints these to the console and removes them from the input string
        /// 
        /// It searches for the first versions of // and \\
        /// Removes them from the string, then recurses to see if there is a second set
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <returns></returns>
        private static string RemoveComments(string sINPUT)
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
        public static string RemoveX(string userINPUT)
        {
            userINPUT = ReplaceTempVariables(userINPUT);
            userINPUT = RemoveHex(userINPUT);
            userINPUT = RemoveBinary(userINPUT);
            userINPUT = ReplaceVariables(userINPUT);
            userINPUT = RemoveTrig(userINPUT);
            userINPUT = RemoveLog(userINPUT);

            userINPUT = RemoveBooleanStatements(userINPUT);
            if (userINPUT == "CLOSE_CONDITION_PROCESSED")
            {
                return userINPUT;
            }
            return userINPUT;
        }
        public static string RemoveLog(string userINPUT)
        {
            List<int> logidxs = userINPUT.AllIndexs("log"); //Find the positions of all the log statements
            var idx = userINPUT.IndexOf("log");
            if (idx == -1)
            {
                return userINPUT;
            }
            int openingBracketIDX = NextBracket(userINPUT, idx);

            var logbase = userINPUT.Substring(idx + 3, openingBracketIDX - idx - 3); //+3 is for the length of log
            if (logbase == "")
            {
                logbase = "10"; //Default base is 10
            }

            var lognum = userINPUT.Substring(openingBracketIDX + 1, ClosingBracket(userINPUT, openingBracketIDX + 1) - openingBracketIDX - 1);

            string before = userINPUT.Substring(0, idx); //Get the string that comes before this, up until idx
            string after = userINPUT.Substring(ClosingBracket(userINPUT, openingBracketIDX + 1)+1); //End at the closing bracket
            string replace = Math.Log(double.Parse(lognum), double.Parse(logbase)).ToString();
            userINPUT = before + replace + after; //Modify the string

            userINPUT = RemoveLog(userINPUT);

            if (userINPUT.StartsWith("np")) //User doesn't want to print binary of the result
            {
                userINPUT = userINPUT.Substring(2);
                noprint = true;
            }
            if (!userINPUT.StartsWith("doum"))
            {
                userINPUT = userINPUT.Insert(0, "doum");
            }

            return userINPUT;
        }
        /// <summary>
        /// Descriptions are marked with ///
        /// This function prints the descriptions to the console
        /// 
        /// Mark with ** where you want things to be recognised as normal commands
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <returns></returns>
        enum PrintType
        {
            Comment,
            Command,
        }
        private static string ShowDescription(string sINPUT)
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
                            PrintColour(c.s, false, false, false);
                            break;
                    }
                }
                Console.WriteLine();
            }
            return sINPUT;
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
        /// Runs a custom system function ran()
        /// Generates a random number between the first num, and the second num
        /// Looks for the position of ran( in the string, then looks for ) and splits the two numbers
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <returns></returns>
        private static string RemoveRandom(string sINPUT)
        {
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
                        int nextBracket = ClosingBracket(sINPUT, i + 1); //Find index of the closing brackets
                        string constraints = sINPUT.Substring(i + 1, nextBracket - i - 1); //Remove ran( and the closing brackets
                        //We are now left with two numbers and a comma

                        Random random = new Random();
                        string[] nums = constraints.Split(',');
                        nums[0] = RemoveBrackets(nums[0],'u');
                        nums[1] = RemoveBrackets(nums[1],'u'); //User may have variables or functions declared here. Check for these

                        int nextRan = random.Next(int.Parse(nums[0]), 1 + int.Parse(nums[1])); //+1 because max val is INCLUSIVE
                        PrintColour("Random number is: " + nextRan.ToString(), true);

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

        #region printDecimals
        /// <summary>
        /// Prints a double decimal values binary output
        /// So far NO bugs exist. Will not be documented, because I have no f*cking idea how it works
        /// </summary>
        /// <param name="input"></param>
        private static void PrintDouble(string input)
        {
            var res = Regex.Replace(RemoveSpaces(input), "\n", ""); //Remove all spaces, new lines or blanks from string


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
        /// So far NO bugs exist. Will not be documented, because I have no f*cking idea how it works
        /// </summary>
        /// <param name="input"></param>
        private static void PrintFloat(string input)
        {
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
        #endregion


        /// <summary>
        /// Removes boolean questions
        /// 4==4?3:2
        /// Replaces this entire statement with the new result
        /// 
        /// THIS CODE IS FULL OF BUGS
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
                        int lastOperatorIDX = LastNegOperatorIDX(sINPUT, i - 1);
                        string after = sINPUT.Substring(NextOperatorIDX_NoBrackets(sINPUT, i));
                        string inputCondition = sINPUT.Substring(lastOperatorIDX + 1, i - lastOperatorIDX - 1);

                        string conditionResult;
                        if (printWorkings == true)
                        {
                            printWorkings = false;
                            conditionResult = RemoveHex(RemoveBrackets(BitCalculate(CheckForBooleans(inputCondition, 'u'), 'u'), 'u'));
                            printWorkings = true;
                        }
                        else
                        {
                            conditionResult = RemoveHex(RemoveBrackets(BitCalculate(CheckForBooleans(inputCondition, 'u'), 'u'), 'u'));
                        }

                        PrintColour(String.Format("{0} is {1}", inputCondition, conditionResult), true);
                        if (conditionResult == "true")
                        {
                            string result = sINPUT.Substring(sINPUT.IndexOf('?') + 1, NextOperatorIDX_NoBrackets(sINPUT, i) - sINPUT.IndexOf('?') - 1); //Space between the ? and the : is the final condition
                            string before = sINPUT.Substring(0, LastOperatorIDX(sINPUT, sINPUT.IndexOfCondition() - 1) + 1);
                            if (sINPUT[NextOperatorIDX(sINPUT, 0)].IsConditionary()) //First operator is the boolean statement?
                            {
                                before = "";
                            }
                            DoMainMethod(before + result + after, false);
                        }
                        else
                        {
                            string result = "0";
                            string before = sINPUT.Substring(0, LastOperatorIDX(sINPUT, sINPUT.IndexOfCondition() - 1) + 1);
                            if (sINPUT[NextOperatorIDX(sINPUT, 0)].IsConditionary()) //First operator is the boolean statement?
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
                        int lastOperatorIDX = LastNegOperatorIDX(sINPUT, i - 1);
                        int nextColonIDX = NextColonIDX(sINPUT, i + 1);
                        string after = sINPUT.Substring(NextOperatorIDX_NoBrackets(sINPUT, nextColonIDX));

                        string conditionResult = "";
                        string inputCondition = sINPUT.Substring(lastOperatorIDX + 1, i - lastOperatorIDX - 1);

                        if (printWorkings == true)
                        {
                            printWorkings = false;
                            conditionResult = RemoveHex(RemoveBrackets(BitCalculate(CheckForBooleans(inputCondition, 'u'), 'u'), 'u'));
                            printWorkings = true;
                        }
                        else
                        {
                            conditionResult = RemoveHex(RemoveBrackets(BitCalculate(CheckForBooleans(inputCondition, 'u'), 'u'), 'u'));
                        }
                        PrintColour(String.Format("{0} is {1}", inputCondition, conditionResult),true);


                        if (conditionResult == "true")
                        {
                            string result = sINPUT.Substring(sINPUT.IndexOf('?')+1, sINPUT.IndexOf(':')-sINPUT.IndexOf('?')-1); //Space between the ? and the : is the final condition
                            int lastoperatoridx = LastOperatorIDX(sINPUT, sINPUT.IndexOfCondition() - 1);
                            string before = sINPUT.Substring(0, lastOperatorIDX + 1);
                            if (sINPUT[NextOperatorIDX(sINPUT, 0)].IsConditionary()) //First operator is the boolean statement?
                            {
                                before = "";
                            }
                            DoMainMethod(before+result+after, false);
                        }
                        else
                        {
                            string result = sINPUT.Substring(sINPUT.IndexOf(':') + 1, NextOperatorIDX_NoBrackets(sINPUT, sINPUT.IndexOf(':')) - sINPUT.IndexOf(':') - 1); //Space between the ? and the : is the final condition
                            string before = sINPUT.Substring(0, LastOperatorIDX(sINPUT, sINPUT.IndexOfCondition() - 1) + 1);
                            if (sINPUT[NextOperatorIDX(sINPUT, 0)].IsConditionary()) //First operator is the boolean statement?
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

        #region angleconversions
        public static double DegreeToRadian(double angle)
        {
            return angle * (Math.PI / 180.0);
        }
        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
        #endregion

        /// <summary>
        /// Removes trig statesments from the users input
        /// 
        /// IS BUGGY, REQUIRES FIX
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <returns></returns>
        private static string RemoveTrig(string sINPUT)
        {
            if (sINPUT.Contains("sin(") ||
                sINPUT.Contains("arcsin(") ||
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
                        return CalcTrig(sINPUT, i, MathAngleType.ArcSin);
                    }
                    if (buffer.Contains("arccos("))
                    {
                        return CalcTrig(sINPUT, i, MathAngleType.ArcCos);
                    }
                    if (buffer.Contains("arctan("))
                    {
                        return CalcTrig(sINPUT, i, MathAngleType.ArcTan);
                    }
                    if (buffer.Contains("sin("))
                    {
                        return CalcTrig(sINPUT, i, MathAngleType.Sin);
                    }
                    if (buffer.Contains("cos("))
                    {
                        return CalcTrig(sINPUT, i, MathAngleType.Cos);
                    }
                    if (buffer.Contains("tan("))
                    {
                        return CalcTrig(sINPUT, i, MathAngleType.Tan);
                    }
                }
            }
            return sINPUT;
        }
        enum MathAngleType
        {
            Sin,
            Cos,
            Tan,
            ArcSin,
            ArcCos,
            ArcTan,
        }
        private static string CalcTrig(string sINPUT, int stringIDX, MathAngleType mathAngleType)
        {
            string fixedval = sINPUT.Substring(0, stringIDX - 3); //Find the math that happens before it
            int nextOperaror = ClosingBracket(sINPUT, stringIDX + 1); //The next operator
            //BUG IF IT INCLUDES E-...d
            string result = DoubleRemoveBrackets(sINPUT.Substring(stringIDX + 1, nextOperaror - stringIDX - 1)); //Find the number between the brackets
            double calcNum = DegreeToRadian(double.Parse(result)); //Convert it from degrees to radians
            switch (mathAngleType)
            {
                case MathAngleType.Sin:
                    calcNum = Math.Sin(calcNum);
                    break;
                case MathAngleType.Cos:
                    calcNum = Math.Cos(calcNum);
                    break;
                case MathAngleType.Tan:
                    calcNum = Math.Tan(calcNum);
                    break;
                case MathAngleType.ArcSin:
                    calcNum = Math.Asin(calcNum);
                    break;
                case MathAngleType.ArcCos:
                    calcNum = Math.Acos(calcNum);
                    break;
                case MathAngleType.ArcTan:
                    calcNum = Math.Atan(calcNum);
                    break;
            }
            string afterThat = sINPUT.Substring(nextOperaror + 1, sINPUT.Length - nextOperaror - 1);
            PrintColour(string.Format("{0}({1}) = {2}",mathAngleType.ToString() , result, calcNum), true);
            string return_result = fixedval + calcNum + afterThat;

            if (return_result.StartsWith("arc")) //Was this an arc operation?
            {
                return_result = return_result.Substring(3); //Remove it
            }
            if (return_result.StartsWith("nw"))
            {
                printWorkings = false;
                return_result = return_result.Substring(2);
            }
            if (return_result.StartsWith("np"))
            {
                noprint = true;
                return_result = return_result.Substring(2);
            }
            if (return_result.Length <= 3 || return_result.Substring(0,4) != "doum")
            {
                return_result = return_result.Insert(0,"doum");
            }
            return RemoveTrig(return_result);
        }

        /// <summary>
        /// Finds the index of the closing bracket in a string
        /// NO BUGS HERE
        /// </summary>
        /// <param name="sINPUT"></param>
        /// <param name="curridx">where to search from in the string</param>
        /// <returns></returns>
        private static int ClosingBracket(string sINPUT, int curridx)
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
        /// <summary>
        /// Deletes the variable as the name specifies
        /// </summary>
        /// <param name="input"></param>
        private static void DeleteVariable(string input)
        {
            List<string> variables = File.ReadAllLines(DataFilePath).ToList(); //Find a list of all the variables
            var copy = new List<string>();
            foreach (var v in variables)
            {
                copy.Add(v);
            } //Copy the list so that we can iterate through this list while removing items from the other
            foreach (var s in variables)
            {
                var v = s.Split(',')[0];
                if (v == input) //Is this the variable we are deleting?
                {
                    copy.Remove(s); //Remove it from the list
                }
            }
            foreach (var v in copy)
            {
                PrintColour(v, true); //Print out the rest of the variables defined
            }
            File.WriteAllLines(DataFilePath, copy); //Write the new variable set to the file
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

                        int nextOperaror = NextOperatorIDX_NoLetter(input, i + 1); //Find the index of the next operator so that we know when the binary statement ends
                        string binNum = Convert.ToUInt64(input.Substring(i + 1, nextOperaror - i - 1), 2).ToString(); //Find the binary num, convert it to a uint64

                        string afterThat = input.Substring(nextOperaror, input.Length - nextOperaror); //Find the trailing characters
                        PrintColour(string.Format("{0} --> {1}", input, fixedval + binNum + afterThat), true); //Show the user what has been replaced
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
                        int nextOperaror = NextOperatorIDX_NoLetter(input, i + 1);
                        string hexNum = ulong.Parse(input.Substring(i + 1, nextOperaror - i - 1), System.Globalization.NumberStyles.HexNumber).ToString();
                        string afterThat = input.Substring(nextOperaror, input.Length - nextOperaror);
                        if (printWorkings)
                        {
                            PrintColour(input.Substring(i + 1, nextOperaror - i - 1) + " --> " + hexNum);
                        }
                        return RemoveHex(fixedval + hexNum + afterThat);
                    }
                }
            }
            return input;
        }
        /// <summary>
        /// Prints Hex in a snazzy colour
        /// </summary>
        /// <param name="v"></param>
        private static void PrintHex(string v)
        {
            Colorful.Console.WriteLine(v.Insert(0, "#"), Color.FromArgb(234, 255, 0));
        }
        /// <summary>
        /// Converts hex to rgb
        /// You type in hrgb #ffffff
        /// An voila, it converts it into rgb(255,255,255)
        /// NOT to be used in conjunction with other things
        /// 
        /// So far this works, so eh, cant be bothered documenting it
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
            PrintColour(result, false);
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
        /// <param name="v"></param>
        /// <param name="workings"></param>
        /// <param name="isBin"></param>
        public static void PrintColour(string v, bool workings = false, bool isBin = false, bool writeline = true)
        {
            //Checking for functions
            #region functionCheck
            var AllFunctions = File.ReadAllLines(FuncFilePath);
            List<FuncLocation> funclocations = new List<FuncLocation>();
            foreach (var s in File.ReadAllLines(FuncFilePath))
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
                if (v.Contains(name)) //Name is in the users input?
                {
                    funclocations.Add(new FuncLocation(v.IndexOf(name), ClosingBracket(v, v.IndexOf(name)+name.Length+1), name));
                }
            }

            bool atsystem = false;
            List<FuncLocation> sysfunclocations = new List<FuncLocation>();
            foreach (var s in File.ReadAllLines(FuncFilePath))
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
                if (v.Contains(name) && name.Length >= 2) //Name is in the users input?
                {
                    sysfunclocations.Add(new FuncLocation(v.IndexOf(name), v.IndexOf(name) + name.Length, name));
                }
            }
            #endregion
            funclocations = funclocations.OrderBy(f => f.start).ToList();
            sysfunclocations = sysfunclocations.OrderBy(f => f.start).ToList();
            //Order the functions so that the ones that appear first are at the start of the list

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
                    }
                } //Binary font
            }
            else
            {
                bool isyellow = false;
                bool printingFunction = false;
                bool printingsysFunction = false;
                FuncLocation current = new FuncLocation(int.MaxValue, int.MaxValue, "");
                FuncLocation currentsys = new FuncLocation(int.MaxValue, int.MaxValue, "");
                for (int i = 0; i < v.Length; i++)
                {
                    if (currentsys.end == i)
                    {
                        printingsysFunction = false;
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

                    char c = v[i];

                    if (printingFunction) //Are we printing out a function?
                    {
                        if (i < current.start + current.name.Length) //Are we in the name of the function?
                        {
                            Colorful.Console.Write(c, Color.FromArgb(168, 0, 149));
                        }
                        else if (i == current.start + current.name.Length && c== '(') //Opening bracket?
                        {
                            Colorful.Console.Write(c, Color.FromArgb(255,255,255));
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
                    else
                    {
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
            lastprint = v;
        }
        public static void ChangeUserTextColour(string userinput)
        {
            var x = Console.CursorLeft;
            var y = Console.CursorTop;
            y--; //Get the row above us
            Console.SetCursorPosition(x,y);
            Console.WriteLine(); //Clear previous line
            Console.SetCursorPosition(x, y);

            Colorful.Console.Write("-->", Color.FromArgb(255, 181, 158)); //Re-print the header

            PrintColour(userinput);
        }
        public static void ChangeUserTextColourLive(string userinput)
        {
            var x = Console.CursorLeft;
            var y = Console.CursorTop;
            x-= userinput.Length; //Get the start of the row
            Console.SetCursorPosition(x, y);

            PrintColour(userinput, false, false, false);
        }
        public static bool printWorkings = true;
        public static void PrintError(string errorMessage)
        {
            Colorful.Console.WriteLine(errorMessage, Color.Red);
        }
        public static bool IsOperator(char c)
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
        /*
         * Calculates <<'s given a specific string
         */
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
                            strings[0] = (byte.MaxValue / 2 + 1).ToString();
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
        /// <summary>
        /// Like remove math, except exclusively for bitshifts
        /// </summary>
        /// <param name="input"></param>
        /// <param name="chosenType"></param>
        /// <returns></returns>
        public static string RemoveBitShift(string input, char chosenType)
        {
            string buffer = "";
            if (input.Contains("<<") || input.Contains(">>")) //Is there a bitwise operator?
            {
                for (int i = 0; i < input.Length; i++) //Iterate through the string
                {
                    buffer += input[i];
                    if (buffer.Contains("<<") || buffer.Contains(">>")) //Did we just get a <<?
                    {
                        buffer = ""; //Reset the bugger
                        int lastOperatorIDX = LastOperatorIDX(input, i - 2);
                        if ((lastOperatorIDX != 0 || IsOperator(input[0])) && i >= 2)
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
            loop = loop.Substring(tocalc.Length + 1);
            for (int i = 0; i < timesAround; ++i)
            {
                string currentLoop = ReplaceTempVariables(loop, 'i', i.ToString());
                DoMainMethod(currentLoop);
            }
        }

        /// <summary>
        /// VERY BUGGY. WILL BE FIXED LATER
        /// </summary>
        /// <param name="input"></param>
        /// <param name="variableName"></param>
        /// <param name="variableValue"></param>
        /// <returns></returns>
        private static string ReplaceTempVariables(string input, char variableName, string variableValue)
        {
            string result = "";
            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];
                if (c == variableName)
                {
                    if ((input.Length - 1 == i || IsOperator(input[i + 1])) && (i == 0 || IsOperator(input[i - 1])))
                    {
                        result += variableValue;
                    }
                    else if (i == 2 && (input.StartsWith("nw") || input.StartsWith("np"))) //Is there a nw or a np command?
                    {
                        //Replace the variable
                        result += variableValue;
                    }
                    else if (i == 4 && input.StartsWith("nwnp")) //nw and np?
                    {
                        //replace the variable
                        result += variableValue;
                    }
                    else
                    {
                        result += c;
                    }
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }
        /// <summary>
        /// VERY BUGGY. WILL BE FIXED LATER
        /// </summary>
        /// <param name="input"></param>
        /// <param name="variableName"></param>
        /// <param name="variableValue"></param>
        /// <returns></returns>
        private static string ReplaceTempVariables(string input, string variableName, string variableValue)
        {
            string result = "";
            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];
                string currentVar = "";
                result += c;
                if (result.Length >= variableName.Length && !IsOperator(input[i]) && !char.IsDigit(input[i]))
                {
                    currentVar = result.Substring(i - variableName.Length + 1);
                }
                if (currentVar == variableName) //Is this the current variable we are working on
                {
                    if (input.Length == result.Length //Are we at the end of the line?
                    || (IsOperator(input[i + 1]) //At the end of the variable. i.e. there isn't a letter or number after the variable, but an operator
                    && (i == variableName.Length - 1 || IsOperator(input[i - variableName.Length]))))

                    {
                        result = result.Substring(0, result.Length - variableName.Length) + input.Substring(i + 1, input.Length - 1 - i);
                        result = result.Insert(i - variableName.Length + 1, "(" + variableValue + ")");
                        PrintColour(variableName + " --> " + variableValue, true);
                        return ReplaceTempVariables(result, variableName, variableValue);
                    }
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
        public static Dictionary<string, string> tempVariables = new Dictionary<string, string>();
        public static void DefineTempVariable(string variable)
        {
            string[] strings = variable.SplitAtFirst('=');
            int equalsIDX = strings[0].Length - 1;
            string value = strings[1];
            string variableName = variable.Substring(0, equalsIDX+1);
            if (variableName.Any(c => !char.IsLetter(c) && c != ' '))
            {
                PrintError("Invalid variable name");
                return;
            }
            if (VariableExists(variableName))
            {
                PrintError("Variable is already defined");
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
                    input[i] == '=' ||
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

        private static int NextOperatorIDX_NoBrackets(string input, int currIDX)
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

        private static int LastAsterixIDX(string input, int currIDX)
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

        private static int LastNegOperatorIDX(string input, int currIDX)
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
            WriteHelp("Welcome to DevTools 2022");
            WriteHelp("Below listed are the available functions you can use");
            WriteHelp("To get data on how to use the function, just type *help-functionname*");
            Console.WriteLine();
            PrintColour("loop");
            PrintColour("#define");
            PrintColour("#defunc");
            PrintColour("#delfunc");
            PrintColour("#del");
            PrintColour("nw");
            PrintColour("showfunc");
            PrintColour("dv");
            PrintColour("dtv");
            PrintColour("exit");
            PrintColour("quit");
            PrintColour("ran");
            PrintColour("alg");
            PrintColour("v");
            PrintColour("doub");
            PrintColour("float");
            PrintColour("adv");
            PrintColour("afv");
            PrintColour("dt");
            PrintColour("var");
            PrintColour("np");
            PrintColour("hrgb");
            PrintColour("asci");
            PrintColour("basci");
            PrintColour("pw");
            PrintColour("fpw");
            PrintColour("cv");
            PrintColour("avg");
            PrintColour("r");
            PrintColour("rf");
            PrintColour("ati");
            PrintColour("i");
            PrintColour("s");
            PrintColour("b");
            PrintColour("h");
            PrintColour("#_");
            PrintColour("b_");
            PrintColour("doum");
            PrintColour("booleans");
            PrintColour("bitmath");
            PrintColour("trig");
            PrintColour("log");
            PrintColour("ipconfig");
            PrintColour("");
            WriteHelp("You can also type in math equations using math operators *,/,+,-");
        }
        public static void WriteHelp(string s)
        {
            ShowDescription(s.Insert(0,@"///") + @"\\\");
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
            string[] strings = variable.SplitAtFirst('=');
            int equalsIDX = strings[0].Length-1;
            string value = strings[1];
            string variableName = variable.Substring(7,equalsIDX-6);
            if (variableName.Any(c=>!char.IsLetter(c) && c != ' ') )
            {
                PrintColour("Invalid variable name", false);
                return;
            }
            if (VariableExists(variableName))
            {
                PrintError("Variable is already defined");
                return;
            }
            List<string> contents = DefineVariableContents(variableName, value);
            if (contents.All(s=>s.SplitAtFirst(',')[0] != variableName))
            {
                contents.Add(string.Format("{0},{1}", variableName, value));
            }
            File.WriteAllLines(DataFilePath, contents);
        }
        public static List<string> DefineVariableContents(string variableName = "", string value = "")
        {
            List<string> contents = File.ReadAllLines(DataFilePath).ToList();
            for (int i = 0; i < contents.Count; i++)
            {
                string s = contents[i];
                string[] args = s.SplitAtFirst(',');
                if (args[0] == variableName)
                {
                    var ss = s.SplitAtFirst(',');
                    ss[1] = value;
                    contents[i] = ss[0] + ',' + ss[1];
                    File.WriteAllLines(DataFilePath, contents);
                }
            }

            return contents;
        }
        static void DefineFunction(string function)
        {
            function = function.Substring("#defunc".Length);

            var prev = File.ReadAllLines(FuncFilePath).ToList();
            var name = function.Substring(0, function.IndexOf('('));
            if (VariableExists(name))
            {
                PrintError("Variable is already defined");
                return;
            }
            List<string> toremove = new List<string>();
            foreach (var s in prev)
            {
                if (s == "SYSTEM FUNCTIONS:")
                {
                    continue;
                }
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

            string result = function + "\n" + File.ReadAllText(FuncFilePath);

            File.WriteAllText(FuncFilePath, result);
        }
        static void DeleteFunction(string name)
        {
            var prev = File.ReadAllLines(FuncFilePath).ToList();
            List<string> toremove = new List<string>();
            foreach (var s in prev)
            {
                if (s == "SYSTEM FUNCTIONS:")
                {
                    break;
                }
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
            var nextidx = 0;
            for (int i = 0; i < prev.Count; i++)
            {
                string? s = prev[i];
                if (s == "SYSTEM FUNCTIONS:")
                {
                    nextidx = i + 1;
                    break;
                }
                var bracketidx = s.IndexOf('(');
                var substr = s.Substring(0, bracketidx);
                if (substr == name) //Already defined function
                {
                    ShowDescription(s);
                }
            }
            for (int i = nextidx; i < prev.Count; i++)
            {
                string? s = prev[i];

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
            try
            {
                string i = input;
                foreach (var s in DefineVariableContents())
                {
                    if (!s.Contains(','))
                    {
                        File.WriteAllText(DataFilePath, "");
                        PrintColour("All variables cleared because of invalid input. DO NOT EDIT THE VARIABLES FILE", false);
                        return "";
                    }

                    var ss = s.SplitAtFirst(',');
                    i = Regex.Replace(i, ss[0], "(" + ss[1] + ")");
                }
                foreach (var s in File.ReadAllLines(FuncFilePath))
                {
                    if (s == "")
                    {
                        continue;
                    }
                    if (s == "SYSTEM FUNCTIONS:")
                    {
                        break;
                    }
                    if (!s.Contains('('))
                    {
                        File.WriteAllText(FuncFilePath, Help.DEFAULTFUNCS);
                        PrintColour("All FUNCTIONS cleared because of invalid input. DO NOT EDIT THE functions FILE", false);
                        return "";
                    }
                    var name = s.Split('(')[0];
                    if (i.Contains(name))
                    {
                        string replacestring = s;
                        int closingBracketidx = ClosingBracket(replacestring, name.Length + 1);
                        replacestring = replacestring.Substring(closingBracketidx + 1);

                        int valuesstartidx = i.IndexOf(name) + name.Length+1;
                        string[] values = i.Substring(valuesstartidx, ClosingBracket(i, valuesstartidx) - valuesstartidx).Split(',');
                        string[] names = s.Substring(name.Length + 1, ClosingBracket(s, name.Length + 1) - name.Length - 1).Split(',');
                        Dictionary<string, int> variableValues = new Dictionary<string, int>();
                        //Iterate through here and add the variable values to the variable names
                        //swap out the variable values for the variable names in the function stored file
                        //Replace the function text with the text found in the file

                        for (int idx = 0; idx < values.Length; ++idx)
                        {
                            replacestring = ReplaceTempVariables(replacestring, names[idx], values[idx]);
                        }
                        if (replacestring.Contains("///"))
                        {
                            replacestring = replacestring.Substring(0, replacestring.IndexOf("///"));
                        }
                        string before = i.Substring(0, i.IndexOf(name));
                        string after = i.Substring(ClosingBracket(i, i.IndexOf(name) + name.Length + 1)+1);
                        i = before + replacestring + after;
                        return ReplaceVariables(i);
                    }
                }
                if (i != input)
                {
                    PrintColour(i, true);
                }
                i = RemoveRandom(i);
                return i;
            }
            catch
            {
                throw new Exception("Ya might wanna check ur usage of that function");
            }
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