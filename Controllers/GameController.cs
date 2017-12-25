using System;
using System.Diagnostics;
using System.Windows.Forms;
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

        private readonly Initializator initializator;

        private readonly Stopwatch timer = new Stopwatch();

        private readonly SituationController situationController;

        private readonly BlueSquareTrackingService blueSquareTrackingService;

        private Enums.Situation situation;

        private Enums.Turn turn;

        private readonly Board board;

        public GameController()
        {
            this.board = new Board();
            this.cameraService = new CameraService();
            this.situationController = new SituationController();
            this.blueSquareTrackingService = new BlueSquareTrackingService(this.cameraService, this.board);
            this.initializator = new Initializator(this.cameraService, this.blueSquareTrackingService, this.board);
        }

        /// <summary>
        /// Initializes everything at the beginning
        /// </summary>
        /// <returns>true if every step succeeded</returns>
        public bool InitializeGame()
        {
            if (!this.cameraService.InitializeCamera())
                return false;
            if (!this.initializator.InitializeBoard())
                return false;
            if (!this.initializator.InitializeRobot())
                return false;
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
                this.SetFragmentsOfImageForTrackersDetection();

                //check if tracker is there where it should be
                if (!this.blueSquareTrackingService.DetectTrackerInSquare())
                {
                    MessageBox.Show("Trackers has been lost. Game is finished.");
                    return;
                }
                Console.WriteLine(this.timer.ElapsedMilliseconds + " ms between frames. " + (int)(1000/ this.timer.ElapsedMilliseconds) + " fps.");

                this.AnalyzeAndChangeStateOfGame();
                this.DisplayElementsOnBoard();
                this.cameraService.ShowFrame();
            }
        }

        private void SetFragmentsOfImageForTrackersDetection()
        {
            foreach (var blueSquareTracker in this.board.TrackersList)
            {
                blueSquareTracker.SetImage(this.cameraService.ActualFrame);
            }
        }

        private void IncrementFramesAndCheckTrackersState()
        {
            foreach (var blueSquareTracker in this.board.TrackersList)
            {
                if (blueSquareTracker.FramesSinceDetected++ > Constants.MaxFrameAmountTrackerNotDetectedToDelete)
                    blueSquareTracker.State = Enums.TrackerDetectionState.Inactive;
            }           
        }

        private void DisplayElementsOnBoard()
        {
            this.board.PrintTrackersOnImage(this.cameraService.ActualFrame);
        }

        /// <summary>
        /// Analyzes and changes actual state of the game
        /// </summary>
        private void AnalyzeAndChangeStateOfGame()
        {
            this.IncrementFramesAndCheckTrackersState();
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
