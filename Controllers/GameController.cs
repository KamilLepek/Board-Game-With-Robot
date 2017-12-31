using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using BoardGameWithRobot.ImageProcessing;
using BoardGameWithRobot.Map;
using BoardGameWithRobot.Utilities;

namespace BoardGameWithRobot.Controllers
{
    /// <summary>
    ///     Main controller of the application
    /// </summary>
    internal class GameController
    {
        private readonly BlueSquareTrackingService blueSquareTrackingService;

        private readonly Board board;

        private readonly CameraService cameraService;

        private readonly DiceDetectingService diceDetectingService;

        private readonly FieldsDetectingService fieldsDetectingService;

        private readonly GamePawnsDetectingService gamePawnsDetectingService;

        private readonly Initializator initializator;

        private readonly Stopwatch timer = new Stopwatch();

        private int diceDetectionFrames;

        private int dicePipsNumber;

        private Enums.Player player;

        private Enums.Situation situation;

        public GameController()
        {
            this.board = new Board();
            this.cameraService = new CameraService();
            this.blueSquareTrackingService = new BlueSquareTrackingService(this.cameraService, this.board);
            this.fieldsDetectingService = new FieldsDetectingService(this.cameraService, this.board);
            this.gamePawnsDetectingService = new GamePawnsDetectingService(this.cameraService, this.board);
            this.initializator = new Initializator(this.cameraService, this.blueSquareTrackingService,
                this.fieldsDetectingService, this.gamePawnsDetectingService, this.board);
            this.diceDetectingService = new DiceDetectingService(this.cameraService);
        }

        /// <summary>
        ///     Initializes everything at the beginning
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
            this.player = Enums.Player.Human;
            MessageBox.Show("Initialization Completed! Press ok to start.");
            return true;
        }

        /// <summary>
        ///     Main loop of the game
        /// </summary>
        public void PlayGame()
        {
            bool finishFlag = false;
            while (!finishFlag)
            {
                this.timer.Restart();
                this.cameraService.GetCameraFrame();
                this.SetFragmentsOfImageForTrackersDetection();

                //check if tracker is there where it should be
                if (!this.blueSquareTrackingService.DetectTrackersInTheirSquares())
                {
                    MessageBox.Show("Trackers has been lost. Game is finished.");
                    return;
                }

                this.AnalyzeAndChangeStateOfGame(ref finishFlag);
                this.DisplayElementsOnBoard();
                this.cameraService.ShowFrame();
                Console.WriteLine(this.timer.ElapsedMilliseconds + " ms between frames. " +
                                  (int) (1000 / this.timer.ElapsedMilliseconds) + " fps.");
            }
        }

        private void SetFragmentsOfImageForTrackersDetection()
        {
            foreach (var blueSquareTracker in this.board.TrackersList)
                blueSquareTracker.SetImage(this.cameraService.ActualFrame);
        }

        private void IncrementFramesAndCheckTrackersState()
        {
            foreach (var blueSquareTracker in this.board.TrackersList)
                if (blueSquareTracker.FramesSinceDetected++ > Constants.MaxFrameAmountTrackerNotDetectedToBecomeInactive)
                    blueSquareTracker.State = Enums.TrackerDetectionState.Inactive;
        }

        private void DisplayElementsOnBoard()
        {
            this.board.PrintTrackersOnImage(this.cameraService.ActualFrame);
            this.board.PrintFieldsOnBoard(this.cameraService.ActualFrame);
            this.board.PrintPawnsSquarePlaceOnBoard(this.cameraService.ActualFrame);
        }

        /// <summary>
        ///     Analyzes and changes actual state of the game
        /// </summary>
        private void AnalyzeAndChangeStateOfGame(ref bool finishFlag)
        {
            this.IncrementFramesAndCheckTrackersState();
            switch (this.situation)
            {
                case Enums.Situation.AwaitToRollTheDice:
                    MessageLogger.LogMessage(string.Format("{0} turn. Roll the dice!", this.player.ToString()));
                    this.ChangeStateUponDiceDetection();
                    break;
                case Enums.Situation.AwaitForReaction:
                    MessageLogger.LogMessage(string.Format("{0} turn. Move {1} squares!", this.player.ToString(),
                        this.dicePipsNumber));
                    this.ChangeStateUponMovementFinishDetection(ref finishFlag);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        

        private void ChangeStateUponMovementFinishDetection(ref bool finishFlag)
        {
            if (this.gamePawnsDetectingService.DetectPawnOnExpectedField(ref this.dicePipsNumber, this.player)) //validate that player has finished his movement!
            {
                if (this.board.PawnsList.FindIndex(v => v.SquareNumber == Constants.NumberOfFields / 2) >= 0)
                {
                    finishFlag = true;
                    MessageLogger.LogMessage(string.Format("{0} won!", this.player.ToString()));
                }
                this.gamePawnsDetectingService.PawnDetectionAfterMovementFramesAmount = 0;
                this.situation = Enums.Situation.AwaitToRollTheDice;
                this.ChangeTurn();
            }
        }

        /// <summary>
        ///     Changes state if dice have been detected long enough to ensure that the result is not affected by movement
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
            if (this.player == Enums.Player.Human)
                this.player = Enums.Player.Robot;
            else if (this.player == Enums.Player.Robot)
                this.player = Enums.Player.Human;
            else
                throw new ArgumentOutOfRangeException();
        }
    }
}