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
        public const int Aperture = 3; // 3 or 5

        #endregion

        public const int BlueHueBottomConstraint = 170;
        public const int BlueHueTopConstraint = 280;
        public const double ColorRadiusDetectingFactor = 0.9;
        public const int CurveSizeIgnoringMargin = 2000;
        public const double CurveDetectingFactor = 0.02;
        public const int CameraId = 0;
        public const string WindowName = "Board Game";
        public const int MinimumDelayBetweenFrames = 1; //ms
        public const int DetectorFrameSearchRadius = 5;
        public const int NumberOfFramesWithNoTrackerDetectedToEscalate = 20;

        public const string Quality = "FHD"; //FHD, HD, 480
    }
}
