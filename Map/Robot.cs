using System.Drawing;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.Map
{
    internal class Robot
    {
        public Point GlobalPosition { get; set; }

        public Point FrontVector { get; set; }

        public SquareBoundsCurve FrontCircle { get; set; }

        public Robot(SquareBoundsCurve frontCircle)
        {
            this.FrontCircle = frontCircle;
            this.GlobalPosition = frontCircle.MassCenter;
            this.FrontVector = new Point(-1, 0);
        }

        public void PrintRobotFrontOnBoard(Mat image)
        {
            DrawingService.PutSquareOnImage(image, new SquareBoundsCurve(this.GlobalPosition, this.FrontCircle.Radius));
        }
    }
}
