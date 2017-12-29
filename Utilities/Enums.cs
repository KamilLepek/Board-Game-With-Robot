namespace BoardGameWithRobot.Utilities
{
    internal class Enums
    {
        public enum Player
        {
            Human,
            Robot
        }

        public enum Situation
        {
            AwaitToRollTheDice,
            AwaitForReaction
        }

        public enum TrackerDetectionState
        {
            Active,
            Inactive
        }
    }
}