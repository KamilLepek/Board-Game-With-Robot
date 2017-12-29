using BoardGameWithRobot.Utilities;

namespace BoardGameWithRobot.Map
{
    internal class GamePawn : BoardObject
    {
        public GamePawn(SquareBoundsCurve boundary, Enums.Player pl) : base(boundary)
        {
            this.Player = pl;
        }

        public Enums.Player Player { get; }
    }
}