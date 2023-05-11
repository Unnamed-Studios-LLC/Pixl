using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Pixl;

public class Material : GraphicsResource
{
    private readonly string _vertexShaderPath;
    private readonly string _fragmentShaderPath;
    private readonly VertexLayoutDescription[] _vertexLayouts;

    private Shader[]? _shaders;
    private readonly Dictionary<Framebuffer, Pipeline> _pipelineCache = new();

    internal Material(string vertexShaderPath, string fragmentShaderPath, VertexLayoutDescription[] vertexLayouts)
    {
        _vertexShaderPath = vertexShaderPath;
        _fragmentShaderPath = fragmentShaderPath;
        _vertexLayouts = vertexLayouts;
    }

    internal static Material CreateDefault(Game game)
    {
        var vertexPath = Path.Combine(game.InternalAssetsPath, "default.vert");
        var fragmentPath = Path.Combine(game.InternalAssetsPath, "default.frag");

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("Color", VertexElementFormat.Byte4_Norm, VertexElementSemantic.TextureCoordinate)
        );

        var vertexLayouts = new VertexLayoutDescription[]
        {
            vertexLayout
        };

        return new Material(vertexPath, fragmentPath, vertexLayouts);
    }

    internal static Material CreateError(Game game)
    {
        var vertexPath = Path.Combine(game.InternalAssetsPath, "error.vert");
        var fragmentPath = Path.Combine(game.InternalAssetsPath, "error.frag");

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.TextureCoordinate)
        );

        var vertexLayouts = new VertexLayoutDescription[]
        {
            vertexLayout
        };

        return new Material(vertexPath, fragmentPath, vertexLayouts);
    }

    internal Pipeline GetPipeline(Framebuffer framebuffer)
    {
        // check cache first
        if (_pipelineCache.TryGetValue(framebuffer, out var pipeline)) return pipeline;

        var rasterizerState = new RasterizerStateDescription
        {
            CullMode = FaceCullMode.Back,
            FillMode = PolygonFillMode.Solid,
            FrontFace = FrontFace.Clockwise,
            DepthClipEnabled = true,
            ScissorTestEnabled = false
        };

        var shaderSet = new ShaderSetDescription
        {
            Shaders = _shaders,
            VertexLayouts = _vertexLayouts
        };

        var pipelineDescription = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.Disabled,
            RasterizerState = rasterizerState,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = Array.Empty<ResourceLayout>(),
            ShaderSet = shaderSet,
            Outputs = framebuffer.OutputDescription
        };

        var factory = Game.Current.Graphics.ResourceFactory;
        pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        _pipelineCache.Add(framebuffer, pipeline);
        return pipeline;
    }

    protected override void OnCreate()
    {
        // load shaders
        var vertBytes = File.ReadAllText(_vertexShaderPath);
        var fragBytes = File.ReadAllText(_fragmentShaderPath);
        var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertBytes), "main");
        var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragBytes), "main");

        var factory = Game.Current.Graphics.ResourceFactory;
        _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
    }

    protected override void OnDestroy()
    {
        if (_shaders != null)
        {
            foreach (var shader in _shaders) shader.Dispose();
            _shaders = null;
        }

        foreach (var pipeline in _pipelineCache.Values) pipeline.Dispose();
        _pipelineCache.Clear();
    }
}
