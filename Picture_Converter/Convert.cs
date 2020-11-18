using Picture_Converter.Properties;
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
            //Console.WriteLine("Now working in Directory {0}", dir.FullName);
            Logger.Log.Ging($"Now working in Directory {dir.FullName}");
            // list the files
            try
            {
                foreach (FileInfo f in dir.GetFiles())
                {
                    byte[] imageInBytes = File.ReadAllBytes(f.FullName.ToString());
                    var fileName = Path.GetFileNameWithoutExtension(f.FullName.ToString());
                    var extension = Path.GetExtension(f.FullName.ToString());
                    var imageFormat = GetImageFormat(imageInBytes);

                    if (imageFormat == ImageFormat2.png)
                    {
                        if (extension == ".jpg")
                        {
                            File.Move(f.FullName.ToString(), f.DirectoryName+"\\"+fileName+".png");
                        }
                        allFiles.Add(f);
                    }
                    else
                    {
                        filesToConvert.Add(f);
                        allFiles.Add(f);
                    }
                }
                ConvertToPNG(filesToConvert);
            }
            catch(Exception e)
            {
                Logger.Log.Ging($"Directory {dir.FullName} could not be Accessed");
                Logger.Log.Ging(e.Message);
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
            //var inputArray = filesToConvert.ToArray();
            string outputName;
            int currentProcessCount = 0;
            int converted = 0;
            string currentImage = "";

            try
            {
                for (int i = 0; i < filesToConvert.Count; i++)
                {
                    currentImage = filesToConvert[i].FullName;
                    var extension = filesToConvert[i].Extension;

                    if (extension == ".webp")
                    {
                        outputName = Path.GetFileNameWithoutExtension(filesToConvert[i].FullName) + ".png";
                        var outputFullName = filesToConvert[i].DirectoryName + "\\" + outputName;
                        Process dwebp = new Process();
                        dwebp.StartInfo.FileName = $"{Settings.Default.dwebp}/bin/dwebp.exe";
                        dwebp.StartInfo.Arguments = $"\"{filesToConvert[i].FullName}\" -o \"{outputFullName}\"";
                        dwebp.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        dwebp.EnableRaisingEvents = true;
                        dwebp.Exited += (s, e) =>
                        {
                            File.Delete(filesToConvert[i].FullName);
                        };
                        dwebp.Start();

                        if (currentProcessCount >= 0)
                            dwebp.WaitForExit();
                    }
                    else
                    {
                        outputName = Path.GetFileNameWithoutExtension(filesToConvert[i].FullName) + ".png";
                        var outputFullName = filesToConvert[i].DirectoryName + "\\" + outputName;
                        var oldOutputFullname = "";
                        using (var imageBitmap = new Bitmap(filesToConvert[i].FullName))
                        {
                            if (outputFullName == filesToConvert[i].FullName)
                            {
                                oldOutputFullname = outputFullName;
                                outputName = Path.GetFileNameWithoutExtension(filesToConvert[i].FullName) + "_converted" + ".png";
                                outputFullName = filesToConvert[i].DirectoryName + "\\" + outputName;
                                filesToRename.Add(outputFullName);
                                imageBitmap.Save(outputFullName, ImageFormat.Png);
                            }
                            else
                            {
                                imageBitmap.Save(outputFullName, ImageFormat.Png);
                                filesToDelete.Add(filesToConvert[i].FullName);
                            }
                        }
                    }
                    converted++;
                    Logger.Log.Ging($"Fertig mit dem Bild {currentImage}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Ging(ex.Message+$" Fehler bei Bild: {currentImage}");
            }
            Rename(filesToRename);
            Delete(filesToDelete);
            filesToConvert.Clear();
            Logger.Log.Ging($"Fertig mit dem Ordner, {converted} Bilder wurden Verarbeitet");
        }

        private static void Delete(List<string> filesToDelete)
        {
            //var inputArray = filesToDelete.ToArray();

            for (int i = 0; i < filesToDelete.Count; i++)
            {
                File.Delete(filesToDelete[i]);
            }
            filesToDelete.Clear();
        }

        private static void Rename(List<string> filesToRename)
        {
            //var inputArray = filesToRename.ToArray();

            for (int i = 0; i < filesToRename.Count; i++)
            {
                var nameToConvert = Path.GetFileNameWithoutExtension(filesToRename[i]);
                var pathToFileToConvert = filesToRename[i];

                string newName = nameToConvert.Split(new string[] { "_converted" }, StringSplitOptions.RemoveEmptyEntries)[0];

                var newPathToFileToConvert = Path.GetDirectoryName(filesToRename[i])+"\\"+newName+".png";

                File.Delete(newPathToFileToConvert);

                File.Move(pathToFileToConvert, newPathToFileToConvert);
            }
            filesToRename.Clear();
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
