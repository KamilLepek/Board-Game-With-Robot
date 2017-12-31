using System;
using System.Drawing;
using BoardGameWithRobot.Utilities;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Map
{
    internal class SquareBoundsCurve
    {
        public SquareBoundsCurve(VectorOfPoint curve)
        {
            this.Curve = curve;
            this.MassCenter = GeometryUtilis.MassCenter(this.Curve);
            this.Radius = GeometryUtilis.CalculateSquareRadius(this.Curve, this.MassCenter);
        }

        public SquareBoundsCurve(Point center, int radius)
        {
            int r = (int)(radius / Math.Sqrt(2));
            Point[] values = new Point[4];
            values[0] = new Point(center.X - r, center.Y - r);
            values[1] = new Point(center.X - r, center.Y + r);
            values[2] = new Point(center.X + r, center.Y + r);
            values[3] = new Point(center.X + r, center.Y - r);
            this.Curve = new VectorOfPoint(values);
            this.MassCenter = center;
            this.Radius = radius;
        }

        public VectorOfPoint Curve { get; }

        public Point MassCenter { get; }

        /// <summary>
        ///     Half diagonal of bounding square of this curve
        /// </summary>
        public int Radius { get; }
    }
}