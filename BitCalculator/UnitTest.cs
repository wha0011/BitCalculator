using System;
using System.Collections.Generic;
using System.Text;

namespace DevTools
{
    internal class UnitTest
    {
//        public static UnitTest unitTest = new UnitTest("");

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
