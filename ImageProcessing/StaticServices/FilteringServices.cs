using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

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
            CvInvoke.Canny(GaussianBlurImage(GrayImage(source)), result, Constants.ThresholdLow, Constants.ThresholdHigh, Constants.Aperture);
            return result;
        }

        public static Mat BgrToBinary(Mat source, int determiner)
        {
            return GrayToBinary(GrayImage(source), determiner);
        }

        public static Mat GrayToBinary(Mat source, int determiner)
        {
            Image<Gray, byte> grayImage = source.ToImage<Gray, byte>();

            for (int i = 0; i < grayImage.Height; i++)
            {
                for (int j = 0; j < grayImage.Width; j++)
                {
                    if (grayImage.Data[i, j, 0] < determiner)
                        grayImage.Data[i, j, 0] = 0;
                    else
                        grayImage.Data[i, j, 0] = 255;
                }
            }
            Mat result = grayImage.Mat.Clone();
            grayImage.Dispose();
            return result;
        }

        /// <summary>
        /// Returns image converted to grayscale
        /// </summary>
        public static Mat GrayImage(Mat source)
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
