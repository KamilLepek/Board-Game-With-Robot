using System.Drawing;

namespace BoardGameWithRobot.Utilities
{
    internal class Constants
    {
        #region Gaussian blur

        public static Size KSize = new Size(5, 5);
        public const int GaussianX = 0;

        #endregion

        #region Canny

        public const int ThresholdLow = 5;
        public const int ThresholdHigh = 50;
        public const int Aperture = 5; // 3 or 5

        #endregion

        #region Tracker constants

        public const int TrackerDistanceDifferenceFromLastPosition = 4;
        public const int NumberOfTrackers = 4;
        public const int MinimumNumberOfActiveTrackers = 3;

        /// <summary>
        /// We ignore trackers with bigger radius than this
        /// </summary>
        public const int BlueTrackerRadiusMargin = 100;

        public const int AmountOfInitFramesToSearchForTrackers = 10;
        public const int MaxFrameAmountTrackerNotDetectedToDelete = 50;

        #endregion

        public const int MessageFramesDuration = 50;
        public const int BlueHueBottomConstraint = 199;
        public const int BlueHueTopConstraint = 240;
        public const double ColorRadiusDetectingFactor = 0.8;
        public const int CurveSizeIgnoringMargin = 2000;
        public const double CurveDetectingFactor = 0.02;
        public const int CameraId = 1;
        public const string WindowName = "Board Game";
        public const int MinimumDelayBetweenFrames = 1; //ms

        public const string Quality = "FHD"; //FHD, HD, 480
    }
}
