using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DRenderingSystem
{
    static class Extension_Methods
    {
        public static int Round(this double Input)
        {
            return Convert.ToInt32(Math.Round(Input));
        }
        public static double Round(this double Input, int decimals)
        {
            return Math.Round(Input, decimals);
        }
        public static string ToString(this double Input, int decimals, bool alwaysXDecimals = false)
        {
            char formatChar;
            if (alwaysXDecimals)
                formatChar = '0';
            else
                formatChar = '#';
            string formatStr = "{0:0." + new String(formatChar, decimals) + "}";
            return String.Format(formatStr, Input);
        }



        /*=====String to vector. Cool but useless=====
        
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
