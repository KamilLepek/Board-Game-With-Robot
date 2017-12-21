using System;
using BoardGameWithRobot.Utilities;

namespace BoardGameWithRobot.Controllers
{
    /// <summary>
    /// Main image processing class
    /// </summary>
    internal class Detector
    {
        private readonly CameraService cameraService;

        /// <summary>
        /// Number of frames that we havent detected tracker
        /// </summary>
        private int framesNoMatch = 0;

        public Detector(CameraService camera)
        {
            this.cameraService = camera;
        }

        /// <summary>
        /// Checks if game can start
        /// </summary>
        /// <returns>returns true if starting the game is possible</returns>
        public bool InitializeBoard()
        {
            if (!this.DetectTrackerOnInit())
                return false;
            return true;
        }

        /// <summary>
        /// Detected blue square on the image to determine if board is available(board has blue square tracker)
        /// </summary>
        /// <returns>true if tracker detected</returns>
        public bool DetectTrackerOnInit()
        {
            for (int i = 0; i < Constants.DetectorFrameSearchRadius; i++)
            {
                this.cameraService.GetCameraFrame();
                this.DetectTrackerInSquare(true);
                return true;
            }
            Console.WriteLine("Tracker not detected!");
            return false;
        }

        public bool DetectTrackerInSquare(bool wholeMap = false)
        {
            BlueSquareTracker.DetectTrackerOnImage(this.cameraService.ActualFrame);
            if (wholeMap)
            {
                //jesli chcemy zwracac true to update Tracker...
                return true;
            }
            else
            {
                //szukaj w kwadracie ktorego wymiary masz Tracker...
                //jesli chcemy zwracac true to update Tracker...
                this.framesNoMatch = 0;
                return true;
                if (this.framesNoMatch < Constants.NumberOfFramesWithNoTrackerDetectedToEscalate)
                {
                    Console.WriteLine("Tracker not detected for " + ++this.framesNoMatch + "frames");
                    return true;
                }
            }
            Console.WriteLine("Tracker lost. Possibility of board/camera movement!");
            return false;
        }

        

        
    }
}
