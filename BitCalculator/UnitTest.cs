using System;
using System.Collections.Generic;
using System.Text;

namespace DevTools
{
    internal class UnitTest
    {
        List<UnitTest> unitTests = new List<UnitTest>()
        {
            new UnitTest("nw np 4==4?5:3", "5"),
            new UnitTest("#del bob; #define bob = 2; nw np bob + 7", "9"),
            new UnitTest("alg(7,-11,-6)", "(1x-2)(7x+3)"),
            new UnitTest("nw np tan(70)", "Tan(70) = 2.7474774194546216"),
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
            return false;
        }
    }
}