using System.Drawing;
using BoardGameWithRobot.Map;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace BoardGameWithRobot.Utilities
{
    /// <summary>
    ///     Class which maintains drawing text and shapes on image
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

        public static void PutSquareOnImage(Mat image, SquareBoundsCurve boundary, bool thick = false,
            double resize = 1)
        {
            int radius = (int) (boundary.Radius * resize);
            int thickness = thick ? 2 : 1;
            CvInvoke.Rectangle(
                image,
                new Rectangle(boundary.MassCenter.X - radius, boundary.MassCenter.Y - radius,
                    2 * radius + 1, 2 * radius + 1),
                new Bgr(0, 255, 0).MCvScalar,
                thickness,
                LineType.EightConnected,
                0);
        }

        public static void PutLineOnImage(Mat image, Point startPoint, Point endPoint, bool thick = false)
        {
            int thickness = thick ? 2 : 1;
            CvInvoke.Line(
                image, 
                startPoint, 
                endPoint, 
                new Bgr(0, 255, 0).MCvScalar, 
                thickness, 
                LineType.EightConnected,
                0);
        }
    }
}