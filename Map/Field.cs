using System;
using BoardGameWithRobot.Utilities;
using Emgu.CV;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Map
{
    internal class Field : BoardObject
    {
        public Enums.Player Player { get; }

        public string Label { get; set; }

        public Field(VectorOfPoint curve, Mat image) : base(curve)
        {
            this.Player = this.Center.Y < image.Height / 2 ? Enums.Player.Robot : Enums.Player.Human;
        }

        public void Print(Mat image)
        {
            int manhattanRadius = (int)(this.Radius / Math.Sqrt(2));
            DrawingService.PutSquareOnBoard(image, this.Center, manhattanRadius, true);
            DrawingService.PutTextOnImage(image, this.Center, this.Label);
        }

    }
}
