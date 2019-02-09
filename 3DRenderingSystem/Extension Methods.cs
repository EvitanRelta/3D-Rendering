using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DRenderingSystem
{
    static class Extension_Methods
    {
        public static int Round(this double input)
        {
            return Convert.ToInt32(Math.Round(input));
        }
        public static double Round(this double input, int decimalPlaces)
        {
            return Math.Round(input, decimalPlaces);
        }
        public static string ToString(this double input, int decimalPlaces, bool trailingZerosAfterDecimal = false)
        {
            char formatChar;
            if (trailingZerosAfterDecimal)
                formatChar = '0';
            else
                formatChar = '#';
            string formatStr = "{0:0." + new String(formatChar, decimalPlaces) + "}";
            return String.Format(formatStr, input);
        }



        /*String to vector. Cool but useless
        
        public static Vector ToVector(this string Input)
        {
            List<double> matches = new List<double>();
            foreach (Match i in Regex.Matches(Input, "[1-9]\\d*(\\.\\d+)?,?|[0]\\.\\d+,?|0(?=\\D|$)"))
                matches.Add(Convert.ToDouble(i.Value));
            if (matches.Count == 3)
                return new Vector(matches[0], matches[1], matches[2]);
            else
                throw new ArgumentException($"Expected 3 arguments, but {matches.Count} argument(s) was given after Regex cleaning");
        }
        */
    }
}
