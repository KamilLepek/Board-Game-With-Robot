using Emgu.CV;
using Emgu.CV.Structure;


namespace BoardGameWithRobot
{
    public class Game
    {
        static void Main(string[] args)
        {
            Image<Bgr, byte> img = new Image<Bgr, byte>(@"G:\image.jpg");
            CvInvoke.Imshow("test", img);
            CvInvoke.WaitKey(0);
        }
    }
}
