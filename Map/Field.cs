using System;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.Map
{
    internal class Field : BoardObject
    {
        public Field(SquareBoundsCurve boundary, Mat image) : base(boundary)
        {
            this.Player = this.Boundary.MassCenter.Y < image.Height / 2 ? Enums.Player.Robot : Enums.Player.Human;
        }

        public Enums.Player Player { get; }

        public string Label { get; set; }

        public void Print(Mat image)
        {
            DrawingService.PutSquareOnImage(image, this.Boundary, true, 1 / Math.Sqrt(2));
            DrawingService.PutTextOnImage(image, this.Boundary.MassCenter, this.Label);
        }
    }
}