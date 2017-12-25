using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Utilities
{
    /// <summary>
    /// Class which maintains drawing text and shapes on image
    /// </summary>
    internal static class DrawingService
    {
        public static void PutTextOnMassCenterOfCurve(Mat image, VectorOfPoint approxCurve, string text)
        {
            CvInvoke.PutText(
                image, 
                text, 
                GeometryUtilis.MassCenter(approxCurve), 
                FontFace.HersheyComplex, 
                1.0,
                new Bgr(0, 255, 0).MCvScalar);
        }

        public static void PutSquareOnBoard(Mat image, Point center, int radius)
        {
            CvInvoke.Rectangle(
                image,
                new Rectangle(center.X - radius, center.Y - radius, 2 * radius + 1, 2 * radius + 1),
                new Bgr(0, 255, 0).MCvScalar,
                1,
                LineType.EightConnected,
                0);
        }
    }
}
