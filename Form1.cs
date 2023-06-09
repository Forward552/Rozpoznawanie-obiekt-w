using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Camera_Configuration;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Rozpoznawanie_obiektów_na_zdjeciach.Classes;
using Rozpoznawanie_obiektów_na_zdjeciach.Helpers;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace Rozpoznawanie_obiektów_na_zdjeciach
{
    public partial class Form1 : Form
    {
        int _frame = 0;
        private Thread camera;
        bool isCameraRunning = false;
        private bool _wykrywanieTwarzy = true;
        private static int _faces = 0;

        public Form1()
        {
            InitializeComponent();
            _ListCameras();
            fpsTimer.Start();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text.Equals("Start Camera"))
            {
                isCameraRunning = true;
                CaptureCamera();
                btnStart.Text = "Stop Camera";
            }
            else
            {
                isCameraRunning = false;
                stopCamera();
                btnStart.Text = "Start Camera";
            }
        }

        // Declare required methods
        private void CaptureCamera()
        {
            int cameraindex = Convert.ToInt32(cbVideoSource.SelectedValue);
            string result = Camera.Init(cameraindex);
            tbOutputConsole.AppendText("Camera Started" + Environment.NewLine + result + Environment.NewLine);
            camera = new Thread(new ThreadStart(CaptureCameraCallback));
            camera.Start();
        }

        private void stopCamera()
        {
            Camera.ResetCamera();
        }

        private void CaptureCameraCallback()
        {
            while (isCameraRunning)
            {
                if (_wykrywanieTwarzy)
                    pictureBox1.Image = Camera.ScanImage();
                else
                    detekcjaTwarzy(Camera.ScanImage().ToMat());
                Thread.Sleep(10);
                _frame++;
            }
        }

        private void btnSnapShot_Click(object sender, EventArgs e)
        {
            if (isCameraRunning)
            {
                // Take snapshot of the current image generate by OpenCV in the Picture Box
                Bitmap snapshot = new Bitmap(pictureBox1.Image);
                FolderBrowserDialog folderDlg = new FolderBrowserDialog();
                folderDlg.ShowNewFolderButton = true;
                // Show the FolderBrowserDialog.  
                DialogResult result = folderDlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    snapshot.Save(string.Format(folderDlg.SelectedPath + @"\{0}.jpg", Guid.NewGuid()), ImageFormat.Jpeg);
                }
            }
            else
            {
                Console.WriteLine("Cannot take picture if the camera isn't capturing image!");
            }
        }

        private void _ListCameras()
        {
            cbVideoSource.DataSource = Camera.DetectCamerasConnected();
            cbVideoSource.DisplayMember = "Text";
            cbVideoSource.ValueMember = "Value";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isCameraRunning = false;
            stopCamera();
        }

        private void sbtest_Click(object sender, EventArgs e)
        {
            _wykrywanieTwarzy = !_wykrywanieTwarzy;
        }

        private void fpsTimer_Tick(object sender, EventArgs e)
        {
            laFps.Text = (_frame).ToString();
            laFaces.Text = _faces.ToString();
            _frame = 0;
        }

        public void detekcjaTwarzy(Mat srcImage)
        {
            
            var grayImage = new Mat();
            Cv2.CvtColor(srcImage, grayImage, ColorConversionCodes.BGRA2GRAY);
            Cv2.EqualizeHist(grayImage, grayImage);

            var cascade = new CascadeClassifier(@"C:\Users\mzdro\source\repos\Rozpoznawanie obiektów na zdjeciach\Data\haarcascade_frontalface_alt.xml");
            var nestedCascade = new CascadeClassifier(@"C:\Users\mzdro\source\repos\Rozpoznawanie obiektów na zdjeciach\Data\haarcascade_eye_tree_eyeglasses.xml");

            var faces = cascade.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.1,
                minNeighbors: 2,
                flags: HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.ScaleImage,
                minSize: new Size(30, 30)
                );

            _faces = faces.Length.GetSafeInt();

            var count = 1;
            foreach (var faceRect in faces)
            {
                var detectedFaceImage = new Mat(srcImage, faceRect);
                //pictureBox1.Image = detectedFaceImage.ToBitmap();
                //Cv2.WaitKey(1); // do events

                //var color = Scalar.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));

                Cv2.Rectangle(srcImage, faceRect, Scalar.Green, 3);


                var detectedFaceGrayImage = new Mat();
                Cv2.CvtColor(detectedFaceImage, detectedFaceGrayImage, ColorConversionCodes.BGRA2GRAY);
                var nestedObjects = nestedCascade.DetectMultiScale(
                    image: detectedFaceGrayImage,
                    scaleFactor: 1.1,
                    minNeighbors: 2,
                    flags: HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.ScaleImage,
                    minSize: new Size(30, 30)
                    );
                //Wykrywanie oczu
                //foreach (var nestedObject in nestedObjects)
                //{
                //    var center = new Point
                //    {
                //        X = (int)(Math.Round(nestedObject.X + nestedObject.Width * 0.5, MidpointRounding.ToEven) + faceRect.Left),
                //        Y = (int)(Math.Round(nestedObject.Y + nestedObject.Height * 0.5, MidpointRounding.ToEven) + faceRect.Top)
                //    };
                //    var radius = Math.Round((nestedObject.Width + nestedObject.Height) * 0.25, MidpointRounding.ToEven);
                //    Cv2.Circle(srcImage, center, (int)radius, Scalar.LightBlue, thickness: 3);
                //}

                count++;
            }
            pictureBox1.Image = srcImage.ToBitmap();
            srcImage.Dispose();


        }





    }


}



