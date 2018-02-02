using System.Drawing;
using BoardGameWithRobot.Utilities;
using Emgu.CV;

namespace BoardGameWithRobot.Map
{
    internal class GamePawn : BoardObject
    {
        public GamePawn(SquareBoundsCurve boundary, Enums.Player pl) : base(boundary)
        {
            this.Player = pl;
            this.SquareNumber = 1;
        }

        public Enums.Player Player { get; }

        public int SquareNumber { get; set; }

        public void PrintPawnPlace(Mat image)
        {
            if (this.Player == Enums.Player.Human)
                DrawingService.PutTextOnImage(image, new Point(0, 500),
                    $"Human: H{this.SquareNumber}");
            else
                DrawingService.PutTextOnImage(image, new Point(0, 550),
                    $"Robot: R{this.SquareNumber}");
        }
    }
}