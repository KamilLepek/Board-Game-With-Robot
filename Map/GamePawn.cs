using BoardGameWithRobot.Utilities;
using Emgu.CV.Util;

namespace BoardGameWithRobot.Map
{
    internal class GamePawn : BoardObject
    {
        public Enums.Player Player { get; }

        public GamePawn(VectorOfPoint curve, Enums.Player pl) : base(curve)
        {
            this.Player = pl;
        }
    }
}
