using System;
using System.Collections.Generic;
using System.Text;

namespace DevTools
{
    internal class UnitTest
    {
        public static List<UnitTest> unitTests = new List<UnitTest>()
        {
            new UnitTest("np 4==4?5:3", "5"),
            new UnitTest("#del bob; #define bob = 2; np bob + 7", "9"),
            new UnitTest("alg(7,-11,-6)", "(1x-2)(7x+3)"),
            new UnitTest("np tan(70)", "Closest conversion: 2.74747741945462"),
            new UnitTest("arcsin(45) + arccos(45)", "Closest conversion: 1.5707963267949"),
            new UnitTest("hrgb ffffff", "rgb(255,255,255);"),
            new UnitTest("np tan(70)+sin(90)-arccos(1.002)-40+6.75532-3", "Closest conversion: -34.0505098166892"),
            new UnitTest("np b_01010101010000101001010101 + 345", "22350766"),
            new UnitTest("np doum b_01010101010000101001010101 + 345", "Closest conversion: 22350766"),
            new UnitTest("np doum #ffff + 3 - 2 + 4 - b_010101010101 + 0.0055", "Closest conversion: 64175.0055"),
            new UnitTest("np log8(9) + log89(90) + log(10) + 10", "Closest conversion: 13.0591309087963"),
        };

        public string Input;
        public string ExpectedOutput;

        public UnitTest(string input, string expectedOutput)
        {
            Input = input;
            ExpectedOutput = expectedOutput;
        }

        public bool Test()
        {
            Program.DoMainMethod(Input);

            var result = Program.lastprint;
            if (result == ExpectedOutput)
            {
                CustomConsole.PrintError("Test passed");
                return true;
            }
            else
            {
                CustomConsole.PrintError("Test failed");
                CustomConsole.PrintError(String.Format("Expected: {0}, Recieved: {1}", ExpectedOutput, result));
                return false;
            }
        }
    }
}