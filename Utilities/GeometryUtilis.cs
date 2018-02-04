using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Utilities
{
    internal static class GeometryUtilis
    {
        public static Point MassCenter(VectorOfPoint approxCurve)
        {
            var moment = CvInvoke.Moments(approxCurve);
            return new Point((int) (moment.M10 / moment.M00), (int) (moment.M01 / moment.M00));
        }

        public static Point DiffrenceVector(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static double NormOfVecotr(Point x)
        {
            return Math.Sqrt(x.X * x.X + x.Y * x.Y);
        }

        public static double DistanceBetweenPoints(Point a, Point b)
        {
            return NormOfVecotr(DiffrenceVector(a, b));
        }

        public static int CalculateSquareRadius(VectorOfPoint approxCurve, Point massCenter)
        {
            return (int) DistanceBetweenPoints(approxCurve[0], massCenter);
        }

        public static Point PointBetweenPoints(Point a, Point b)
        {
            return new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }
    }
}