using Pixl;
using Pixl.Demo;
using Pixl.Win;
using Pixl.Win.Player;

try
{
    WinWindow window = new("Pixl Game", new Int2(1000, 800));
    WinGamePlayer player = new(window);

    var worldToClipMatrix = SharedProperty.Create(PropertyDescriptor.CreateStandard(PropertyType.Mat4));
    var defaultMaterial = Material.CreateDefault(player.InternalAssetsPath, worldToClipMatrix);
    var errorMaterial = Material.CreateError(player.InternalAssetsPath, worldToClipMatrix);

    Resources resources = new(
        worldToClipMatrix,
        defaultMaterial,
        errorMaterial
    );

    var graphics = new Graphics();
    graphics.Start(resources, window, GraphicsApi.DirectX);

    var game = new Game(resources, graphics, player, new Entry());

    var gameThread = new Thread(() => runGame(window, game, graphics));
    gameThread.Start();

    window.Run();
    gameThread.Join();

    graphics.Stop(resources);

    return player.ExitCode;
}
catch (Exception e)
{
    File.WriteAllText("crash.log", e.ToString());
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