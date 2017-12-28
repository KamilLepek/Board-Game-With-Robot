using System.Drawing;
using System.Runtime.CompilerServices;
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
        public static void PutTextOnImage(Mat image, Point place, string text)
        {
            CvInvoke.PutText(
                image,
                text,
                place,
                FontFace.HersheyComplex,
                1.0,
                new Bgr(0, 255, 0).MCvScalar);
        }

        public static void PutTextOnMassCenterOfCurve(Mat image, VectorOfPoint approxCurve, string text)
        {
            PutTextOnImage(image, GeometryUtilis.MassCenter(approxCurve), text);
        }

        public static void PutSquareOnBoard(Mat image, Point center, int radius, bool thick = false)
        {
            int thickness = thick ? 2 : 1;
            CvInvoke.Rectangle(
                image,
                new Rectangle(center.X - radius, center.Y - radius, 2 * radius + 1, 2 * radius + 1),
                new Bgr(0, 255, 0).MCvScalar,
                thickness,
                LineType.EightConnected,
                0);
        }
    }
}
