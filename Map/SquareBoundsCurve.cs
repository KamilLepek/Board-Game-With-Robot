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

        public VectorOfPoint Curve { get; }

        public Point MassCenter { get; }

        /// <summary>
        ///     Half diagonal of bounding square of this curve
        /// </summary>
        public int Radius { get; }
    }
}