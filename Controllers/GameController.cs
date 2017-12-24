using System;
using System.Diagnostics;
using BoardGameWithRobot.ImageProcessing;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;

namespace BoardGameWithRobot.Controllers
{
    /// <summary>
    /// Main controller of the application
    /// </summary>
    internal class GameController
    {
        private readonly CameraService cameraService;

        private readonly Detector detector;

        private readonly Stopwatch timer = new Stopwatch();

        private readonly SituationController situationController;

        private Enums.Situation situation;

        private Enums.Turn turn;

        public Board World { get; private set; }

        public GameController()
        {
            this.cameraService = new CameraService();
            this.detector = new Detector(this.cameraService);
            this.situationController = new SituationController();
            this.World = new Board();
        }

        /// <summary>
        /// Initializes everything at the beginning
        /// </summary>
        /// <returns>true if every step succeeded</returns>
        public bool InitializeGame()
        {
            if (!this.cameraService.InitializeCamera())
                return false;
            if (!this.detector.InitializeBoard(this.World))
                return false;
            if (!this.detector.InitializeRobot())
                return false;
            this.World.SetMainRectangleSize();
            MessageLogger.LogMessage("Initialization completed successfully");
            this.situation = Enums.Situation.AwaitToRollTheDice;
            this.turn = Enums.Turn.Human;
            return true;
        }

        /// <summary>
        /// Main loop of the game
        /// </summary>
        public void PlayGame()
        {
            while (true)
            {
                this.timer.Restart();
                this.cameraService.GetCameraFrame();

                //check if tracker is there where it should be
                if (!this.detector.DetectTrackerInSquare(this.World))
                {
                    // if tracker wasnt spotted in the right place, init everything again
                    if (!this.detector.InitializeBoard(this.World))
                    {   
                        Console.WriteLine("Tracker has been lost!");
                        return;
                    }
                }
                double elapsed = this.timer.ElapsedMilliseconds;
                Console.WriteLine(elapsed + " ms between frames. " + (int)(1000/elapsed) + " fps.");

                this.AnalyzeAndChangeStateOfGame();

                this.World.PrintMainRectangle(this.cameraService.ActualFrame);
                this.World.PrintTrackersOnImage(this.cameraService.ActualFrame);
                this.cameraService.ShowFrame();
            }
        }

        /// <summary>
        /// Analyzes and changes actual state of the game
        /// </summary>
        private void AnalyzeAndChangeStateOfGame()
        {
            switch (this.situation)
            {
                case Enums.Situation.AwaitToRollTheDice:
                    if (this.situationController.CheckIfDiceStartedRolling())
                        this.situation = Enums.Situation.DiceIsRolling;
                    break;
                case Enums.Situation.DiceIsRolling:
                    if (this.situationController.CheckIfDiceFinnishedRolling())
                        this.situation = Enums.Situation.AwaitForReaction;
                    break;
                case Enums.Situation.AwaitForReaction:
                    if (this.situationController.CheckIfPlayerFinishedTurn())
                    {
                        this.situation = Enums.Situation.DiceIsRolling;
                        this.ChangeTurn();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } 
        }

        private void ChangeTurn()
        {
            if (this.turn == Enums.Turn.Human)
                this.turn = Enums.Turn.Robot;
            else if (this.turn == Enums.Turn.Robot)
                this.turn = Enums.Turn.Human;
            else
                throw new ArgumentOutOfRangeException();
        }

         
    }
}
