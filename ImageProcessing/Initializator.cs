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

        private readonly FieldsDetectingService fieldsDetectingService;

        private readonly GamePawnsDetectingService gamePawnsDetectingService;

        private readonly Board board;

        public Initializator(CameraService camera, BlueSquareTrackingService blueSquareTracking,
            FieldsDetectingService fieldService, GamePawnsDetectingService gamePawnsService, Board b)
        {
            this.board = b;
            this.cameraService = camera;
            this.blueSquareTrackingService = blueSquareTracking;
            this.fieldsDetectingService = fieldService;
            this.gamePawnsDetectingService = gamePawnsService;
        }

        /// <summary>
        /// Initializes board related(visual) elements
        /// </summary>
        /// <returns>returns true if starting the game is not forbidden</returns>
        public bool InitializeBoard()
        {
            if (!this.DetectTrackersOnInit()) 
            {
                Console.WriteLine("Trackers initialization failed. Trying again...");
                if (!this.DetectTrackersOnInit())
                {
                    Console.WriteLine("Trackers initialization failed.");
                    return false;
                }
            }
            if (!this.DetectFieldsOnInit())
            {
                Console.WriteLine("Fields initialization failed. Trying again...");
                if (!this.DetectFieldsOnInit())
                {
                    Console.WriteLine("Fields initialization failed.");
                    return false;
                }
            }
            if (!this.DetectGamePawnsOnInit())
            {
                Console.WriteLine("Game pawns initialization failed.");
                return false;
            }
            return true;
        }

        
        public bool InitializeRobot()
        {
            return true;
        }

        public bool DetectGamePawnsOnInit()
        {
            for (int i = 0; i < Constants.AmountOfInitFramesToSearchForPawns; i++)
            {
                this.cameraService.GetCameraFrame();
                this.gamePawnsDetectingService.DetectPawnsOnInit();
                this.cameraService.ShowFrame();
            }
            if (this.board.PawnsList.Count == Constants.NumberOfPawns)
            {
                return true;
            }
            Console.WriteLine("{0} pawns detected instead of {1}", this.board.PawnsList.Count, Constants.NumberOfPawns);
            return false;
        }

        public bool DetectFieldsOnInit()
        {
            for (int i = 0; i < Constants.AmountOfInitFramesToSearchForFields; i++)
            {
                this.cameraService.GetCameraFrame();
                this.fieldsDetectingService.DetectFieldsOnInit();
                this.cameraService.ShowFrame();
            }
            if (this.board.FieldsList.Count == Constants.NumberOfFields)
            {
                foreach (var field in this.board.FieldsList)
                {
                    this.board.SetFieldLabel(field);
                }
                return true;
            }
            Console.WriteLine("{0} fields detected instead of {1}", this.board.FieldsList.Count, Constants.NumberOfFields);
            return false;
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
