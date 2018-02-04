using System.Drawing;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.Map
{
    internal class Robot
    {

        public SquareBoundsCurve FrontCircle { private get; set; }

        public SquareBoundsCurve BackCircle { private get; set; }

        public readonly Point GlobalInitMassCenterOfBackCircle;

        public Point FrontVector => GeometryUtilis.DiffrenceVector(this.FrontCircle.MassCenter, this.BackCircle.MassCenter);

        public Robot(SquareBoundsCurve frontCircle, SquareBoundsCurve backCircle)
        {
            this.FrontCircle = frontCircle;
            this.BackCircle = backCircle;
            this.GlobalInitMassCenterOfBackCircle = backCircle.MassCenter;
        }

        public void PrintTrackersOnBoard(Mat image)
        {
            DrawingService.PutSquareOnImage(image, this.FrontCircle, true);
            DrawingService.PutSquareOnImage(image, this.BackCircle, true);
        }

        public void PrintFrontLineOnBoard(Mat image)
        {
            DrawingService.PutLineOnImage(image, this.BackCircle.MassCenter, this.FrontCircle.MassCenter, true);
        }
    }
}
