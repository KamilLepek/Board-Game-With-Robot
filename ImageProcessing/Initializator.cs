using System;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;

namespace BoardGameWithRobot.ImageProcessing
{
    /// <summary>
    /// Class used for image processing operations for initialization
    /// </summary>
    internal class Initializator
    {
        private readonly CameraService cameraService;

        private readonly BlueSquareTrackingService blueSquareTrackingService;

        private readonly Board board;

        public Initializator(CameraService camera, BlueSquareTrackingService blueSquareTracking, Board b)
        {
            this.board = b;
            this.cameraService = camera;
            this.blueSquareTrackingService = blueSquareTracking;
        }

        /// <summary>
        /// Initializes board related elements
        /// </summary>
        /// <returns>returns true if starting the game is not forbidden</returns>
        public bool InitializeBoard()
        {
            if (!this.DetectTrackersOnInit()) 
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
        /// Detects blue square trackers on start
        /// </summary>
        /// <returns>true if tracker detected</returns>
        public bool DetectTrackersOnInit()
        {
            for (int i = 0; i < Constants.AmountOfInitFramesToSearchForTrackers; i++)
            {
                this.cameraService.GetCameraFrame();
                this.blueSquareTrackingService.DetectBlueSquareTrackersOnInit();
                this.cameraService.ShowFrame();
            }
            if (this.board.TrackersList.Count == Constants.NumberOfTrackers)
                return true;
            Console.WriteLine("{0} trackers detected instead of {1}", this.board.TrackersList.Count, Constants.NumberOfTrackers);
            return false;
        }
    }
}
