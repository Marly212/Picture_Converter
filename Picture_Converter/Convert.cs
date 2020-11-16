using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Picture_Converter
{
    static class Convert
    {
        static List<FileInfo> allFiles = new List<FileInfo>();  // Liste mit Pfad zu den Dateien
        static List<FileInfo> filesToConvert = new List<FileInfo>();  // Liste mit Pfad zu den Dateien die Convertiert werden müssen
        static List<DirectoryInfo> folders = new List<DirectoryInfo>(); // Liste mit Ordner/Dateien auf die es keinen Zugriff hat
        static List<string> filesToDelete = new List<string>();
        static List<string> filesToRename = new List<string>();
        public static void FullDirList(DirectoryInfo dir)
        {
            Console.WriteLine("Directory {0}", dir.FullName);
            // list the files
            try
            {
                foreach (FileInfo f in dir.GetFiles())
                {
                    byte[] imageInBytes = File.ReadAllBytes(f.FullName.ToString());
                    var imageFormat = GetImageFormat(imageInBytes);

                    if (imageFormat == ImageFormat2.png)
                    {
                        allFiles.Add(f);
                    }
                    else
                    {
                        filesToConvert.Add(f);
                        allFiles.Add(f);
                    }
                }
                Logger.Log.Ging(filesToConvert.ToString());
                ConvertToPNG(filesToConvert);
            }
            catch(Exception e)
            {
                Console.WriteLine("Directory {0}  \n could not be accessed!!!!", dir.FullName);
                Console.WriteLine(e.Message);
                return;  // Überspringt das aktuelle Verzeichnis da wir keinen Zugriff darauf haben
            }

            // process each directory
            // If I have been able to see the files in the directory I should also be able 
            // to look at its directories so I dont think I should place this in a try catch block
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                folders.Add(d);
                FullDirList(d);
            }
        }

        private static void ConvertToPNG(List<FileInfo> filesToConvert)
        {
            var inputArray = filesToConvert.ToArray();
            string outputName;
            int currentProcessCount = 0;
            

            for (int i = 0; i < inputArray.Length; i++)
            {
                var extension = inputArray[i].Extension;

                if (extension == ".webp")
                {
                    outputName = Path.GetFileNameWithoutExtension(inputArray[i].FullName) + ".png";
                    var outputFullName = inputArray[i].DirectoryName + "\\" + outputName;
                    Process dwebp = new Process();
                    dwebp.StartInfo.FileName = $"{Environment.CurrentDirectory}/dwebp.exe";
                    dwebp.StartInfo.Arguments = $"\"{inputArray[i].FullName}\" -o \"{outputFullName}\"";
                    dwebp.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    dwebp.EnableRaisingEvents = true; 
                    dwebp.Exited += (s, e) =>
                    {
                        File.Delete(inputArray[i].FullName);
                    };
                    dwebp.Start();

                    if (currentProcessCount >= 0) 
                        dwebp.WaitForExit(); 
                }
                else
                {
                    try
                    {
                        outputName = Path.GetFileNameWithoutExtension(inputArray[i].FullName) + ".png";
                        var outputFullName = inputArray[i].DirectoryName + "\\" + outputName;
                        var oldOutputFullname = "";
                        using (var imageBitmap = new Bitmap(inputArray[i].FullName))
                        {
                            if (outputFullName == inputArray[i].FullName)
                            {
                                oldOutputFullname = outputFullName;
                                outputName = Path.GetFileNameWithoutExtension(inputArray[i].FullName) + "_converted"+".png";
                                outputFullName = inputArray[i].DirectoryName + "\\" + outputName;
                                filesToRename.Add(outputFullName);
                            }
                            imageBitmap.Save(outputFullName, ImageFormat.Png);
                            filesToDelete.Add(inputArray[i].FullName);
                        }
                            
                    }
                    catch (Exception e)
                    {

                    }
                    
                }
            }
            Rename(filesToRename);
            Delete(filesToDelete);
        }

        private static void Delete(List<string> filesToDelete)
        {
            var inputArray = filesToDelete.ToArray();

            for (int i = 0; i < inputArray.Length; i++)
            {
                File.Delete(inputArray[i]);
            }
        }

        private static void Rename(List<string> filesToRename)
        {
            var inputArray = filesToRename.ToArray();

            for (int i = 0; i < inputArray.Length; i++)
            {
                var nameToConvert = Path.GetFileNameWithoutExtension(inputArray[i]);
                var pathToFileToConvert =inputArray[i];

                string newName = nameToConvert.Split(new string[] { "_converted" }, StringSplitOptions.RemoveEmptyEntries)[0];

                var newPathToFileToConvert = Path.GetDirectoryName(inputArray[i])+"\\"+newName+".png";

                System.IO.File.Move("pathToFileToConvert", "newPathToFileToConvert");
            }
        }

        public enum ImageFormat2
        {
            bmp,
            jpeg,
            gif,
            tiff,
            png,
            webp,
            unknown
        }

        public static ImageFormat2 GetImageFormat(byte[] bytes)
        {
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon
            var webp = new byte[] { 82, 73, 70, 70 }; // webp canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat2.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat2.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat2.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat2.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat2.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat2.jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat2.jpeg;

            if (webp.SequenceEqual(bytes.Take(webp.Length)))
                return ImageFormat2.webp;

            return ImageFormat2.unknown;
        }
    }
}
