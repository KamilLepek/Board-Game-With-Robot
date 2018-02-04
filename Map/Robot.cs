using System.Drawing;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.Map
{
    internal class Robot
    {

        public SquareBoundsCurve FrontCircle { private get; set; }

        public SquareBoundsCurve BackCircle { private get; set; }

        public Point GlobalPosition { get; set; }

        public Point FrontVector => GeometryUtilis.DiffrenceVector(this.FrontCircle.MassCenter, this.BackCircle.MassCenter);

        public Point DiffVector => new Point(this.GlobalPosition.X - this.Center.X, this.GlobalPosition.Y - this.Center.Y);

        public Point Center =>
            GeometryUtilis.PointBetweenPoints(this.FrontCircle.MassCenter, this.BackCircle.MassCenter);

        public Robot(SquareBoundsCurve frontCircle, SquareBoundsCurve backCircle)
        {
            this.FrontCircle = frontCircle;
            this.BackCircle = backCircle;
            this.GlobalPosition = new Point(this.Center.X, this.Center.Y);
        }

        public void PrintTrackersOnBoard(Mat image)
        {
            DrawingService.PutSquareOnImage(image,
                new SquareBoundsCurve(
                    new Point(this.FrontCircle.MassCenter.X + this.DiffVector.X,
                        this.FrontCircle.MassCenter.Y + this.DiffVector.Y), this.FrontCircle.Radius), true);
            DrawingService.PutSquareOnImage(image,
                new SquareBoundsCurve(
                    new Point(this.BackCircle.MassCenter.X + this.DiffVector.X,
                        this.BackCircle.MassCenter.Y + this.DiffVector.Y), this.BackCircle.Radius), true);
        }

        public void PrintFrontLineOnBoard(Mat image)
        {
            DrawingService.PutLineOnImage(image, new Point(this.BackCircle.MassCenter.X + this.DiffVector.X,
                this.BackCircle.MassCenter.Y + this.DiffVector.Y), new Point(
                this.FrontCircle.MassCenter.X + this.DiffVector.X,
                this.FrontCircle.MassCenter.Y + this.DiffVector.Y), true);
        }
    }
}
