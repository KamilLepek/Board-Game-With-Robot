using System;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;

namespace BoardGameWithRobot.ImageProcessing
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
        public bool InitializeBoard(Board world)
        {
            if (!this.DetectTrackersOnInit(world)) 
            {
                Console.WriteLine("Board initialization failed.");
                return false;
            }
            return true;
        }

        public bool InitializeRobot()
        {
            return true;
        }

        /// <summary>
        /// Detected blue square on the image to determine if board is available(board has blue square tracker)
        /// </summary>
        /// <returns>true if tracker detected</returns>
        public bool DetectTrackersOnInit(Board world)
        {
            for (int i = 0; i < Constants.DetectorFrameSearchRadius; i++)
            {
                this.cameraService.GetCameraFrame();
                this.DetectTrackerInSquare(world, true);
            }
            if (world.TrackersList.Count == Constants.NumberOfTrackers)
                return true;
            Console.WriteLine("{0} trackers detected instead of {1}", world.TrackersList.Count, Constants.NumberOfTrackers);
            return false;
        }

        public bool DetectTrackerInSquare(Board world, bool wholeMap = false)
        {
            if (wholeMap)
            {
                BlueSquareTrackingService.DetectTrackersOnImage(world, this.cameraService.ActualFrame);
                return true;
            }
            else
            {
                //szukaj w kwadracie ktorego wymiary masz Tracker...
                //jesli chcemy zwracac true to update Tracker...
                this.framesNoMatch = 0;
                BlueSquareTrackingService.DetectTrackersOnImage(world, this.cameraService.ActualFrame);
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
