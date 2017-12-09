using BoardGameWithRobot.Controllers;


namespace BoardGameWithRobot
{
    public class Game
    {
        static void Main(string[] args)
        {
            GameController controller = new GameController();

            controller.Initialize();

            while (true)
            {
                controller.PrintFrame();
            }
        }
    }
}
