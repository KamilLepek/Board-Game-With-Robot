using System.Windows.Forms;
using BoardGameWithRobot.Controllers;
using Emgu.CV;


namespace BoardGameWithRobot
{
    public class Game
    {
        static void Main(string[] args)
        {
            GameController controller = new GameController();

            if (controller.InitializeGame())
            {
                controller.PlayGame();
                MessageBox.Show("Game has finished!");
                return;
            }
            MessageBox.Show("Game initialization failed");
            CvInvoke.WaitKey();
        }
    }
}
