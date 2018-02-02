using System;
using System.Linq;
using System.Net.Sockets;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Renci.SshNet;

namespace BoardGameWithRobot.ImageProcessing
{
    /// <summary>
    ///     Class used for image processing operations for initialization
    /// </summary>
    internal class Initializator
    {
        private readonly BlueSquareTrackingService blueSquareTrackingService;

        private readonly Board board;

        private readonly CameraService cameraService;

        private readonly FieldsDetectingService fieldsDetectingService;

        private readonly GamePawnsDetectingService gamePawnsDetectingService;

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
        ///     Initializes board related(visual) elements
        /// </summary>
        /// <returns>returns true if starting the game is not forbidden</returns>
        public bool InitializeBoard()
        {
            MessageLogger.LogMessage("Initializing Board: Trackers detection..");
            if (!this.DetectTrackersOnInit())
            {
                Console.WriteLine("Trackers initialization failed. Trying again...");
                if (!this.DetectTrackersOnInit())
                {
                    Console.WriteLine("Trackers initialization failed.");
                    return false;
                }
            }
            MessageLogger.LogMessage("Initializing Board: Fields detection..");
            if (!this.DetectFieldsOnInit())
            {
                Console.WriteLine("Fields initialization failed. Trying again...");
                if (!this.DetectFieldsOnInit())
                {
                    Console.WriteLine("Fields initialization failed.");
                    return false;
                }
            }
            MessageLogger.LogMessage("Initializing Board: Pawns detection..");
            if (!this.DetectGamePawnsOnInit())
            {
                Console.WriteLine("Game pawns initialization failed.");
                return false;
            }
            return true;
        }

        public bool InitializeRobot()
        {
            MessageLogger.LogMessage("Initializing SSH connection with robot..");
            RobotControllingService.InitializeSshConnectionParams();
            try
            {
                RobotControllingService.Ssh.Connect();
            }
            catch (SocketException)
            {
                Console.WriteLine("Couldn't connect to Robot");
                return false;
            }
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
                return true;
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
                    this.board.SetFieldLabel(field);
                RobotControllingService.TrackStartPoint =
                    this.board.FieldsList.Find(v => v.Label == "R1").Boundary.MassCenter;
                RobotControllingService.TrackEndPoint =
                    this.board.FieldsList.Find(v => v.Label == $"R{Constants.NumberOfFields / 2}").Boundary.MassCenter;
                return true;
            }
            Console.WriteLine("{0} fields detected instead of {1}", this.board.FieldsList.Count,
                Constants.NumberOfFields);
            return false;
        }

        /// <summary>
        ///     Detects blue square trackers on start
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
            Console.WriteLine("{0} trackers detected instead of {1}", this.board.TrackersList.Count,
                Constants.NumberOfTrackers);
            return false;
        }
    }
}