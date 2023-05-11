using Pixl;
using Pixl.Demo;
using Pixl.Player.Win;

var player = new WinPlayer(new Int2(1000, 800));
var game = new Game(player, typeof(Entry).Assembly);

var gameThread = new Thread(() => runGame(player, game));
gameThread.Start();

player.Run();
gameThread.Join();

return player.ExitCode;

static void runGame(WinPlayer player, Game game)
{
    game.Start();
    while (game.Run())
    {
        game.WaitForNextUpdate();
    }
    player.Stop();
    game.Stop();
}