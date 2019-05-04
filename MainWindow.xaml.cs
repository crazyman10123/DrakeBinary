using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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

        private void ConFromText(object sender, RoutedEventArgs e)
        {
            String ToParse = inputTB.Text.ToString();
            String finalBinary = StringToBinary(ToParse);
            inputTB.Text = finalBinary;
            var nah = new Bitmap(Properties.Resources.drakeNah);
            var yah = new Bitmap(Properties.Resources.drakeYeh);
            var blank = new Bitmap(Properties.Resources.blank);
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "zip files (*.zip)|*.zip";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
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
                                String fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
                                nah.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                                archive.CreateEntryFromFile(fileName, i + ".png");
                            }
                            else if (c == '1')
                            {
                                String fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
                                yah.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                                archive.CreateEntryFromFile(fileName, i + ".png");
                            }
                            else
                            {
                                String fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
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

        }

        public static string StringToBinary(string data)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] buf = encoding.GetBytes(data);

            StringBuilder binaryStringBuilder = new StringBuilder();
            foreach (byte b in buf)
            {
                binaryStringBuilder.Append(Convert.ToString(b, 2));
                binaryStringBuilder.Append(" ");
            }
            return binaryStringBuilder.ToString();
        }
    }
}
