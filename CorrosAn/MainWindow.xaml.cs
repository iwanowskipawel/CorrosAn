using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace CorrosAn
{
    public delegate double CalcSampleDelegate(string path);
    public delegate void ChangeTextDel(string text);
    public delegate void TextBox1Delegate(string textToAdd);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string sampleName = "";

        public MainWindow()
        {
            InitializeComponent();
            textBox_level.Text = "0";
        }

        private void openSample_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.ShowDialog();

            if (dlg.FileNames != null)
                GetPathAndCalc(dlg.FileNames, dlg.SafeFileNames);
        }

        private void GetPathAndCalc(string[] pathTab, string[] nameTab)
        {
            int i = 0;
            CalcSampleDelegate calcSampleDel = new CalcSampleDelegate(CalculateSample);
            foreach (string path in pathTab)
            {
                sampleName = nameTab[i++];
                IAsyncResult calcSampleRes = calcSampleDel.BeginInvoke(path, new AsyncCallback(CallBack), sampleName);
            }
        }

        public void CallBack(IAsyncResult asRes) 
        {
            AsyncResult result = (AsyncResult)asRes;
            CalcSampleDelegate calcSampleDel = (CalcSampleDelegate)result.AsyncDelegate;
            TextBox1Delegate textDel = new TextBox1Delegate(DisplayResults);
            try
            {
                double factor = calcSampleDel.EndInvoke(asRes);
                string sampleName = (string)result.AsyncState;
                textBox1.Dispatcher.Invoke(textDel, "\n" + sampleName.Remove(sampleName.LastIndexOf('.')) + "\t" + factor);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void DisplayResults(string textToAdd)
        {
            textBox1.Text += textToAdd;
        }

        public double CalculateSample(string _samplePath)
        {
            if (_samplePath == "")
                throw new Exception("Invalid path");
            BitmapImage sample = new BitmapImage(new Uri(_samplePath));

            Bitmap sampleBmp = new Bitmap(_samplePath);
            PixelElement[,] pixelTable = CopyBitmapToTable(sampleBmp);
            return CalculateTresholding(pixelTable);
        }

        private PixelElement[,] CopyBitmapToTable(Bitmap _bmp)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, _bmp.Width, _bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = _bmp.LockBits(
                rect,
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                _bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * _bmp.Height;
            byte[] rgbValues = new byte[bytes];
            PixelElement[,] elements = new PixelElement[_bmp.Width, _bmp.Height];
            
            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            int x = 0, y = 0;
            for (int counter = 0; counter < rgbValues.Length; )
            {
                if (x == _bmp.Width)
                {
                    x = 0;
                    y++;
                }
                if (y == _bmp.Height)
                    break;
                elements[x++, y] = new PixelElement(
                    rgbValues[counter++],
                    rgbValues[counter++],
                    rgbValues[counter++]);
            }

            // Unlock the bits.
            _bmp.UnlockBits(bmpData);

            return elements;
        }

        private double CalculateTresholding(PixelElement[,] pixelTable)
        {
            int WidthTol = 300, HeightTol = 600,
                areaWidth = pixelTable.GetLength(0) - WidthTol,
                areaHeight = pixelTable.GetLength(1) - HeightTol,
                radius = 5;
            double TresFactor = 1,
                Positive = 0, Negative = 0,
                IPx = 0,
                limit = 0,
                k = 1,
                sum = 0;

            int xMin,
                xMax,
                yMin,
                yMax,
                numberOfPixels = 0;

            for (int x = WidthTol; x <= areaWidth; x++)
                for (int y = HeightTol; y <= areaHeight; y++)
                {
                    IPx = pixelTable[x, y].RGB;
                    xMin = x - radius;
                    xMax = x + radius;
                    yMin = y - radius;
                    yMax = y + radius;
                    numberOfPixels = 0;
                    for (int i = xMin; i <= xMax; i++)
                        for (int j = yMin; j <= yMax; j++)
                        {
                            sum += pixelTable[i, j].RGB;
                            numberOfPixels++;
                        }
                    limit = (sum / numberOfPixels);
                    if (IPx > (limit/k))
                        Positive++;
                    else Negative++;
                }
            TresFactor = Math.Round((Positive / (Positive + Negative)), 3);
            return TresFactor;
        }

    }
}