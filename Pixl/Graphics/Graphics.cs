using Veldrid;

namespace Pixl;

internal sealed class Graphics
{
    private GraphicsDevice? _device;
    private CommandList? _mainCommands;
    private ResourceFactory? _resourceFactory;

    public CommandList Commands => _mainCommands ?? throw SetupException();
    public GraphicsDevice Device => _device ?? throw SetupException();
    public Framebuffer FrameBuffer => _device?.SwapchainFramebuffer ?? throw SetupException();
    public ResourceFactory ResourceFactory => _resourceFactory ?? throw SetupException();

    public bool Setup => _mainCommands != null;

    public void Start(Resources resources, IPlayer player)
    {
        var options = new GraphicsDeviceOptions
        {
            Debug = true,
            SwapchainDepthFormat = PixelFormat.R32_Float,
            HasMainSwapchain = true,
            PreferDepthRangeZeroToOne = true,
            PreferStandardClipSpaceYDirection = true,
            ResourceBindingModel = ResourceBindingModel.Default,
            SwapchainSrgbFormat = false,
            SyncToVerticalBlank = false
        };

        var size = player.WindowSize;
        var swapchain = new SwapchainDescription
        {
            DepthFormat = PixelFormat.R32_Float,
            SyncToVerticalBlank = false,
            ColorSrgb = false,
            Width = (uint)size.X,
            Height = (uint)size.Y,
            Source = player.SwapchainSource
        };

        var api = player.GraphicsApi;
        _device = api switch
        {
            GraphicsApi.DirectX => CreateDirectXGraphicsDevice(in options, in swapchain),
            GraphicsApi.OpenGlEs => CreateOpenGlEsGraphicsDevice(in options, in swapchain),
            //GraphicsApi.OpenGl => CreateOpenGlGraphicsDevice(in options, in swapchain), TODO
            GraphicsApi.Metal => CreateMetalGraphicsDevice(in options, in swapchain),
            GraphicsApi.Vulkan => CreateVulkanGraphicsDevice(in options, in swapchain),
            _ => throw new Exception($"Unable to create graphics device for {api}")
        };

        // create device resources
        _resourceFactory = _device.ResourceFactory;
        CreateResources(resources);
    }

    public void Stop(Resources resources)
    {
        // destroy device resources
        DestroyResources(resources);
        _resourceFactory = null;

        _device?.Dispose();
        _device = null;
    }

    public void Submit(CommandList commands)
    {
        if (_device == null) throw SetupException();
        _device.SubmitCommands(commands);
    }

    public void SwapBuffers()
    {
        if (_device == null) throw SetupException();
        _device.SwapBuffers();
    }

    public void UpdateWindowSize(Int2 windowSize)
    {
        if (_device == null) throw SetupException();
        if (windowSize.X == _device.SwapchainFramebuffer.Width &&
            windowSize.Y == _device.SwapchainFramebuffer.Height) return;
        _device.ResizeMainWindow((uint)windowSize.X, (uint)windowSize.Y);
    }

    private static GraphicsDevice CreateDirectXGraphicsDevice(in GraphicsDeviceOptions options, in SwapchainDescription swapchain) => GraphicsDevice.CreateD3D11(options, swapchain);
    private static GraphicsDevice CreateOpenGlEsGraphicsDevice(in GraphicsDeviceOptions options, in SwapchainDescription swapchain) => GraphicsDevice.CreateOpenGLES(options, swapchain);
    private static GraphicsDevice CreateMetalGraphicsDevice(in GraphicsDeviceOptions options, in SwapchainDescription swapchain) => GraphicsDevice.CreateMetal(options, swapchain);
    private static GraphicsDevice CreateVulkanGraphicsDevice(in GraphicsDeviceOptions options, in SwapchainDescription swapchain) => GraphicsDevice.CreateVulkan(options, swapchain);

    private static Exception SetupException() => new("Graphics device not setup!");

    private void CreateResources(Resources resources)
    {
        _mainCommands = _resourceFactory?.CreateCommandList();
        foreach (var resource in resources.All.Where(x => x is GraphicsResource).Select(x => (GraphicsResource)x))
        {
            resource.Create();
        }
    }

    private void DestroyResources(Resources resources)
    {
        foreach (var resource in resources.All.Where(x => x is GraphicsResource).Select(x => (GraphicsResource)x))
        {
            resource.Destroy();
        }

        _mainCommands?.Dispose();
        _mainCommands = null;
    }
}
