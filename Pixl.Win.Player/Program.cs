using Pixl;
using Pixl.Demo;
using Pixl.Win;
using Pixl.Win.Player;
using System;
using System.IO;
using System.Threading;

string crashPath = "crash.log";
try
{
    WinWindow window = new("Pixl Game", new Int2(1000, 800));
    WinGamePlayer player = new(window);
    Resources resources = new();
    Graphics graphics = new();

    crashPath = Path.Combine(player.DataPath, crashPath);
    graphics.Start(resources, window, GraphicsApi.DirectX);

    var game = new Game(resources, graphics, player, new Entry());
    var gameThread = new Thread(() => runGame(window, game, graphics));
    gameThread.Start();

    window.Run();

    gameThread.Join();
    graphics.Stop(resources);

    player.Logger.Flush();
    return player.ExitCode;
}
catch (Exception e)
{
    File.WriteAllText(crashPath, e.ToString());
    return 1;
}

static void runGame(WinWindow window, Game game, Graphics graphics)
{
    game.Start();
    while (game.Run())
    {
        graphics.SwapBuffers();
        game.WaitForNextUpdate();
    }
    window.Stop();
    game.Stop();
}