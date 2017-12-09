using Emgu.CV;

namespace BoardGameWithRobot.Utilities
{
    internal class CameraService
    {
        private readonly Emgu.CV.Capture cameraDevice;

        public CameraService()
        {
            this.cameraDevice = new Emgu.CV.Capture(Constants.CameraId);
            this.cameraDevice.Start();
        }

        public Mat GetCameraFrame()
        {
            Mat frame = new Mat();
            this.cameraDevice.Retrieve(frame);
            return frame;
        }
    }
}
