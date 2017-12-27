namespace BoardGameWithRobot.Utilities
{
    internal class Enums
    {
        public enum Situation
        {
            AwaitToRollTheDice,
            AwaitForReaction
        }

        public enum Turn
        {
            Human,
            Robot
        }

        public enum TrackerDetectionState
        {
            Active,
            Inactive
        }
    }
}
