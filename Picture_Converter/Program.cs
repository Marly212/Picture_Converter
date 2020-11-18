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
                if (File.Exists("log"))
                {
                    File.Delete("log");
                }

                //DirectoryInfo dir = new DirectoryInfo(Settings.Default.Path);

                //Convert.FullDirList(dir);
                if (args[0] == "Convert")
                {
                    Logger.Log.Ging("Beginne Converten");
                    DirectoryInfo dir = new DirectoryInfo(Settings.Default.Path);
                    Convert.FullDirList(dir);
                }
                

            }
            catch (Exception e)
            {
                Logger.Log.Ging(e.Message);
            }
        }
    }
}
