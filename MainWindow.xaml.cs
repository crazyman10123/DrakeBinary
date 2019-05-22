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

        public Bitmap nah = new Bitmap(Properties.Resources.drakeNah);
        public Bitmap yah = new Bitmap(Properties.Resources.drakeYeh);
        public Bitmap blank = new Bitmap(Properties.Resources.blank);

        private void ConFromText(object sender, RoutedEventArgs e)
        {
            String ToParse = inputTB.Text.ToString();
            String finalBinary = StringToBinary(ToParse);
            inputTB.Text = finalBinary;
            GenImage(finalBinary);
        }

        private void ConFromImage(object sender, RoutedEventArgs e)
        {
            String readBinary = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "png files (.png)|*.png";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                String filename = openFileDialog.FileName;
                Image image = Image.FromFile(filename);
                int numRows = image.Height / nah.Height;
                int numCols = image.Width / nah.Width;
                var imagearray = new Image[numRows * numCols];
                for(int i = 0; i < numCols; i++)
                {
                    for(int j = 0; j < numRows; j++)
                    {
                        var index = i * numCols + j;
                        imagearray[index] = new Bitmap(nah.Width, nah.Height);
                        var graphics = Graphics.FromImage(imagearray[index]);
                        graphics.DrawImage(image, new Rectangle(0, 0, nah.Width, nah.Height), new Rectangle(j * nah.Width, i * nah.Height, nah.Width, nah.Height), GraphicsUnit.Pixel);
                        graphics.Dispose();
                    }
                }
                foreach(Image img in imagearray)
                {
                    if (imgCompare((Bitmap)img, nah))
                    {
                        readBinary += "0";
                    }
                    else if (imgCompare((Bitmap)img, yah))
                    {
                        readBinary += "1";
                    }
                    img.Dispose();
                }
                inputTB.Text = BinaryToString(readBinary);
            }
        }

        public static string StringToBinary(string data)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] buf = encoding.GetBytes(data);
            string result = "";

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
            imageOne.Dispose();
            imageTwo.Dispose();
            return true;

        }

        private void GenImage(String binary)
        {
            double binLength = binary.Length;
            int finHeight, finWidth, imgWidth, imgHeight;
            finHeight = (int)Math.Round(Math.Sqrt(binLength));
            finWidth = finHeight;
            imgHeight = finHeight * nah.Height;
            imgWidth = finWidth * nah.Width;
            Bitmap newBitmap = new Bitmap(imgWidth, imgHeight);
            Image[] imagearray = new Image[finHeight * finWidth];
            int index = 0;
            foreach (char thisChar in binary)
            {
                Image curImage = nah;
                if (thisChar == '0')
                {
                    imagearray[index] = new Bitmap(nah);
                }
                else if (thisChar == '1')
                {
                    imagearray[index] = new Bitmap(yah);
                }
                else if (thisChar == ' ')
                {
                    imagearray[index] = new Bitmap(blank);
                }
                index++;
            }
            using (Graphics graphics = Graphics.FromImage(newBitmap))
            {
                graphics.Clear(System.Drawing.Color.Black);
                for (int i = 0; i < finWidth; i++)
                {
                    for (int j = 0; j < finHeight; j++)
                    {
                        var newindex = i * finWidth + j;
                        var destRectangle = new Rectangle(j*nah.Width, i*nah.Height, nah.Width, nah.Height);
                        var srcRectangle = new Rectangle(0, 0, nah.Width, nah.Height);
                        try
                        {
                            graphics.DrawImage(imagearray[newindex], destRectangle, srcRectangle, GraphicsUnit.Pixel);
                        } catch(System.ArgumentNullException)
                        {
                            graphics.DrawImage(blank, destRectangle, srcRectangle, GraphicsUnit.Pixel);
                        }
                    }
                }
                graphics.Dispose();
            }
            imagearray = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "png files (.png)|*.png";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = false;
            if (openFileDialog.ShowDialog() == true)
            {
                String filename = openFileDialog.FileName;
                newBitmap.Save(filename);
            }
            newBitmap.Dispose();
            openFileDialog = null;
        }
    }
}
