using System;

namespace Space_Wars.Content.Main
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new Engine();
                game.Run();
        }
    }
}
