
using System;
using System.IO;
using System.Text;
using System.Linq;
using Serilog;
using DataMinerAPI.Models;

namespace DataMinerAPI.Engine
{
    public static class Utils
	{
        public static string RemoveWhitespace(this string input)
        {
            int j = 0, inputlen = input.Length;
            char[] newarr = new char[inputlen];

            for (int i = 0; i < inputlen; ++i)
            {
                char tmp = input[i];

                if (!char.IsWhiteSpace(tmp))
                {
                    newarr[j] = tmp;
                    ++j;
                }
            }
            return new String(newarr, 0, j);
        }

    }

}