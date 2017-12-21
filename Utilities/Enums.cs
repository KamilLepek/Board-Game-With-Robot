namespace BoardGameWithRobot.Utilities
{
    internal class Enums
    {
        public enum Situation
        {
            AwaitToRollTheDice,
            DiceIsRolling,
            AwaitForReaction
        }

        public enum Turn
        {
            Human,
            Robot
        }
    }
}
