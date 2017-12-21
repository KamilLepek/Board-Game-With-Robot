using System;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace BoardGameWithRobot.Utilities
{
    internal class CameraService
    {
        private Capture cameraDevice;

        public Mat ActualFrame { get; }

        public CameraService()
        {
            this.ActualFrame = new Mat();
        }

        public bool InitializeCamera()
        {
            try
            {
                this.cameraDevice = new Capture(Constants.CameraId);
                this.SetCameraProperties();
                this.cameraDevice.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }


        private void SetCameraProperties()
        {
            double width, height;
            switch (Constants.Quality)
            {
                case "FHD":
                    height = 1080.0;
                    width = 1920.0;
                    break;
                case "HD":
                    height = 720;
                    width = 1280;
                    break;
                case "480":
                    height = 480.0;
                    width = 854.0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Not supported video camera input format");
            }
            //this.cameraDevice.SetCaptureProperty(CapProp.FourCC,) //FOURCC code?
            this.cameraDevice.SetCaptureProperty(CapProp.FrameHeight, height);
            this.cameraDevice.SetCaptureProperty(CapProp.FrameWidth, width);
        }

        /// <summary>
        /// Retrieves single frame from camera device
        /// </summary>
        /// <returns>Returns frame as matrix</returns>
        public void GetCameraFrame()
        {
            if (!this.cameraDevice.Retrieve(this.ActualFrame))
                throw new Exception("Failed to retrieve camera frame");
        }

        /// <summary>
        /// Prints image on window
        /// </summary>
        /// <param name="image">image to print</param>
        /// <param name ="name">name of the window</param>
        public static void PrintMatrix(Mat image, string name = Constants.WindowName)
        {
            CvInvoke.NamedWindow(name, NamedWindowType.FreeRatio);
            CvInvoke.Imshow(name, image);
            CvInvoke.WaitKey(Constants.MinimumDelayBetweenFrames);
        }

        /// <summary>
        /// Prints frame from camera on window
        /// </summary>
        public void PrintFrame()
        {
            PrintMatrix(this.ActualFrame);
        }
    }
}
