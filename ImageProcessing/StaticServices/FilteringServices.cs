using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace BoardGameWithRobot.ImageProcessing
{
    /// <summary>
    /// Class uses for providing common image processing services
    /// </summary>
    internal static class FilteringServices
    {
        /// <summary>
        /// Returnes cannied source image
        /// </summary>
        public static Mat CannyImage(Mat source)
        {
            Mat result = new Mat();
            CvInvoke.Canny(GaussianBlurImage(GreyImage(source)), result, Constants.ThresholdLow, Constants.ThresholdHigh, Constants.Aperture);
            return result;
        }

        /// <summary>
        /// Returns image converted to greyscale
        /// </summary>
        public static Mat GreyImage(Mat source)
        {
            Mat result = new Mat();
            CvInvoke.CvtColor(source, result, ColorConversion.Bgr2Gray);
            return result;
        }

        /// <summary>
        /// Returns image blurred by gaussian function
        /// </summary>
        public static Mat GaussianBlurImage(Mat source)
        {
            Mat result = new Mat();
            CvInvoke.GaussianBlur(source, result, Constants.KSize, Constants.GaussianX);
            return result;
        }

        /// <summary>
        /// Returns same image as HSV
        /// </summary>
        public static Mat BGRToHSV(Mat image)
        {
            Mat HSV = new Mat();
            CvInvoke.CvtColor(image, HSV, ColorConversion.Bgr2Hsv);
            return HSV;
        }
    }
}
