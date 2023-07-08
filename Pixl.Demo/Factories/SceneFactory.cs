namespace Pixl.Demo.Factories
{
    internal static class SceneFactory
    {
        public static void LoadGameScene(this Scene scene)
        {
            scene.AddSystem<RenderingSystem>();
        }
    }
}