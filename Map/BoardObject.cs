namespace BoardGameWithRobot.Map
{
    /// <summary>
    ///     Class which determines every basic object on the board.
    /// </summary>
    internal abstract class BoardObject
    {
        protected BoardObject(SquareBoundsCurve boundary)
        {
            this.Boundary = boundary;
        }

        public SquareBoundsCurve Boundary { get; }
    }
}