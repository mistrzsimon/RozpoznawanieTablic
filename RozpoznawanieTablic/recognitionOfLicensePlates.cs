using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using Excel = Microsoft.Office.Interop.Excel;

/// <summary>
/// 
/// Naprawić algorytm 3
/// Dodać parametr do autoamtu
/// 
/// 
/// </summary>

namespace RozpoznawanieTablic
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> _mainImage;

        private Image<Gray, byte> _imgAlgorithm_1;
        private Image<Gray, byte> _imgAlgorithm_2;
        private Image<Gray, byte> _imgAlgorithm_3;

        private Image<Bgr, byte> _imgPlateRecognition_1;
        private Image<Bgr, byte> _imgPlateRecognition_2;
        private Image<Bgr, byte> _imgPlateRecognition_3;

        private List<ListElement> _segmentation_Algorithm_1;
        private List<ListElement> _segmentation_Algorithm_2;
        private List<ListElement> _segmentation_Algorithm_3;

        private string _tablePlate_1;
        private string _tablePlate_2;
        private string _tablePlate_3;

        Excel.Application xlApp = new Excel.Application();

        struct ListElement { public Image<Gray, byte> image; public int Number; }

        private List<ListElement> ListOfStructure;

        struct ListPatterns { public Image<Gray, byte> image; public string name; }

        private List<ListPatterns> ListOfPatterns;

        struct ListImage { public Image<Bgr, byte> image; public string number; public string plateName; }

        private List<ListImage> ListOfImage;

        private string[] excelWyniki = new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };

        private string[] excelAlgorithm;

        public Form1()
        {
            InitializeComponent();
            _mainImage = new Image<Bgr, byte>(@"C:\Users\Szymon\Desktop\tablice\1_2976.jpg");
            label7.Text = "Resolution read image: " + _mainImage.Width.ToString() + "x" + _mainImage.Height.ToString();
            _mainImage = _mainImage.Resize(528, 384, Inter.Linear);
            label8.Text = "Resolution after resize image: " + _mainImage.Width.ToString() + "x" + _mainImage.Height.ToString();
            imageBox1.Image = _mainImage;

            DirectoryInfo dir = new DirectoryInfo(@"C:\Users\Szymon\Desktop\tablice\wzorce\");
            FileInfo[] imageFiles = dir.GetFiles("*.jpg");
            Console.WriteLine("Found {0} *.jpg files\n", imageFiles.Length);

            ListOfPatterns = new List<ListPatterns>();
            ListPatterns pattern;

            foreach (FileInfo f in imageFiles)
            {
                pattern.image = new Image<Gray, byte>(@"C:\Users\Szymon\Desktop\tablice\wzorce\" + f.Name);
                pattern.name = f.Name;
                ListOfPatterns.Add(pattern);
                // Console.WriteLine("File name: {0}", f.Name);
            }
            Console.WriteLine("Wielkość listy ListOfPatterns " + ListOfPatterns.Count + "\n");

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog Openfile = new OpenFileDialog
                {
                    InitialDirectory = (@"C:\Users\Szymon\Desktop\tablice")
                };
                if (Openfile.ShowDialog() == DialogResult.OK)
                {
                    excelWyniki = new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
                    _mainImage = new Image<Bgr, byte>(Openfile.FileName);
                    label7.Text = "Resolution read image: " + _mainImage.Width.ToString() + "x" + _mainImage.Height.ToString();
                    //_mainImage = _mainImage.Resize(720, 576, Inter.Linear);
                    //_mainImage = _mainImage.Resize(640, 480, Inter.Linear);
                    _mainImage = _mainImage.Resize(528, 384, Inter.Linear);
                    label8.Text = "Resolution after resize image: " + _mainImage.Width.ToString() + "x" + _mainImage.Height.ToString();
                    imageBox1.Image = _mainImage;
                    excelWyniki[0] = Openfile.SafeFileName.Substring(0, Openfile.SafeFileName.Length - 4);
                    Emgu.CV.UI.ImageBox[] array = new Emgu.CV.UI.ImageBox[] { imageBox2, imageBox3, imageBox4, imageBox5, imageBox6, imageBox7, imageBox8, imageBox9, imageBox10, imageBox11, imageBox12, imageBox13, imageBox14, imageBox15, imageBox16, imageBox17, imageBox18, imageBox19, imageBox20, imageBox21, imageBox22, imageBox23, imageBox24, imageBox25 };
                    foreach (var imageBox in array)
                    {
                        imageBox.Image = null;
                        imageBox.Invalidate();
                    }
                    System.Windows.Forms.Label[] array2_plate = new System.Windows.Forms.Label[] { label12, label13, label14 };
                    foreach (var plate in array2_plate)
                    {
                        plate.Text = "Number Plate: ";
                    }
                    System.Windows.Forms.Label[] array2_resolution = new System.Windows.Forms.Label[] { label10, label15, label17 };
                    foreach (var resolution in array2_resolution)
                    {
                        resolution.Text = "Plate resolution: ";
                    }
                    System.Windows.Forms.Label[] array2_segmentaion = new System.Windows.Forms.Label[] { label9, label16, label18 };
                    foreach (var segment in array2_segmentaion)
                    {
                        segment.Text = "Number of segment: ";
                    }
                    System.Windows.Forms.CheckBox[] array_CheckedListBox = new System.Windows.Forms.CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7, checkBox8, checkBox9 };
                    foreach (var checkbox in array_CheckedListBox)
                    {
                        checkbox.Checked = true;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd : " + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void algorithm_1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n\n ----- ALGORYTM 1 ----- \n");
            //Detekcja tablicy
            _imgAlgorithm_1 = await algorithm_1_Async(_mainImage);

            //szuaknie kontur 
            _imgPlateRecognition_1 = await szukajKontur(_imgAlgorithm_1, imageBox7, imageBox8, imageBox23, label12, label10, label9);

            if (_imgPlateRecognition_1 == null)
            {
                MessageBox.Show(" Plate not found ");
                Console.WriteLine("\n Nie znaleziono tablicy rejestracyjnej");
                imageBox7.Image = null;
                showImageError(imageBox8, imageBox23, label12, label10, label9);
                Console.WriteLine("\n ----- KONIEC ALGORYTM 1 ----- \n\n");
                return;
            }

            // Segmentacja
            _segmentation_Algorithm_1 = await segmentacjaZnaków(_imgPlateRecognition_1, imageBox23, label10, label9);
            /*
            string tablica=null;
            foreach (ListElement character1 in ListOfStructure)
            {
               tablica = tablica + OCR_bleble(character1.image , _imgPlateRecognition_1, imageBox26);
               MessageBox.Show("Wymiary znaku: wysokosc" + character1.image.Height + " szerokosc " + character1.image.Width );
            }
            MessageBox.Show(tablica);*/

            _tablePlate_1 = await recognitionOfCharacters(_segmentation_Algorithm_1);
            if (_tablePlate_1 == null)
            {
                Console.WriteLine("\n Nie znaleziono znaków na tablicy");
                label12.Text = "Number Plate: ";
            }
            else
            {
                Console.WriteLine("\n Tablica rejestracyjna: " + _tablePlate_1);
                label12.Text = "Number Plate: " + _tablePlate_1;
            }
            Console.WriteLine("\n ----- KONIEC ALGORYTM 1 ----- \n\n ");
        }

        private async Task<Image<Gray, byte>> algorithm_1_Async(Image<Bgr, byte> _mainImage1)
        {

            return await Task.Run(() =>
            {
                Image<Gray, byte> _imgAlgorithm_1_Async;
                //gray scale
                _imgAlgorithm_1_Async = _mainImage1.Convert<Gray, byte>();
                //Open MorphologyEx
                Mat kernelOpen1 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(35, 3), new Point(-1, -1));
                imageBox2.Image = _imgAlgorithm_1_Async;
                _imgAlgorithm_1_Async = _imgAlgorithm_1_Async - _imgAlgorithm_1_Async.MorphologyEx(MorphOp.Open, kernelOpen1, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox3.Image = _imgAlgorithm_1_Async;
                //Threshold
                double prog = CvInvoke.Threshold(_imgAlgorithm_1_Async, _imgAlgorithm_1_Async, 0, 255, ThresholdType.Otsu);
                imageBox4.Image = _imgAlgorithm_1_Async;
                //MessageBox.Show(prog.ToString());
                //Open  and Close MorphologyEx
                Mat kernelOpen2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(4, 4 ), new Point(-1, -1));
                Mat kernelClose = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(4, 4), new Point(-1, -1));
                _imgAlgorithm_1_Async = _imgAlgorithm_1_Async.MorphologyEx(MorphOp.Open, kernelOpen2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox5.Image = _imgAlgorithm_1_Async;
                _imgAlgorithm_1_Async = _imgAlgorithm_1_Async.MorphologyEx(MorphOp.Dilate, kernelClose, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());
                imageBox6.Image = _imgAlgorithm_1_Async;
                //Return Image after detected
                return _imgAlgorithm_1_Async;
            });
        }

        private async void algorithm_2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n\n ----- ALGORYTM 2 ----- \n");
            //Detekcja tablicy
            _imgAlgorithm_2 = await algorithm_2_Async();

            // szukanie kontur
            _imgPlateRecognition_2 = await szukajKontur(_imgAlgorithm_2, imageBox14, imageBox15, imageBox24, label13, label15, label16);

            if (_imgPlateRecognition_2 == null)
            {
                MessageBox.Show(" Plate not found ");
                Console.WriteLine("\n Nie znaleziono tablicy rejestracyjnej");
                imageBox14.Image = null;
                showImageError(imageBox15, imageBox24, label13, label15, label16);
                Console.WriteLine("\n ----- KONIEC ALGORYTM 2 ----- \n\n");
                return;
            }

            // Segmentacja
            _segmentation_Algorithm_2 = await segmentacjaZnaków(_imgPlateRecognition_2, imageBox24, label15, label16);


            _tablePlate_2 = await recognitionOfCharacters(_segmentation_Algorithm_2);

            if (_tablePlate_2 == null)
            {
                Console.WriteLine("\n Nie znaleziono znaków na tablicy");
                label13.Text = "Number Plate: ";
            }
            else
            {
                Console.WriteLine("\n Tablica rejestracyjna: " + _tablePlate_2);
                label13.Text = "Number Plate: " + _tablePlate_2;
            }
            Console.WriteLine("\n ----- KONIEC ALGORYTM 2 ----- \n\n");
        }

        private async Task<Image<Gray, byte>> algorithm_2_Async()
        {
            return await Task.Run(() =>
            {
                //Bład przy zbyt jasnym zdjeciu, thershold się głubi
                Image<Gray, byte> _imgAlgorithm_2_Async;
                // gray covert
                _imgAlgorithm_2_Async = _mainImage.Convert<Gray, byte>();
                imageBox9.Image = _imgAlgorithm_2_Async;
                // smoothing blur
                _imgAlgorithm_2_Async = _imgAlgorithm_2_Async.SmoothBlur(3, 3);
                imageBox10.Image = _imgAlgorithm_2_Async;
                // vertical edge detection
                _imgAlgorithm_2_Async = _imgAlgorithm_2_Async.Sobel(1, 0, 5).Convert<Gray, byte>();
                //_imgAlgorithm_2_Async = _imgAlgorithm_2_Async.Sobel(0, 1, 3).AbsDiff(new Gray(0.0)).Convert<Gray, byte>(); ;
                imageBox11.Image = _imgAlgorithm_2_Async;

                // thershold
                double prog = CvInvoke.Threshold(_imgAlgorithm_2_Async, _imgAlgorithm_2_Async, 0, 255, ThresholdType.Otsu);
                double wartosc = _imgAlgorithm_2_Async.GetAverage().Intensity;
                Console.WriteLine(" \n Prog Otsu : " + prog.ToString() + ", średnia wartosc: " + wartosc);

                if (wartosc > 100)
                {
                    _imgAlgorithm_2_Async = _imgAlgorithm_2_Async.Not();
                    Console.WriteLine(" \n Obraz zbyt biały, zamieniam na negatyw.");
                }

                // Zamkniecie morfologiczne
                //Mat kernelClose = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(25,4), new Point(-1, -1));
                Mat kernelClose = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(26, 4), new Point(-1, -1));
                _imgAlgorithm_2_Async = _imgAlgorithm_2_Async.MorphologyEx(MorphOp.Close, kernelClose, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox12.Image = _imgAlgorithm_2_Async;

                //Mat kernelClose2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
                //_imgAlgorithm_2_Async = _imgAlgorithm_2_Async.MorphologyEx(MorphOp.Open, kernelClose2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                //imageBox13.Image = _imgAlgorithm_2_Async;
                //Return Image after detected
                return _imgAlgorithm_2_Async;
            });
        }

        private async void algorithm_3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n\n ----- ALGORYTM 3 ----- \n");

            _imgAlgorithm_3 = await algorithm_3_Async();

            _imgPlateRecognition_3 = await szukajKontur(_imgAlgorithm_3, imageBox21, imageBox22, imageBox25, label14, label17, label18);

            if (_imgPlateRecognition_3 == null)
            {
                MessageBox.Show(" Plate not found ");
                Console.WriteLine("\n Nie znaleziono tablicy rejestracyjnej");
                //imageBox15.Image = null;
                //showImageError(imageBox15, imageBox24, label13, label15, label16);
                Console.WriteLine("\n ----- KONIEC ALGORYTM 3 ----- \n\n");
                return;
            }

            // Segmentacja
            _segmentation_Algorithm_3 = await segmentacjaZnaków(_imgPlateRecognition_3, imageBox25, label17, label18);


            _tablePlate_3 = await recognitionOfCharacters(_segmentation_Algorithm_3);

            if (_tablePlate_3 == null)
            {
                Console.WriteLine("\n Nie znaleziono znaków na tablicy");
                label14.Text = "Number Plate: ";
            }
            else
            {
                Console.WriteLine("\n Tablica rejestracyjna: " + _tablePlate_3);
                label14.Text = "Number Plate: " + _tablePlate_3;
            }

            Console.WriteLine("\n ----- KONIEC ALGORYTM 3 ----- \n\n");
        }

        private async Task<Image<Gray, byte>> algorithm_3_Async()
        {
            return await Task.Run(() =>
            {
                Image<Gray, byte> _imgAlgorithm_3_Async;
                // gray covert
                _imgAlgorithm_3_Async = _mainImage.Convert<Gray, byte>();

                //Top hat
                Mat kernel1 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(90, 90), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Tophat, kernel1, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                //imageBox16.Image = _imgAlgorithm_3_Async;

                Mat kernel2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(8, 5), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Blackhat, kernel2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox16.Image = _imgAlgorithm_3_Async;

                Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(115, 3), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Close, kernel3, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox17.Image = _imgAlgorithm_3_Async;

                Mat kernel4 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 15), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Open, kernel4, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox18.Image = _imgAlgorithm_3_Async;

                Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(25,10), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Open, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox19.Image = _imgAlgorithm_3_Async;

                double prog = CvInvoke.Threshold(_imgAlgorithm_3_Async, _imgAlgorithm_3_Async, 0, 255, ThresholdType.Otsu);
                double wartosc = _imgAlgorithm_3_Async.GetAverage().Intensity;
                Console.WriteLine(" \n Prog Otsu : " + prog.ToString() + ", średnia wartosc: " + wartosc);
                imageBox20.Image = _imgAlgorithm_3_Async;

                /*
                //Top hat
                Mat kernel1 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(20, 10), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Close, kernel1, new Point(-1, -1), 1, BorderType.Default, new MCvScalar()) - _mainImage.Convert<Gray, byte>();
                imageBox16.Image = _imgAlgorithm_3_Async;

                // thershold
                double prog = CvInvoke.Threshold(_imgAlgorithm_3_Async, _imgAlgorithm_3_Async, 0, 255, ThresholdType.Otsu);
                double wartosc = _imgAlgorithm_3_Async.GetAverage().Intensity;
                //Console.WriteLine(" \n Prog Otsu : " + prog.ToString() + ", średnia wartosc: " + wartosc);
                imageBox17.Image = _imgAlgorithm_3_Async;

                // Closing
                Mat kernelClose = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(45, 1), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Close, kernelClose, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox18.Image = _imgAlgorithm_3_Async;

                //Open
                Mat kernelOpen1 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(1, 15), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Open, kernelOpen1, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox19.Image = _imgAlgorithm_3_Async;

                Image<Gray, byte> odiecie = _imgAlgorithm_3_Async;

                //Open
                Mat kernelOpen2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(1, 35), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Open, kernelOpen2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox20.Image = _imgAlgorithm_3_Async;

                //odiecie orazu
                _imgAlgorithm_3_Async = odiecie - _imgAlgorithm_3_Async;
                imageBox21.Image = _imgAlgorithm_3_Async;

                //Open
                Mat kernelOpen3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(45, 1), new Point(-1, -1));
                _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Open, kernelOpen3, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                imageBox22.Image = _imgAlgorithm_3_Async;
                */

                return _imgAlgorithm_3_Async;
            });

        }

        private async Task<Image<Bgr, byte>> szukajKontur(Image<Gray, byte> _imgAlgorithm, Emgu.CV.UI.ImageBox imageOut1, Emgu.CV.UI.ImageBox imageOut2, Emgu.CV.UI.ImageBox segmentOut, System.Windows.Forms.Label plateOut, System.Windows.Forms.Label plateResolutionOut, System.Windows.Forms.Label numberOfSegmentOut)
        {
            return await Task.Run(() =>
            {
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat hier = new Mat();
                Image<Bgr, byte> plateRecognize;
                float szerokosc, dlugosc, szerokosc_wysokosc;
                Image<Gray, byte> _imgOut = new Image<Gray, byte>(_imgAlgorithm.Width, _imgAlgorithm.Height);

                CvInvoke.FindContours(_imgAlgorithm, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                Dictionary<int, double> dict = new Dictionary<int, double>();

                if (contours.Size > 0)
                {
                    for (int i = 0; i < contours.Size; i++)
                    {

                        double area = CvInvoke.ContourArea(contours[i]);
                        Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                        //Console.WriteLine("\n Obszar ma odleglosc: " + rect.Width + ", wysokość: " + rect.Height);
                        //if (rect.Width < 300 && rect.Width > 60 && rect.Height < 100 && rect.Height > 15)
                        if (rect.Width < 277 && rect.Width > 60 && rect.Height < 73 && rect.Height > 15)
                        {
                            szerokosc = rect.Width;
                            dlugosc = rect.Height;
                            szerokosc_wysokosc = szerokosc / dlugosc;
                            if (szerokosc_wysokosc > 2.4 && szerokosc_wysokosc < 9.5)
                            {
                                //MessageBox.Show("Do doania pole o wymiarach, szerokosc: " + szerokosc + " wysokość " + dlugosc + " lewo " + rect.Left + " prawo " + rect.Right + " dól " + rect.Bottom + " góra " + rect.Top);
                                if (rect.Top > (0.3 * _mainImage.Height) && rect.Left > (0.1 * _mainImage.Width) && rect.Right < (0.9 * _mainImage.Width))
                                {

                                    Console.WriteLine("\n Dodano tablice o wymiarach, szerokość: " + szerokosc + ", wysokość: " + dlugosc + ", lewy bok: " + rect.Left + ", prawy bok: " + rect.Right + ", dolny próg: " + rect.Bottom + ", górny próg: " + rect.Top);
                                    dict.Add(i, area);
                                }

                                /* CvInvoke.Rectangle(_imgOut, rect, new MCvScalar(100, 255, 0), 3);
                                 _mainImage.ROI = Rectangle.Empty;
                                 _mainImage.ROI = rect;
                                 plateRecognize = _mainImage.CopyBlank();
                                 _mainImage.CopyTo(plateRecognize);
                                 MessageBox.Show("Dodano W/H : " + szerokosc_wysokosc);
                                 NewWindow Powiekszenie = new NewWindow(plateRecognize)
                                 {
                                     Text = "Szerokosc: " + szerokosc + " Wysokość: " + dlugosc
                                 };
                                 Powiekszenie.Show();*/
                                //MessageBox.Show("Dodałem znak o wymiarach: " + szerokosc + " " + dlugosc);
                            }
                        }
                    }
                }

                var item = dict.OrderByDescending(v => v.Value);

                if (item.Count() == 0)
                {
                    imageOut1.Image = null;
                    showImageError(imageOut2, segmentOut, plateOut, plateResolutionOut, numberOfSegmentOut);
                }

                foreach (var it in item)
                {
                    int key = int.Parse(it.Key.ToString());
                    Rectangle rect = CvInvoke.BoundingRectangle(contours[key]);
                    CvInvoke.Rectangle(_imgOut, rect, new MCvScalar(100, 255, 0), 3);
                    _mainImage.ROI = Rectangle.Empty;
                    _mainImage.ROI = rect;
                    plateRecognize = _mainImage.CopyBlank();
                    _mainImage.CopyTo(plateRecognize);
                    imageOut2.Image = plateRecognize;
                    imageOut1.Image = _imgAlgorithm + _imgOut;

                    DialogResult dialogResult = MessageBox.Show("Wymiary prostokąta: \n Szerokość = " + rect.Size.Width + " Wysokość = " + rect.Size.Height, "Czy to jest tablica ?", MessageBoxButtons.YesNoCancel);
                    if (dialogResult == DialogResult.Yes)
                    {
                        _mainImage.ROI = Rectangle.Empty;
                        _mainImage = new Image<Bgr, Byte>(imageBox1.Image.Bitmap);
                        return plateRecognize;
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        imageOut2.Image = null;
                        imageOut1.Image = null;
                        showImageError(imageOut2, segmentOut, plateOut, plateResolutionOut, numberOfSegmentOut);
                        _mainImage.ROI = Rectangle.Empty;
                        _mainImage = new Image<Bgr, Byte>(imageBox1.Image.Bitmap);
                        plateRecognize = null;
                    }
                    else if (dialogResult == DialogResult.Cancel)
                    {
                        imageOut2.Image = null;
                        imageOut1.Image = null;
                        showImageError(imageOut2, segmentOut, plateOut, plateResolutionOut, numberOfSegmentOut);
                        _mainImage.ROI = Rectangle.Empty;
                        _mainImage = new Image<Bgr, Byte>(imageBox1.Image.Bitmap);
                        plateRecognize = null;
                        return null;
                    }
                }
                showImageError(imageOut2, segmentOut, plateOut, plateResolutionOut, numberOfSegmentOut);
                return null;
            });
        }

        private async Task<List<ListElement>> segmentacjaZnaków(Image<Bgr, byte> _ImagePlate, Emgu.CV.UI.ImageBox imageOut1, System.Windows.Forms.Label plateResolutionOut, System.Windows.Forms.Label numberOfSegmentOut)
        {
            return await Task.Run(() =>
            {

                Image<Gray, byte> plateRecognize;
                Image<Gray, byte> plate1;
                Image<Gray, byte> plate = _ImagePlate.Convert<Gray, byte>();
                int dlugoscTablicy = plate.Width;
                //CvInvoke.GaussianBlur(plate, plate, new Size(3, 3), 1);

                double prog = CvInvoke.Threshold(plate, plate, 0, 255, ThresholdType.Otsu);
                //imageBox16.Image = plate;
                plate = plate.Not();
                //imageBox16.Image = plate;

                plateResolutionOut.Invoke(new Action(delegate ()
                {
                    plateResolutionOut.Text = "Plate resolution, Width: " + plate.Width + " Height: " + plate.Height;
                }));
                Console.WriteLine(" \n Znaleziona tablica ma wymiary: " + plate.Width + " na " + plate.Height);

                plate1 = plate.CopyBlank();
                plate.CopyTo(plate1);

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat hier = new Mat();

                CvInvoke.FindContours(plate, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                Console.WriteLine(" \n Ilość kontur w FindContour: " + contours.Size);
                //Console.WriteLine(" Hier: channel " + hier.NumberOfChannels + " " + hier.Data );

                Image<Bgr, byte> _imgOut = new Image<Bgr, byte>(plate.Width, plate.Height);
                float szerokosc, wysokosc, szerokosc_wysokosc;
                int srednia;
                int czyZnalezionoCos = 0;
                ListOfStructure = new List<ListElement>();
                ListElement character;

                if (contours.Size > 0)
                {
                    for (int i = 0; i < contours.Size; i++)
                    {
                        Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                        CvInvoke.Rectangle(_imgOut, rect, new MCvScalar(0, 0, 255), 1);
                        plate.ROI = Rectangle.Empty;
                        plate.ROI = rect;
                        Console.WriteLine(" \n Znak ma wielkośc " + rect.Width + " wysokośc " + rect.Height);
                        /*Invoke(new Action(delegate ()
                        {
                            NewWindow Powiekszenie = new NewWindow(plate)
                            {
                                Text = "Szerokosc: " + rect.Width + " Wysokość: " + rect.Height
                            };
                            Powiekszenie.Show();
                        }));*/

                        if (rect.Width < 36 && rect.Width > 3 && rect.Height < 37 && rect.Height > 12)
                        {
                            szerokosc = rect.Width;
                            wysokosc = rect.Height;
                            szerokosc_wysokosc = szerokosc / wysokosc;

                            if (szerokosc_wysokosc > 0.18 && szerokosc_wysokosc < 1.2)
                            {
                                double wartosc = plate.GetAverage().Intensity;
                                //Console.WriteLine(" \n Średnia wartość : " + wartosc);
                                Console.WriteLine(" \n Znak ma wielkośc " + rect.Width + " wysokośc " + rect.Height + " lewy bok: " + plate.ROI.Left + " oraz  0.95 * Dlugość plate: " + 0.95 * dlugoscTablicy);

                                if (wartosc > 160 && plate.ROI.Left < 10)
                                {
                                    Console.WriteLine(" \n Prawdopodobnie znak początka, odrzucam");
                                }
                                else if ((plate.ROI.Left > (0.9 * dlugoscTablicy)) && plate.ROI.Width < 7)
                                {
                                    Console.WriteLine(" \n Prawdopodobnie znak końca, odrzucam");
                                }
                                else
                                {
                                    czyZnalezionoCos += 1;
                                    srednia = (plate.ROI.Left + plate.ROI.Right) / 2;
                                    plateRecognize = plate.CopyBlank();
                                    plate.CopyTo(plateRecognize);
                                    character.image = plateRecognize;
                                    character.Number = srednia;
                                    ListOfStructure.Add(character);
                                    imageOut1.Image = _ImagePlate + _imgOut;
                                    Console.WriteLine(" \n Dodany znak o wymiarach, szerokość: " + szerokosc + ", wysokość: " + wysokosc + " oraz położeniu: " + srednia + " lewy bok: " + plate.ROI.Left);
                                }
                            }
                        }
                    }
                    //plate.ROI = Rectangle.Empty;
                }
                else
                {
                    MessageBox.Show("Nie znalazłem kontur");
                }

                if (czyZnalezionoCos < 1)
                {
                    VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
                    Mat hier1 = new Mat();
                    Rectangle rect1 = new Rectangle();
                    CvInvoke.FindContours(plate1, contours1, hier1, RetrType.List , ChainApproxMethod.ChainApproxSimple);

                    //int[,] hierachy = CvInvoke.FindContourTree(plate1, contours1, ChainApproxMethod.ChainApproxSimple);
                    Console.WriteLine(" \n Ilość kontur w FindContourTree: " + contours1.Size);

                    for (int i = 0; i < contours1.Size; i++)
                    {
                        rect1 = CvInvoke.BoundingRectangle(contours1[i]);
                        CvInvoke.Rectangle(_imgOut, rect1, new MCvScalar(0, 0, 255), 1);
                        plate1.ROI = Rectangle.Empty;
                        plate1.ROI = rect1;
                        Console.WriteLine(" \n Znak ma wielkośc " + rect1.Width + " wysokośc " + rect1.Height + " oraz numer " + i);
                       
                        /* Invoke(new Action(delegate ()
                        {
                            NewWindow Powiekszenie = new NewWindow(plate1)
                            {
                                Text = "Szerokosc: " + rect1.Width + " Wysokość: " + rect1.Height + " Numer countury: " + i
                            };
                            Powiekszenie.Show();
                        }));*/

                        if (rect1.Width < 36 && rect1.Width > 3 && rect1.Height < 37 && rect1.Height > 12)
                        {
                            szerokosc = rect1.Width;
                            wysokosc = rect1.Height;
                            szerokosc_wysokosc = szerokosc / wysokosc;

                            if (szerokosc_wysokosc > 0.18 && szerokosc_wysokosc < 1.2)
                            {
                                double wartosc = plate1.GetAverage().Intensity;
                                //Console.WriteLine(" \n Średnia wartość : " + wartosc);
                                Console.WriteLine(" \n Znak ma wielkośc " + rect1.Width + " wysokośc " + rect1.Height + " lewy bok: " + plate1.ROI.Left + " oraz  0.95 * Dlugość plate: " + 0.95 * dlugoscTablicy);

                                if (wartosc > 160 && plate1.ROI.Left < 10)
                                {
                                    Console.WriteLine(" \n Prawdopodobnie znak początka, odrzucam");
                                }
                                else if ((plate1.ROI.Left > (0.9 * dlugoscTablicy)) && plate1.ROI.Width < 7)
                                {
                                    Console.WriteLine(" \n Prawdopodobnie znak końca, odrzucam");
                                }
                                else
                                {
                                    srednia = (plate1.ROI.Left + plate1.ROI.Right) / 2;
                                    plateRecognize = plate1.CopyBlank();
                                    plate1.CopyTo(plateRecognize);
                                    character.image = plateRecognize;
                                    character.Number = plate1.ROI.Left;
                                    ListOfStructure.Add(character);
                                    imageOut1.Image = _ImagePlate + _imgOut;
                                    Console.WriteLine(" \n Dodany znak o wymiarach, szerokość: " + szerokosc + ", wysokość: " + wysokosc + " oraz położeniu: " + srednia + " lewy bok: " + plate1.ROI.Left);
                                }
                            }
                        }
                    }
                }


                ListOfStructure.Sort((x, y) => x.Number - y.Number);

                ListElement characterpoprzedni;
                characterpoprzedni.Number = 0;
                int index = 0;
                int[] tab = new int[ListOfStructure.Count];

                foreach (ListElement character1 in ListOfStructure)
                {
                    //NewWindow Powiekszenie = new NewWindow(character1.image) {  Text = "Szerokosc: " + character1.image.Width + " Wysokość: " + character1.image.Height };
                    //Powiekszenie.Show();
                    if (character1.Number - characterpoprzedni.Number < 5) {
                        tab[index] = 1;
                    }
                    index += 1;
                    characterpoprzedni = character1;
                }

                int licznik = 0;
                foreach (int idx in tab)
                {
                    if(idx == 1)
                    {
                        ListOfStructure.RemoveAt(licznik);
                        Console.WriteLine("Odrzucielm znak");
                        licznik = licznik - 1;
                    }
                    licznik += 1;
                }

                numberOfSegmentOut.Invoke(new Action(delegate ()
                {
                    numberOfSegmentOut.Text = "Number of segment: " + ListOfStructure.Count;
                }));

                /*foreach (ListElement character1 in ListOfStructure)
                {
                    Invoke(new Action(delegate ()
                    {
                        NewWindow Powiekszenie = new NewWindow(character1.image)
                        {
                            Text = "Szerokosc: " + character1.image.Width + " Wysokość: " + character1.image.Height
                        };
                        Powiekszenie.Show();
                    }));
                }*/

                return ListOfStructure;
            });
        }

        private async Task<string> recognitionOfCharacters(List<ListElement> _segmentation_Algorithm)
        {

            return await Task.Run(() =>
            {

                Image<Gray, byte> characterOfSegment, characterOfPattern;
                int szerokoscWzorca, wysokoscWzorca;
                int ListElementCount = 0;
                int characterCount = 0;
                float resultOfMatchTemplate = 0;
                float maxValue = 0;
                float maxValue_under = 0;
                string nameOfPattern = null;
                string nameOfPattern_under = null;
                string tablica = null;

                foreach (ListElement character in ListOfStructure)
                {
                    ListElementCount += 1;
                    characterCount += 1;
                    characterOfSegment = character.image.Not();
                    maxValue = 0;
                    maxValue_under = 0;
                    nameOfPattern = null;
                    nameOfPattern_under = null;

                    foreach (ListPatterns pattern in ListOfPatterns)
                    {
                        characterOfPattern = pattern.image;
                        szerokoscWzorca = characterOfPattern.Width;
                        wysokoscWzorca = characterOfPattern.Height;

                        characterOfSegment = characterOfSegment.Resize(szerokoscWzorca, wysokoscWzorca, Inter.Linear);
                        var res = characterOfSegment.MatchTemplate(characterOfPattern, TemplateMatchingType.CcoeffNormed);
                        resultOfMatchTemplate = res.Data[0, 0, 0];

                        //Console.WriteLine("Patern name: " + pattern.name + " oraz średnia: " + resultOfMatchTemplate);

                        if (resultOfMatchTemplate > maxValue_under)
                        {
                            if (characterCount > 2 && pattern.name == "O.jpg")
                            {
                                Console.WriteLine("Pominąłem O w deugiej częsci tablicy");
                            }
                            else if (resultOfMatchTemplate > maxValue)
                            {
                                maxValue_under = maxValue;
                                nameOfPattern_under = nameOfPattern;
                                maxValue = resultOfMatchTemplate;
                                nameOfPattern = pattern.name;
                            }
                            else
                            {
                                maxValue_under = resultOfMatchTemplate;
                                nameOfPattern_under = pattern.name;
                            }
                        }
                        // Console.WriteLine("\n Wartosc result " + resultOfMatchTemplate + " dla Segmentu " + ListElementCount + " dla Wzorca: " + pattern.name);
                    }
                    Console.WriteLine("\n Wartosc maxValue_lider " + maxValue + " dla segmentu " + ListElementCount + " oraz wzorca: " + nameOfPattern.Substring(0, 1) + " oraz maxValue_vicelider " + maxValue_under + " dla Wzorca: " + nameOfPattern_under.Substring(0, 1));
                    tablica = tablica + nameOfPattern.Substring(0, 1);
                }
                return tablica;
            });
        }

        private void showImageError(Emgu.CV.UI.ImageBox imageBox, Emgu.CV.UI.ImageBox segment, System.Windows.Forms.Label plate, System.Windows.Forms.Label plateResolution, System.Windows.Forms.Label numberOfSegment)
        {
            Bitmap bm = new Bitmap(imageBox.Width, imageBox.Height - 50);
            using (Graphics g = Graphics.FromImage(bm))
            {
                using (SolidBrush myBrush = new SolidBrush(Color.Black))

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(234, 230, 202)))
                {
                    g.FillRectangle(brush, 0, 0, imageBox.Width, imageBox.Height - 50);

                    {
                        using (Font myFont = new Font("Times New Roman", 12))

                        {
                            g.DrawString(" not found ", myFont, myBrush, 10, 10);
                            imageBox.Image = new Image<Bgr, Byte>(bm);
                        }
                    }

                }
            }
            segment.Image = null;

            plate.Invoke(new Action(delegate ()
            {
                plate.Text = "Number Plate: ";
            }));

            plateResolution.Invoke(new Action(delegate ()
            {
                plateResolution.Text = "Plate resolution: ";
            }));

            numberOfSegment.Invoke(new Action(delegate ()
            {
                numberOfSegment.Text = "Number of segment: ";
            }));

        }

        private void ExcelSaveData(string[] exceltablica, int WorksheetNumber)
        {
            if (xlApp.Visible != true)
            {
                xlApp.Visible = true;
            }

            Excel.Workbook xlWb = xlApp.Workbooks.Open(@"C:\Users\Szymon\Desktop\tablice\daneWyjsciowe.xlsx");
            Excel.Worksheet xlWorksheet = xlWb.Worksheets[WorksheetNumber];
            Excel.Range MyLast = xlWorksheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);

            for (int i = 0; i < exceltablica.Length; i++)
            {
                xlWorksheet.Cells[MyLast.Row + 1, i + 1] = exceltablica[i];
            }
            xlWb.Save();
        }

        private void saveExcel_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox[] array_CheckedListBox = new System.Windows.Forms.CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7, checkBox8, checkBox9 };
            int i = 1;
            foreach (var checkbox in array_CheckedListBox)
            {
                if (checkbox.Checked == true)
                {
                    excelWyniki[i] = "1";
                    i++;
                }
                else
                {
                    excelWyniki[i] = "0";
                    i++;
                }
            }
            ExcelSaveData(excelWyniki, 1);
        }

        private void imageBox_DoubleClick(object sender, EventArgs e)
        {
            Emgu.CV.UI.ImageBox cb = (Emgu.CV.UI.ImageBox)sender;
            NewWindow Powiekszenie = new NewWindow(cb);
            Powiekszenie.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //
            //Paramtery dostosować do algorytmu i przeszukiwać
            //

            Console.WriteLine("\n\n----- AUTOMAT ALGORYTM 1 -----\n");
            excelAlgorithm = new string[] { "0", "0", "0", "0", "0", "0", "0", "0" };
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DirectoryInfo dir = null;
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                dir = new DirectoryInfo(@"" + fbd.SelectedPath);
            }
            else return;

            ListOfImage = new List<ListImage>();
            ListImage _image;
            int pFrom, pTo;
            int numberFrom, numberTo;

            foreach (FileInfo f in dir.GetFiles("*.jpg").OrderBy(fi => fi.Name))
            {
                string path = fbd.SelectedPath + "\\" + f.Name;
                _image.image = new Image<Bgr, byte>(@path).Resize(528, 384, Inter.Linear);
                pFrom = f.Name.IndexOf("_") + "_".Length;
                pTo = f.Name.LastIndexOf(".jpg");
                _image.plateName = f.Name.Substring(pFrom, pTo - pFrom);
                numberFrom = f.Name.IndexOf("") + "".Length;
                numberTo = f.Name.LastIndexOf("_");
                _image.number = f.Name.Substring(numberFrom, numberTo - numberFrom);
                ListOfImage.Add(_image);
                Console.WriteLine("File name: " + f.Name + "\t Number: " + _image.number + "\t Plate: " + _image.plateName);
            }
            Console.WriteLine("Wielkość listy ListOfImage: " + ListOfImage.Count + "\n");
            excelAlgorithm[0] = "Algorytm 1";
            excelAlgorithm[4] = ListOfImage.Count.ToString();
            excelAlgorithm[5] = fbd.SelectedPath;

            Console.WriteLine("--- START ---\n");
            //
            //UStawienie parametrów
            //
            //int[] parametr1 = new int[] { 30, 35, 40 };
            //int[] parametr2 = new int[] { 2,3,5 };
            int[] parametr1 = new int[] {3,4,5 };
            int[] parametr2 = new int[] {3,4,5 };

            for (int i = 0; i < parametr1.Length; i++)
            {
                for (int j = 0; j < parametr2.Length; j++)
                {
                    excelAlgorithm[1] = "0";
                    excelAlgorithm[2] = "0";
                    excelAlgorithm[3] = "0";
                    excelAlgorithm[6] = parametr1[i].ToString();
                    excelAlgorithm[7] = parametr2[j].ToString();

                    foreach (ListImage _imageCar in ListOfImage)
                    {
                        imageBox1.Image = _imageCar.image;
                        _mainImage = _imageCar.image;
                        label8.Text = "Resolution after resize image: " + _mainImage.Width.ToString() + "x" + _mainImage.Height.ToString();
                        _imgAlgorithm_1 = algorytm1(_imageCar.image, parametr1[i], parametr2[j]);

                        _imgPlateRecognition_1 = szukajKonturAuto(_imgAlgorithm_1, imageBox7, imageBox8, imageBox23, label12, label10, label9, 1);

                        if (_imgPlateRecognition_1 == null)
                        {
                            Console.WriteLine("\n Nie znaleziono tablicy rejestracyjnej");
                            imageBox7.Image = null;
                            showImageError(imageBox8, imageBox23, label12, label10, label9);
                        }
                        else
                        {
                            // Segmentacja
                            _segmentation_Algorithm_1 = segmentacjaZnakowAuto(_imgPlateRecognition_1, imageBox23, label10, label9);

                            _tablePlate_1 = recognitionOfCharactersAuto(_segmentation_Algorithm_1);

                            if (_tablePlate_1 == null)
                            {
                                Console.WriteLine("\n Nie znaleziono znaków na tablicy");
                                label12.Text = "Number Plate: ";
                            }
                            else
                            {
                                Console.WriteLine("\n Tablica rejestracyjna: " + _tablePlate_1);
                                label12.Text = "Number Plate: " + _tablePlate_1;
                            }

                            if (_segmentation_Algorithm_1.Count == _imageCar.plateName.Length)
                            {
                                Console.WriteLine("  Ilość znalezionych segmentów : " + _segmentation_Algorithm_1.Count + " do prawidłowej ilość: " + _imageCar.plateName.Length);
                                int count = Int32.Parse(excelAlgorithm[2]);
                                count = count + 1;
                                excelAlgorithm[2] = count.ToString();
                            }
                            else
                            {
                                Console.WriteLine("  BŁAD --- Ilość znalezionych segmentów : " + _segmentation_Algorithm_1.Count + " do prawidłowej ilość: " + _imageCar.plateName.Length);
                            }

                            if (_tablePlate_1 == _imageCar.plateName)
                            {
                                Console.WriteLine("  Rozpoznana tablica: " + _tablePlate_1 + " prawidłowa tablica: " + _imageCar.plateName);
                                int count = Int32.Parse(excelAlgorithm[3]);
                                count = count + 1;
                                excelAlgorithm[3] = count.ToString();
                            }
                            else
                            {
                                Console.WriteLine("  BŁAD --- Rozpoznana tablica: " + _tablePlate_1 + " prawidłowa tablica: " + _imageCar.plateName);
                            }
                        }
                    }

                    Console.WriteLine("\n Excel wyniki, algorytm: " + excelAlgorithm[0] + " tablica rejestracyjna: " + excelAlgorithm[1] + " segmentacja: " + excelAlgorithm[1] + " rozpoznanie: " + excelAlgorithm[2] + ". Wszystkich obrazów: " + ListOfImage.Count + " Parametr1: " + excelAlgorithm[6] + " Parametr2: " + excelAlgorithm[7]);
                    ExcelSaveData(excelAlgorithm, 2);
                }
            }
            Console.WriteLine("\n----- KONIEC AUTOMAT ALGORYTM 1 -----\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n\n----- AUTOMAT ALGORYTM 2 -----\n");
            excelAlgorithm = new string[] { "0", "0", "0", "0", "0", "0", "0", "0" };
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DirectoryInfo dir = null;
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                dir = new DirectoryInfo(@"" + fbd.SelectedPath);
            }
            else return;

            ListOfImage = new List<ListImage>();
            ListImage _image;
            int pFrom, pTo;
            int numberFrom, numberTo;

            foreach (FileInfo f in dir.GetFiles("*.jpg").OrderBy(fi => fi.Name))
            {
                string path = fbd.SelectedPath + "\\" + f.Name;
                _image.image = new Image<Bgr, byte>(@path).Resize(528, 384, Inter.Linear);
                pFrom = f.Name.IndexOf("_") + "_".Length;
                pTo = f.Name.LastIndexOf(".jpg");
                _image.plateName = f.Name.Substring(pFrom, pTo - pFrom);
                numberFrom = f.Name.IndexOf("") + "".Length;
                numberTo = f.Name.LastIndexOf("_");
                _image.number = f.Name.Substring(numberFrom, numberTo - numberFrom);
                ListOfImage.Add(_image);
                Console.WriteLine("File name: " + f.Name + "\t Number: " + _image.number + "\t Plate: " + _image.plateName);
            }
            Console.WriteLine("Wielkość listy ListOfImage: " + ListOfImage.Count + "\n");
            excelAlgorithm[0] = "Algorytm 2";
            excelAlgorithm[4] = ListOfImage.Count.ToString();
            excelAlgorithm[5] = fbd.SelectedPath;

            Console.WriteLine("--- START ---\n");
            //UStawienie parametrów
            //int[] parametr1 = new int[] { 3,5,7,9 };
            //int[] parametr2 = new int[] { 1 };
            int[] parametr1 = new int[] { 23, 26, 28, 30 };
            int[] parametr2 = new int[] { 3,4 };

            for (int i = 0; i < parametr1.Length; i++)
            {
                for (int j = 0; j < parametr2.Length; j++)
                {
                    excelAlgorithm[1] = "0";
                    excelAlgorithm[2] = "0";
                    excelAlgorithm[3] = "0";
                    excelAlgorithm[6] = parametr1[i].ToString();
                    excelAlgorithm[7] = parametr2[j].ToString();

                    foreach (ListImage _imageCar in ListOfImage)
                    {
                        imageBox1.Image = _imageCar.image;
                        _mainImage = _imageCar.image;
                        label8.Text = "Resolution after resize image: " + _mainImage.Width.ToString() + "x" + _mainImage.Height.ToString();

                        _imgAlgorithm_2 = algorytm2(_imageCar.image, parametr1[i], parametr2[j]);

                        _imgPlateRecognition_2 = szukajKonturAuto(_imgAlgorithm_2, imageBox14, imageBox15, imageBox24, label13, label15, label16, 1);

                        if (_imgPlateRecognition_2 == null)
                        {
                            Console.WriteLine("\n Nie znaleziono tablicy rejestracyjnej");
                            imageBox14.Image = null;
                            showImageError(imageBox15, imageBox24, label13, label15, label16);
                        }
                        else
                        {
                            // Segmentacja
                            _segmentation_Algorithm_2 = segmentacjaZnakowAuto(_imgPlateRecognition_2, imageBox24, label15, label16);

                            _tablePlate_2 = recognitionOfCharactersAuto(_segmentation_Algorithm_2);

                            if (_tablePlate_2 == null)
                            {
                                Console.WriteLine("\n Nie znaleziono znaków na tablicy");
                                label13.Text = "Number Plate: ";
                            }
                            else
                            {
                                Console.WriteLine("\n Tablica rejestracyjna: " + _tablePlate_2);
                                label13.Text = "Number Plate: " + _tablePlate_2;
                            }

                            if (_segmentation_Algorithm_2.Count == _imageCar.plateName.Length)
                            {
                                int count = Int32.Parse(excelAlgorithm[2]);
                                count = count + 1;
                                excelAlgorithm[2] = count.ToString();
                                Console.WriteLine("  Ilość znalezionych segmentów : " + _segmentation_Algorithm_2.Count + " do prawidłowej ilość: " + _imageCar.plateName.Length);
                            }
                            else
                            {
                                Console.WriteLine("  BŁAD --- Ilość znalezionych segmentów : " + _segmentation_Algorithm_2.Count + " do prawidłowej ilość: " + _imageCar.plateName.Length);
                            }

                            if (_tablePlate_2 == _imageCar.plateName)
                            {
                                Console.WriteLine("  Rozpoznana tablica: " + _tablePlate_2 + " prawidłowa tablica: " + _imageCar.plateName);
                                int count = Int32.Parse(excelAlgorithm[3]);
                                count = count + 1;
                                excelAlgorithm[3] = count.ToString();
                            }
                            else
                            {
                                Console.WriteLine("  BŁAD --- Rozpoznana tablica: " + _tablePlate_2 + " prawidłowa tablica: " + _imageCar.plateName);
                            }
                        }
                    }
                    Console.WriteLine("\n Excel wyniki, algorytm: " + excelAlgorithm[0] + " tablica rejestracyjna: " + excelAlgorithm[1] + " segmentacja: " + excelAlgorithm[2] + " rozpoznanie: " + excelAlgorithm[3] + ". Wszystkich obrazów: " + ListOfImage.Count);
                    ExcelSaveData(excelAlgorithm, 2);
                }
            }
            Console.WriteLine("\n----- KONIEC AUTOMAT ALGORYTM 2 -----\n");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n\n----- AUTOMAT ALGORYTM 3 -----\n");
            excelAlgorithm = new string[] { "0", "0", "0", "0", "0", "0", "0", "0" };
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DirectoryInfo dir = null;
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                dir = new DirectoryInfo(@"" + fbd.SelectedPath);
            }
            else return;

            ListOfImage = new List<ListImage>();
            ListImage _image;
            int pFrom, pTo;
            int numberFrom, numberTo;

            foreach (FileInfo f in dir.GetFiles("*.jpg").OrderBy(fi => fi.Name))
            {
                string path = fbd.SelectedPath + "\\" + f.Name;
                _image.image = new Image<Bgr, byte>(@path).Resize(528, 384, Inter.Linear);
                pFrom = f.Name.IndexOf("_") + "_".Length;
                pTo = f.Name.LastIndexOf(".jpg");
                _image.plateName = f.Name.Substring(pFrom, pTo - pFrom);
                numberFrom = f.Name.IndexOf("") + "".Length;
                numberTo = f.Name.LastIndexOf("_");
                _image.number = f.Name.Substring(numberFrom, numberTo - numberFrom);
                ListOfImage.Add(_image);
                Console.WriteLine("File name: " + f.Name + "\t Number: " + _image.number + "\t Plate: " + _image.plateName);
            }
            Console.WriteLine("Wielkość listy ListOfImage: " + ListOfImage.Count + "\n");
            excelAlgorithm[0] = "Algorytm 3";
            excelAlgorithm[4] = ListOfImage.Count.ToString();
            excelAlgorithm[5] = fbd.SelectedPath;

            Console.WriteLine("--- START ---\n");
            //UStawienie parametrów
            //int[] parametr1 = new int[] { 100, 115 , 125 , 150 };
            //int[] parametr2 = new int[] { 3 , 5 };
            int[] parametr1 = new int[] { 3, 8 , 12, 20 };
            int[] parametr2 = new int[] { 3, 5 , 7 };

            for (int i = 0; i < parametr1.Length; i++)
            {
                for (int j = 0; j < parametr2.Length; j++)
                {
                    excelAlgorithm[1] = "0";
                    excelAlgorithm[2] = "0";
                    excelAlgorithm[3] = "0";
                    excelAlgorithm[6] = parametr1[i].ToString();
                    excelAlgorithm[7] = parametr2[j].ToString();

                    foreach (ListImage _imageCar in ListOfImage)
                    {
                        imageBox1.Image = _imageCar.image;
                        _mainImage = _imageCar.image;
                        label8.Text = "Resolution after resize image: " + _mainImage.Width.ToString() + "x" + _mainImage.Height.ToString();

                        _imgAlgorithm_3 = algorytm3(_imageCar.image, parametr1[i], parametr2[j]);

                        _imgPlateRecognition_3 = szukajKonturAuto(_imgAlgorithm_3,  imageBox21, imageBox22, imageBox25, label14, label17, label18, 1);

                        if (_imgPlateRecognition_3 == null)
                        {
                            Console.WriteLine("\n Nie znaleziono tablicy rejestracyjnej");
                            imageBox21.Image = null;
                            showImageError(imageBox22, imageBox25, label14, label17, label18);
                        }
                        else
                        {
                            // Segmentacja
                            _segmentation_Algorithm_3 = segmentacjaZnakowAuto(_imgPlateRecognition_3, imageBox25, label17, label18);

                            _tablePlate_3 = recognitionOfCharactersAuto(_segmentation_Algorithm_3);

                            if (_tablePlate_3 == null)
                            {
                                Console.WriteLine("\n Nie znaleziono znaków na tablicy");
                                label14.Text = "Number Plate: ";
                            }
                            else
                            {
                                Console.WriteLine("\n Tablica rejestracyjna: " + _tablePlate_3);
                                label14.Text = "Number Plate: " + _tablePlate_3;
                            }

                            if (_segmentation_Algorithm_3.Count == _imageCar.plateName.Length)
                            {
                                int count = Int32.Parse(excelAlgorithm[2]);
                                count = count + 1;
                                excelAlgorithm[2] = count.ToString();
                                Console.WriteLine("  Ilość znalezionych segmentów : " + _segmentation_Algorithm_3.Count + " do prawidłowej ilość: " + _imageCar.plateName.Length);
                            }
                            else
                            {
                                Console.WriteLine("  BŁAD --- Ilość znalezionych segmentów : " + _segmentation_Algorithm_3.Count + " do prawidłowej ilość: " + _imageCar.plateName.Length);
                            }

                            if (_tablePlate_3 == _imageCar.plateName)
                            {
                                Console.WriteLine("  Rozpoznana tablica: " + _tablePlate_3 + " prawidłowa tablica: " + _imageCar.plateName);
                                int count = Int32.Parse(excelAlgorithm[3]);
                                count = count + 1;
                                excelAlgorithm[3] = count.ToString();
                            }
                            else
                            {
                                Console.WriteLine("  BŁAD --- Rozpoznana tablica: " + _tablePlate_3 + " prawidłowa tablica: " + _imageCar.plateName);
                            }
                        }
                    }
                    Console.WriteLine("\n Excel wyniki, algorytm: " + excelAlgorithm[0] + " tablica rejestracyjna: " + excelAlgorithm[1] + " segmentacja: " + excelAlgorithm[2] + " rozpoznanie: " + excelAlgorithm[3] + ". Wszystkich obrazów: " + ListOfImage.Count);
                    ExcelSaveData(excelAlgorithm, 2);
                }
            }
            Console.WriteLine("\n----- KONIEC AUTOMAT ALGORYTM 3 -----\n");
        }

        private Image<Gray, byte> algorytm1(Image<Bgr, byte> _mainImage1, int Parametr1, int Parametr2)
        {

            Image<Gray, byte> _imgAlgorithm_1_Async;
            //gray scale
            _imgAlgorithm_1_Async = _mainImage1.Convert<Gray, byte>();
            //Open MorphologyEx
            // kernel 35 , 3 || 40 , 3
            Mat kernelOpen1 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(40, 3), new Point(-1, -1));
            imageBox2.Image = _imgAlgorithm_1_Async;
            _imgAlgorithm_1_Async = _imgAlgorithm_1_Async - _imgAlgorithm_1_Async.MorphologyEx(MorphOp.Open, kernelOpen1, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox3.Image = _imgAlgorithm_1_Async;
            //Threshold
            double prog = CvInvoke.Threshold(_imgAlgorithm_1_Async, _imgAlgorithm_1_Async, 0, 255, ThresholdType.Otsu);
            imageBox4.Image = _imgAlgorithm_1_Async;
            //MessageBox.Show(prog.ToString());
            //Open  and Close MorphologyEx
            // kernelOpen2 i kernelClose 4 , 4  i 4 , 4
            Mat kernelOpen2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(4, 4), new Point(-1, -1));
            Mat kernelClose = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(Parametr1, Parametr2), new Point(-1, -1));
            _imgAlgorithm_1_Async = _imgAlgorithm_1_Async.MorphologyEx(MorphOp.Open, kernelOpen2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox5.Image = _imgAlgorithm_1_Async;
            _imgAlgorithm_1_Async = _imgAlgorithm_1_Async.MorphologyEx(MorphOp.Dilate, kernelClose, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());
            imageBox6.Image = _imgAlgorithm_1_Async;
            //Return Image after detected
            return _imgAlgorithm_1_Async;
        }

        private Image<Gray, byte> algorytm2(Image<Bgr, byte> _mainImage1, int Parametr1, int Parametr2)
        {
            //Bład przy zbyt jasnym zdjeciu, thershold się głubi
            Image<Gray, byte> _imgAlgorithm_2_Async;
            // gray covert
            _imgAlgorithm_2_Async = _mainImage.Convert<Gray, byte>();
            imageBox9.Image = _imgAlgorithm_2_Async;
            // smoothing blur
            _imgAlgorithm_2_Async = _imgAlgorithm_2_Async.SmoothBlur(3, 3);
            imageBox10.Image = _imgAlgorithm_2_Async;
            // vertical edge detection
            _imgAlgorithm_2_Async = _imgAlgorithm_2_Async.Sobel(1, 0, 5).Convert<Gray, byte>();
            //_imgAlgorithm_2_Async = _imgAlgorithm_2_Async.Sobel(0, 1, 3).AbsDiff(new Gray(0.0)).Convert<Gray, byte>(); ;
            imageBox11.Image = _imgAlgorithm_2_Async;
            // thershold
            double prog = CvInvoke.Threshold(_imgAlgorithm_2_Async, _imgAlgorithm_2_Async, 0, 255, ThresholdType.Otsu);
            double wartosc = _imgAlgorithm_2_Async.GetAverage().Intensity;
            //Console.WriteLine(" \n Prog Otsu : " + prog.ToString() + ", średnia wartosc: " + wartosc);

            if (wartosc > 100)
            {
                _imgAlgorithm_2_Async = _imgAlgorithm_2_Async.Not();
                //Console.WriteLine(" \n Obraz zbyt biały, zamieniam na negatyw.");
            }
            // Zamkniecie morfologiczne
            //Mat kernelClose = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(25,4), new Point(-1, -1));
            Mat kernelClose = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(26, 4), new Point(-1, -1));
            _imgAlgorithm_2_Async = _imgAlgorithm_2_Async.MorphologyEx(MorphOp.Close, kernelClose, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox12.Image = _imgAlgorithm_2_Async;
            //Mat kernelClose2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            //_imgAlgorithm_2_Async = _imgAlgorithm_2_Async.MorphologyEx(MorphOp.Open, kernelClose2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //imageBox13.Image = _imgAlgorithm_2_Async;
            //Return Image after detected
            return _imgAlgorithm_2_Async;
        }

        private Image<Gray, byte> algorytm3(Image<Bgr, byte> _mainImage1, int Parametr1, int Parametr2)
        {
            Image<Gray, byte> _imgAlgorithm_3_Async;
            // gray covert
            _imgAlgorithm_3_Async = _mainImage.Convert<Gray, byte>();

            //Top hat
            Mat kernel1 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(90, 90), new Point(-1, -1));
            _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Tophat, kernel1, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //imageBox16.Image = _imgAlgorithm_3_Async;

            //8,5
            Mat kernel2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(Parametr1, Parametr2), new Point(-1, -1));
            _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Blackhat, kernel2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox16.Image = _imgAlgorithm_3_Async;

            // 115,3
            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(115 , 3), new Point(-1, -1));
            _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Close, kernel3, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox17.Image = _imgAlgorithm_3_Async;

            Mat kernel4 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 15), new Point(-1, -1));
            _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Open, kernel4, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox18.Image = _imgAlgorithm_3_Async;

            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(20, 10), new Point(-1, -1));
            _imgAlgorithm_3_Async = _imgAlgorithm_3_Async.MorphologyEx(MorphOp.Open, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox19.Image = _imgAlgorithm_3_Async;

            double prog = CvInvoke.Threshold(_imgAlgorithm_3_Async, _imgAlgorithm_3_Async, 0, 255, ThresholdType.Otsu);
            double wartosc = _imgAlgorithm_3_Async.GetAverage().Intensity;
            imageBox20.Image = _imgAlgorithm_3_Async;

            return _imgAlgorithm_3_Async;
        }

        private Image<Bgr, byte> szukajKonturAuto(Image<Gray, byte> _imgAlgorithm, Emgu.CV.UI.ImageBox imageOut1, Emgu.CV.UI.ImageBox imageOut2, Emgu.CV.UI.ImageBox segmentOut, System.Windows.Forms.Label plateOut, System.Windows.Forms.Label plateResolutionOut, System.Windows.Forms.Label numberOfSegmentOut, int NumberAlgorythm)
        {

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            Image<Bgr, byte> plateRecognize;
            float szerokosc, dlugosc, szerokosc_wysokosc;
            Image<Gray, byte> _imgOut = new Image<Gray, byte>(_imgAlgorithm.Width, _imgAlgorithm.Height);

            CvInvoke.FindContours(_imgAlgorithm, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            Dictionary<int, double> dict = new Dictionary<int, double>();

            if (contours.Size > 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    double area = CvInvoke.ContourArea(contours[i]);
                    Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                    // if (rect.Width < 300 && rect.Width > 60 && rect.Height < 100 && rect.Height > 15)
                    if (rect.Width < 278 && rect.Width > 60 && rect.Height < 73 && rect.Height > 15)
                    {
                        szerokosc = rect.Width;
                        dlugosc = rect.Height;
                        szerokosc_wysokosc = szerokosc / dlugosc;
                        if (szerokosc_wysokosc > 2.4 && szerokosc_wysokosc < 9.5)
                        {
                            //MessageBox.Show("Do doania pole o wymiarach, szerokosc: " + szerokosc + " wysokość " + dlugosc + " lewo " + rect.Left + " prawo " + rect.Right + " dól " + rect.Bottom + " góra " + rect.Top);
                            if (rect.Top > (0.3 * _mainImage.Height) && rect.Left > (0.1 * _mainImage.Width) && rect.Right < (0.9 * _mainImage.Width))
                            {
                                // Console.WriteLine("\n Dodano tablice o wymiarach, szerokość: " + szerokosc + ", wysokość: " + dlugosc + ", lewy bok: " + rect.Left + ", prawy bok: " + rect.Right + ", dolny próg: " + rect.Bottom + ", górny próg: " + rect.Top);
                                dict.Add(i, area);
                            }
                        }
                    }
                }
            }

            var item = dict.OrderByDescending(v => v.Value);

            if (item.Count() == 0)
            {
                imageOut1.Image = null;
                showImageError(imageOut2, segmentOut, plateOut, plateResolutionOut, numberOfSegmentOut);
            }

            foreach (var it in item)
            {
                int key = int.Parse(it.Key.ToString());
                Rectangle rect = CvInvoke.BoundingRectangle(contours[key]);
                CvInvoke.Rectangle(_imgOut, rect, new MCvScalar(100, 255, 0), 3);
                _mainImage.ROI = Rectangle.Empty;
                _mainImage.ROI = rect;
                plateRecognize = _mainImage.CopyBlank();
                _mainImage.CopyTo(plateRecognize);
                imageOut2.Image = plateRecognize;
                imageOut1.Image = _imgAlgorithm + _imgOut;

                DialogResult dialogResult = MessageBox.Show("Wymiary prostokąta: \n Szerokość = " + rect.Size.Width + " Wysokość = " + rect.Size.Height, "Czy to jest tablica ?", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Yes)
                {
                    _mainImage.ROI = Rectangle.Empty;
                    _mainImage = new Image<Bgr, Byte>(imageBox1.Image.Bitmap);
                    int count = Int32.Parse(excelAlgorithm[NumberAlgorythm]);
                    count = count + 1;
                    excelAlgorithm[NumberAlgorythm] = count.ToString();
                    return plateRecognize;
                }
                else if (dialogResult == DialogResult.No)
                {
                    imageOut2.Image = null;
                    imageOut1.Image = null;
                    showImageError(imageOut2, segmentOut, plateOut, plateResolutionOut, numberOfSegmentOut);
                    _mainImage.ROI = Rectangle.Empty;
                    _mainImage = new Image<Bgr, Byte>(imageBox1.Image.Bitmap);
                    plateRecognize = null;
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    imageOut2.Image = null;
                    imageOut1.Image = null;
                    showImageError(imageOut2, segmentOut, plateOut, plateResolutionOut, numberOfSegmentOut);
                    _mainImage.ROI = Rectangle.Empty;
                    _mainImage = new Image<Bgr, Byte>(imageBox1.Image.Bitmap);
                    plateRecognize = null;
                    return null;
                }
            }
            showImageError(imageOut2, segmentOut, plateOut, plateResolutionOut, numberOfSegmentOut);
            return null;
        }

        private List<ListElement> segmentacjaZnakowAuto(Image<Bgr, byte> _ImagePlate, Emgu.CV.UI.ImageBox imageOut1, System.Windows.Forms.Label plateResolutionOut, System.Windows.Forms.Label numberOfSegmentOut)
        {
            Image<Gray, byte> plateRecognize;
            Image<Gray, byte> plate1;
            Image<Gray, byte> plate = _ImagePlate.Convert<Gray, byte>();
            int dlugoscTablicy = plate.Width;
            //CvInvoke.GaussianBlur(plate, plate, new Size(3, 3), 1);

            double prog = CvInvoke.Threshold(plate, plate, 0, 255, ThresholdType.Otsu);
            //imageBox16.Image = plate;
            plate = plate.Not();
            //imageBox16.Image = plate;

            plateResolutionOut.Invoke(new Action(delegate ()
            {
                plateResolutionOut.Text = "Plate resolution, Width: " + plate.Width + " Height: " + plate.Height;
            }));
            //Console.WriteLine("\n Znaleziona tablica ma wymiary: " + plate.Width + " na " + plate.Height);

            plate1 = plate.CopyBlank();
            plate.CopyTo(plate1);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();

            CvInvoke.FindContours(plate, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            //Console.WriteLine(" \n Ilość kontur w FindContour: " + contours.Size);
            //Console.WriteLine(" Hier: channel " + hier.NumberOfChannels + " " + hier.Data );

            Image<Bgr, byte> _imgOut = new Image<Bgr, byte>(plate.Width, plate.Height);
            float szerokosc, wysokosc, szerokosc_wysokosc;
            int srednia;
            int czyZnalezionoCos = 0;
            ListOfStructure = new List<ListElement>();
            ListElement character;

            if (contours.Size > 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                    CvInvoke.Rectangle(_imgOut, rect, new MCvScalar(0, 0, 255), 1);
                    plate.ROI = Rectangle.Empty;
                    plate.ROI = rect;
                    //Console.WriteLine(" \n Znak ma wielkośc " + rect.Width + " wysokośc " + rect.Height);
                    if (rect.Width < 36 && rect.Width > 3 && rect.Height < 37 && rect.Height > 12)
                    {
                        szerokosc = rect.Width;
                        wysokosc = rect.Height;
                        szerokosc_wysokosc = szerokosc / wysokosc;

                        if (szerokosc_wysokosc > 0.18 && szerokosc_wysokosc < 1.2)
                        {
                            double wartosc = plate.GetAverage().Intensity;
                            //Console.WriteLine(" \n Średnia wartość : " + wartosc);
                           // Console.WriteLine(" \n Znak ma wielkośc " + rect.Width + " wysokośc " + rect.Height + " lewy bok: " + plate.ROI.Left + " oraz  0.95 * Dlugość plate: " + 0.95 * dlugoscTablicy);

                            if (wartosc > 160 && plate.ROI.Left < 10)
                            {
                                Console.WriteLine("   Prawdopodobnie znak początka, odrzucam");
                            }
                            else if ((plate.ROI.Left > (0.9 * dlugoscTablicy)) && plate.ROI.Width < 7)
                            {
                                Console.WriteLine("   Prawdopodobnie znak końca, odrzucam");
                            }
                            else
                            {
                                czyZnalezionoCos += 1;
                                srednia = (plate.ROI.Left + plate.ROI.Right) / 2;
                                plateRecognize = plate.CopyBlank();
                                plate.CopyTo(plateRecognize);
                                character.image = plateRecognize;
                                character.Number = srednia;
                                ListOfStructure.Add(character);
                                imageOut1.Image = _ImagePlate + _imgOut;
                               // Console.WriteLine(" \n Dodany znak o wymiarach, szerokość: " + szerokosc + ", wysokość: " + wysokosc + " oraz położeniu: " + srednia + " lewy bok: " + plate.ROI.Left);
                            }
                        }
                    }
                }
                //plate.ROI = Rectangle.Empty;
            }
            else
            {
                MessageBox.Show("Nie znalazłem kontur");
            }

            if (czyZnalezionoCos < 1)
            {
                VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
                Mat hier1 = new Mat();
                Rectangle rect1 = new Rectangle();
                CvInvoke.FindContours(plate1, contours1, hier1, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                //int[,] hierachy = CvInvoke.FindContourTree(plate1, contours1, ChainApproxMethod.ChainApproxSimple);
                //Console.WriteLine("\n Ilość kontur w FindContourTree: " + contours1.Size);

                for (int i = 0; i < contours1.Size; i++)
                {
                    rect1 = CvInvoke.BoundingRectangle(contours1[i]);
                    CvInvoke.Rectangle(_imgOut, rect1, new MCvScalar(0, 0, 255), 1);
                    plate1.ROI = Rectangle.Empty;
                    plate1.ROI = rect1;
                    //Console.WriteLine(" \n Znak ma wielkośc " + rect1.Width + " wysokośc " + rect1.Height + " oraz numer " + i);

                    if (rect1.Width < 36 && rect1.Width > 3 && rect1.Height < 37 && rect1.Height > 12)
                    {
                        szerokosc = rect1.Width;
                        wysokosc = rect1.Height;
                        szerokosc_wysokosc = szerokosc / wysokosc;

                        if (szerokosc_wysokosc > 0.18 && szerokosc_wysokosc < 1.2)
                        {
                            double wartosc = plate1.GetAverage().Intensity;
                            //Console.WriteLine(" \n Średnia wartość : " + wartosc);
                            //Console.WriteLine(" \n Znak ma wielkośc " + rect1.Width + " wysokośc " + rect1.Height + " lewy bok: " + plate1.ROI.Left + " oraz  0.95 * Dlugość plate: " + 0.95 * dlugoscTablicy);

                            if (wartosc > 160 && plate1.ROI.Left < 10)
                            {
                                Console.WriteLine("   Prawdopodobnie znak początka, odrzucam");
                            }
                            else if ((plate1.ROI.Left > (0.9 * dlugoscTablicy)) && plate1.ROI.Width < 7)
                            {
                                Console.WriteLine("   Prawdopodobnie znak końca, odrzucam");
                            }
                            else
                            {
                                srednia = (plate1.ROI.Left + plate1.ROI.Right) / 2;
                                plateRecognize = plate1.CopyBlank();
                                plate1.CopyTo(plateRecognize);
                                character.image = plateRecognize;
                                character.Number = plate1.ROI.Left;
                                ListOfStructure.Add(character);
                                imageOut1.Image = _ImagePlate + _imgOut;
                                //Console.WriteLine(" \n Dodany znak o wymiarach, szerokość: " + szerokosc + ", wysokość: " + wysokosc + " oraz położeniu: " + srednia + " lewy bok: " + plate1.ROI.Left);
                            }
                        }
                    }
                }
            }


            ListOfStructure.Sort((x, y) => x.Number - y.Number);

            ListElement characterpoprzedni;
            characterpoprzedni.Number = 0;
            int index = 0;
            int[] tab = new int[ListOfStructure.Count];

            foreach (ListElement character1 in ListOfStructure)
            {
                //NewWindow Powiekszenie = new NewWindow(character1.image) {  Text = "Szerokosc: " + character1.image.Width + " Wysokość: " + character1.image.Height };
                //Powiekszenie.Show();
                if (character1.Number - characterpoprzedni.Number < 4)
                {
                    tab[index] = 1;
                }
                index += 1;
                characterpoprzedni = character1;
            }

            int licznik = 0;
            foreach (int idx in tab)
            {
                if (idx == 1)
                {
                    ListOfStructure.RemoveAt(licznik);
                    Console.WriteLine("    Odrzuciłem znak");
                    licznik = licznik - 1;
                }
                licznik += 1;
            }

            numberOfSegmentOut.Invoke(new Action(delegate ()
            {
                numberOfSegmentOut.Text = "Number of segment: " + ListOfStructure.Count;
            }));

            return ListOfStructure;

        }

        private string recognitionOfCharactersAuto(List<ListElement> _segmentation_Algorithm)
        {
            Image<Gray, byte> characterOfSegment, characterOfPattern;
            int szerokoscWzorca, wysokoscWzorca;
            int ListElementCount = 0;
            int characterCount = 0;
            float resultOfMatchTemplate = 0;
            float maxValue = 0;
            float maxValue_under = 0;
            string nameOfPattern = null;
            string nameOfPattern_under = null;
            string tablica = null;

            foreach (ListElement character in ListOfStructure)
            {
                ListElementCount += 1;
                characterCount += 1;
                characterOfSegment = character.image.Not();
                maxValue = 0;
                maxValue_under = 0;
                nameOfPattern = null;
                nameOfPattern_under = null;

                foreach (ListPatterns pattern in ListOfPatterns)
                {
                    characterOfPattern = pattern.image;
                    szerokoscWzorca = characterOfPattern.Width;
                    wysokoscWzorca = characterOfPattern.Height;

                    characterOfSegment = characterOfSegment.Resize(szerokoscWzorca, wysokoscWzorca, Inter.Linear);
                    var res = characterOfSegment.MatchTemplate(characterOfPattern, TemplateMatchingType.CcoeffNormed);
                    resultOfMatchTemplate = res.Data[0, 0, 0];

                    if (resultOfMatchTemplate > maxValue_under)
                    {
                        if (characterCount > 2 && pattern.name == "O.jpg")
                        {
                            Console.WriteLine("    Pominąłem O w drugiej częsci tablicy");
                        }
                        else if (resultOfMatchTemplate > maxValue)
                        {
                            maxValue_under = maxValue;
                            nameOfPattern_under = nameOfPattern;
                            maxValue = resultOfMatchTemplate;
                            nameOfPattern = pattern.name;
                        }
                        else
                        {
                            maxValue_under = resultOfMatchTemplate;
                            nameOfPattern_under = pattern.name;
                        }
                    }
                    // Console.WriteLine("\n Wartosc result " + resultOfMatchTemplate + " dla Segmentu " + ListElementCount + " dla Wzorca: " + pattern.name);
                }
                //Console.WriteLine("\n Wartosc maxValue_lider " + maxValue + " dla segmentu " + ListElementCount + " oraz wzorca: " + nameOfPattern.Substring(0, 1) + " oraz maxValue_vicelider " + maxValue_under + " dla Wzorca: " + nameOfPattern_under.Substring(0, 1));
                tablica = tablica + nameOfPattern.Substring(0, 1);
            }
            return tablica;

        }

    }
}
