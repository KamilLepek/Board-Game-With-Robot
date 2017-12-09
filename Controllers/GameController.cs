using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.Controllers
{
    internal class GameController
    {
        private readonly CameraService cameraService;

        public GameController()
        {
            this.cameraService = new CameraService();
        }

        public void Initialize()
        {
            
        }

        public void PrintFrame()
        {
            CvInvoke.Imshow("Camera", cameraService.GetCameraFrame());
            CvInvoke.WaitKey(1);
        }
    }
}
