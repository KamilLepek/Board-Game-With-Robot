namespace BoardGameWithRobot.Utilities
{
    internal static class Enums
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

        public enum RobotControllingState
        {
            Forward,
            Backward,
            Correct,
            Wait,
            Finished
        }
    }
}