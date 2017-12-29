using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace BoardGameWithRobot.Utilities
{
    /// <summary>
    ///     Class for logging information onto image
    /// </summary>
    internal static class MessageLogger
    {
        private static string message;

        public static int FramesDuration;

        public static void AttachCenteredMessageToImage(Mat image)
        {
            if (message == null)
                return;
            CvInvoke.PutText(image, message,
                new Point((int) (image.Width / 2 * (1 - message.Length / 50f)), image.Height / 20),
                FontFace.HersheyComplex,
                2.0, new Bgr(0, 255, 0).MCvScalar , 3);
            if (FramesDuration-- == 0)
                message = null;
        }

        public static void LogMessage(string msg, int duration = Constants.MessageFramesDuration)
        {
            FramesDuration = duration;
            message = msg;
        }
    }
}