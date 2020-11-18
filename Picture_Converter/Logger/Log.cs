using System;
using System.IO;

namespace Picture_Converter.Logger
{
    class Log
    {
        public static void Ging(string me)
        {
            File.AppendAllLines(Environment.CurrentDirectory+"log", new string[] { me });
        }
    }
}