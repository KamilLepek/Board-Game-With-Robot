using System;
using System.Drawing;

namespace BoardGameWithRobot.Utilities
{
    internal class Constants
    {
        public const int DistanceFromLastPositionIgnoringMargin = 5;

        public const int MessageFramesDuration = int.MaxValue;
        public const int BlueHueBottomConstraint = 199;
        public const int BlueHueTopConstraint = 240;
        public const double ColorRadiusDetectingFactor = 0.8;
        public const int DefaultCurveSizeIgnoringMargin = 2000;
        public const double CurveDetectingFactor = 0.02;
        public const string WindowName = "Board Game";

        /// <summary>
        ///     We ignore squares with bigger radius than this
        /// </summary>
        public const int SquareRadiusMargin = 100;

        #region Gaussian blur

        public static Size KSize = new Size(5, 5);
        public const int GaussianX = 0;

        #endregion

        #region Canny

        public const int ThresholdLow = 5;
        public const int ThresholdHigh = 50;
        public const int Aperture = 3; // 3 or 5

        #endregion

        #region Tracker constants

        public const int NumberOfTrackers = 4;
        public const int MinimumNumberOfActiveTrackers = 2;
        public const int AmountOfInitFramesToSearchForTrackers = 10;
        public const int MaxFrameAmountTrackerNotDetectedToBecomeInactive = 150;
        public const int TrackerCurveSizeIgnoringMargin = 5000;

        #endregion

        #region Dice constants

        public const int DiceContourSizeTopConstraint = 900;
        public const int DiceContourSizeBottomConstraint = 600;
        public const int DiceSquareRadiusConstraint = 25;
        public const int DiceFramesDetectedAcceptanceMargin = 40;
        public const int DicePipBottomSizeConstraint = 750;
        public const int DicePipTopSizeConstraint = 2000;
        public const int DiceBinaryPoint = 125;
        public const int DiceResizingFactor = 10;
        public const int DiceFramesIgnoredInPipsDetection = 15;
        public const int IgnoredDistanceBetweenPips = 5;

        #endregion

        #region Field constants

        public const int AmountOfInitFramesToSearchForFields = 20;
        public const int NumberOfFields = 20;
        public const int FieldSizeIgnoringMargin = 10000;

        #endregion

        #region Pawn constants

        public const int NumberOfPawns = 2;
        public const int AmountOfInitFramesToSearchForPawns = 20;
        public const int PawnContourSizeBottomConstraint = 1000;
        public const int PawnContourSizeTopConstraint = 3000;
        public const int PawnDistanceFromFieldMargin = 50;
        public const int AmountOfFramesToDeterminePawn = 10;

        #endregion

        #region Camera related constants

        public const string Quality = "FHD"; //FHD, HD, 480
        public const int CameraId = 0;
        public const int MinimumDelayBetweenFrames = 1; //ms

        #endregion
    }
}