using System;
using System.IO;
using Picture_Converter.Properties;

namespace Picture_Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Settings.Default.Path);

                Convert.FullDirList(dir);
                //if (args[0] == "Convert")
                //{
                //    Convert.ConvertPictures();
                //}
            }
            catch (Exception e)
            {
                Logger.Log.Ging(e.Message);
            }
        }
    }
}
