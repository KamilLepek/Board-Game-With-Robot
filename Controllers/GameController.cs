﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        private readonly RobotDetectingService robotDetectingService;

        private int dicePipsNumber;

        private Enums.Player player;

        private Enums.Situation currentStateOfGame;

        private bool diceHasBeenSpotted = false;

        private bool diceHasBeenSpottedAndFinishedMovement = false;

        private int diceDetectionFrames = 0;

        public GameController()
        {
            this.board = new Board();
            this.cameraService = new CameraService();
            this.blueSquareTrackingService = new BlueSquareTrackingService(this.cameraService, this.board);
            this.fieldsDetectingService = new FieldsDetectingService(this.cameraService, this.board);
            this.gamePawnsDetectingService = new GamePawnsDetectingService(this.cameraService, this.board);
            this.robotDetectingService = new RobotDetectingService(this.board, this.cameraService);
            this.initializator = new Initializator(this.cameraService, this.blueSquareTrackingService,
                this.fieldsDetectingService, this.gamePawnsDetectingService, this.robotDetectingService, this.board);
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
            if (!this.initializator.InitializeRobotConnection())
                return false;
            this.currentStateOfGame = Enums.Situation.AwaitToRollTheDice;
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
            Task.Factory.StartNew(RobotControllingService.TryToPushForward);
            while (!finishFlag)
            {
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
            }
            RobotControllingService.State = Enums.RobotControllingState.Finished;
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
            //this.board.PrintRobotTrackLineOnBoard(this.cameraService.ActualFrame);
            this.board.PrintRobotTrackersOnBoard(this.cameraService.ActualFrame);
        }

        /// <summary>
        ///     Analyzes and changes actual state of the game
        /// </summary>
        private void AnalyzeAndChangeStateOfGame(ref bool finishFlag)
        {
            this.IncrementFramesAndCheckTrackersState();
            switch (this.currentStateOfGame)
            {
                case Enums.Situation.AwaitToRollTheDice:
                    MessageLogger.LogMessage($"{this.player.ToString()} turn. Roll the dice!");
                    this.ChangeStateUponDiceDetection();
                    break;
                case Enums.Situation.AwaitForReaction:
                    MessageLogger.LogMessage($"{this.player.ToString()} turn. Move {this.dicePipsNumber} squares!");
                    this.ChangeStateUponMovementFinishDetection(ref finishFlag);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeStateUponMovementFinishDetection(ref bool finishFlag)
        {
            if (this.player == Enums.Player.Robot)
            {
                RobotControllingService.RobotTurn = true;
                this.robotDetectingService.DetectRobotDuringGame();
                if (RobotControllingService.State == Enums.RobotControllingState.Wait)
                {
                    RobotControllingService.Angle = GeometryUtilis.AngleBetweenVectors(RobotControllingService.Vector,
                        this.board.MovingSystem.FrontVector);
                    if (this.gamePawnsDetectingService.PawnDetectionAfterMovementFramesAmount == 0)
                        RobotControllingService.State = Enums.RobotControllingState.Forward;
                }
            }
            else
                RobotControllingService.RobotTurn = false;
            if (this.gamePawnsDetectingService.DetectPawnOnExpectedField(ref this.dicePipsNumber, this.player)) //validate that player has finished his movement!
            {
                if (this.board.PawnsList.FindIndex(v => v.SquareNumber == Constants.NumberOfFields / 2) >= 0)
                {
                    finishFlag = true;
                    MessageLogger.LogMessage($"{this.player.ToString()} won!");
                }
                this.gamePawnsDetectingService.PawnDetectionAfterMovementFramesAmount = 0;
                this.currentStateOfGame = Enums.Situation.AwaitToRollTheDice;
                this.ChangeTurn();
                RobotControllingService.State = Enums.RobotControllingState.Wait;
                RobotControllingService.RobotTurn = false;
            }
        }

        /// <summary>
        ///     Changes state if dice have been detected long enough to ensure that the result is not affected by movement
        /// </summary>
        private void ChangeStateUponDiceDetection()
        {
            if (this.diceHasBeenSpotted)
            {
                this.diceDetectionFrames++;
                if (this.diceHasBeenSpottedAndFinishedMovement)
                {
                    this.diceDetectingService.DetectRolledNumber();
                    if (this.diceDetectionFrames == Constants.DiceFramesDetectedAcceptanceMargin)
                    {
                        this.dicePipsNumber = this.diceDetectingService.DetermineNumberAndResetPipList();
                        this.diceDetectionFrames = 0;
                        this.diceDetectingService.IsDiceRegionDefined = false;
                        this.diceHasBeenSpotted = false;
                        this.diceHasBeenSpottedAndFinishedMovement = false;
                        this.currentStateOfGame = Enums.Situation.AwaitForReaction;
                    }
                }
                else
                {
                    if (this.diceDetectionFrames <= 5)
                        return;
                    this.diceDetectionFrames--;
                    this.diceHasBeenSpottedAndFinishedMovement = this.diceDetectingService.DetectDice();
                }
            }
            else
            {
                this.diceHasBeenSpotted = this.diceDetectingService.DetectDice();                    
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