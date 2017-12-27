using System;
using System.Diagnostics;
using System.Windows.Forms;
using BoardGameWithRobot.ImageProcessing;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

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

        private readonly BlueSquareTrackingService blueSquareTrackingService;

        private readonly DiceDetectingService diceDetectingService;

        private Enums.Situation situation;

        private Enums.Turn turn;

        private readonly Board board;

        private int diceDetectionFrames = 0;

        private int dicePipsNumber = 0;

        public GameController()
        {
            this.board = new Board();
            this.cameraService = new CameraService();
            this.blueSquareTrackingService = new BlueSquareTrackingService(this.cameraService, this.board);
            this.initializator = new Initializator(this.cameraService, this.blueSquareTrackingService, this.board);
            this.diceDetectingService = new DiceDetectingService(this.cameraService);
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

                this.AnalyzeAndChangeStateOfGame();
                this.DisplayElementsOnBoard();
                this.cameraService.ShowFrame();
                Console.WriteLine(this.timer.ElapsedMilliseconds + " ms between frames. " + (int)(1000 / this.timer.ElapsedMilliseconds) + " fps.");
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
                    MessageLogger.LogMessage(string.Format("{0} turn. Roll the dice!", this.turn.ToString()));
                    this.ChangeStateUponDiceDetection();
                    break;
                case Enums.Situation.AwaitForReaction:
                    MessageLogger.LogMessage(string.Format("{0} turn. Move {1} squares!", this.turn.ToString(), this.dicePipsNumber));
                    if (false)//validate that player has finished his movement!
                    {
                        this.situation = Enums.Situation.AwaitToRollTheDice;
                        this.ChangeTurn();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } 
        }

        /// <summary>
        /// Changes state if dice have been detected long enough to ensure that the result is not affected by movement
        /// </summary>
        private void ChangeStateUponDiceDetection()
        {
            if (this.diceDetectingService.DetectDice())
                this.diceDetectionFrames++;
            else
                this.diceDetectionFrames--;
            if (this.diceDetectionFrames < 0)
            {
                // Reset region in order to search again
                this.diceDetectionFrames = 0;
                this.diceDetectingService.IsDiceRegionDefined = false;
            }

            if (this.diceDetectionFrames == Constants.DiceFramesDetectedAcceptanceMargin)
            {
                this.dicePipsNumber = this.diceDetectingService.DetermineNumberAndResetPipList();
                this.diceDetectionFrames = 0;
                this.diceDetectingService.IsDiceRegionDefined = false;
                this.situation = Enums.Situation.AwaitForReaction;
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
