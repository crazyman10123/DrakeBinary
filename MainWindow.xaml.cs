using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace DrakeBinary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        public string tempPath = Path.GetTempPath() + "DrakeBinary";
        public Bitmap nah = new Bitmap(Properties.Resources.drakeNah);
        public Bitmap yah = new Bitmap(Properties.Resources.drakeYeh);
        public Bitmap blank = new Bitmap(Properties.Resources.blank);

        private void ConFromText(object sender, RoutedEventArgs e)
        {
            String ToParse = inputTB.Text.ToString();
            String finalBinary = StringToBinary(ToParse);
            inputTB.Text = finalBinary;
            
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "zip files (*.zip)|*.zip";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            Directory.CreateDirectory(tempPath);
            if (saveFileDialog1.ShowDialog() == true)
            {
                using (FileStream zipToOpen = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        ZipArchiveEntry readmeEntry = archive.CreateEntry("Readme.txt");
                        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                        {
                            writer.WriteLine("Text version of binary below.");
                            writer.WriteLine(finalBinary);
                        }
                        int binLength = finalBinary.Length;
                        char[] binCharArray = new char[binLength];
                        binCharArray = finalBinary.ToCharArray();
                        int i = 0;
                        foreach (char c in binCharArray)
                        {
                            if (c == '0')
                            {
                                String fileName = tempPath + "\\drakeNah.png";
                                nah.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                                archive.CreateEntryFromFile(fileName, i + ".png");
                            }
                            else if (c == '1')
                            {
                                String fileName = tempPath + "\\drakeYah.png";
                                yah.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                                archive.CreateEntryFromFile(fileName, i + ".png");
                            }
                            else
                            {
                                String fileName = tempPath + "\\blank.png";
                                blank.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                                archive.CreateEntryFromFile(fileName, i + ".png");
                            }
                            i++;
                        }
                    }
                }
            }
        }

        private void ConFromZip(object sender, RoutedEventArgs e)
        {
            string tempFolder = "";
            string readBinary = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "zip files (.zip)|*.zip";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                tempFolder = tempPath + "\\" + Guid.NewGuid().ToString();
                Directory.CreateDirectory(tempFolder);
                string extractPath = tempFolder + Path.DirectorySeparatorChar;
                using (ZipArchive archive = ZipFile.OpenRead(openFileDialog.FileName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            string destPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                            if (destPath.StartsWith(extractPath, StringComparison.Ordinal))
                                entry.ExtractToFile(destPath);
                            Image curImage = Image.FromFile(destPath);
                            if (imgCompare((Bitmap)curImage, nah))
                            {
                                readBinary += "0";
                            }
                            else if (imgCompare((Bitmap)curImage, yah))
                            {
                                readBinary += "1";
                            }
                            curImage.Dispose();
                        }
                    }
                }
            }
            inputTB.Text = BinaryToString(readBinary);
        }

        public static string StringToBinary(string data)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] buf = encoding.GetBytes(data);
            string result = "";

            StringBuilder binaryStringBuilder = new StringBuilder();

            foreach (byte value in data)
            {
                //storage for the individual byte
                string binarybyte = Convert.ToString(value, 2);
                //if the binarybyte is not 8 characters long, its not a proper result
                while (binarybyte.Length < 8)
                {
                    //prepend the value with a 0
                    binarybyte = "0" + binarybyte;
                }
                //append the binarybyte to the result
                result += binarybyte;
                result += " ";
            }
            //return the result
            return result;
        }

        public static string BinaryToString(string data)
        {
            List<Byte> byteList = new List<Byte>();
            string result = "";

            data = data.Replace(" ", String.Empty);
            Console.WriteLine(data);

            while (data.Length > 0)
            {
                var first8 = data.Substring(0, 8);
                data = data.Substring(8);
                var number = Convert.ToInt32(first8, 2);
                result += (char)number;
            }
            return result;
        }

        private bool imgCompare(Bitmap imageOne, Bitmap imageTwo)
        {

            imageOne = (Bitmap)imageOne.GetThumbnailImage(16, 16, null, IntPtr.Zero);
            imageTwo = (Bitmap)imageTwo.GetThumbnailImage(16, 16, null, IntPtr.Zero);

            if (!imageOne.Size.Equals(imageTwo.Size))
            {
                return false;
            }
            for (int x = 0; x < imageOne.Width; ++x)
            {
                for (int y = 0; y < imageOne.Height; ++y)
                {
                    if (imageOne.GetPixel(x, y) != imageTwo.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Directory.Delete(tempPath, true);
        }

    }
}
