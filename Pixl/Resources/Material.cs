using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Pixl;

public class Material : GraphicsResource
{
    private readonly VertexShader _vertexShader;
    private readonly FragmentShader _fragmentShader;
    private readonly PropertyLayout _vertexPropertyLayout;
    private readonly PropertyLayout _fragmentPropertyLayout;
    private readonly VertexLayoutDescription[] _vertexLayouts;

    private ResourceLayout[]? _resourceLayouts;
    private PropertyResource[]? _vertexResources;
    private ResourceSet? _vertexResourceSet;
    private Veldrid.Shader[]? _shaders;

    internal Material(VertexShader vertexShader, FragmentShader fragmentShader, PropertyLayout vertexPropertyLayout, PropertyLayout fragmentPropertyLayout)
    {
        _vertexShader = vertexShader;
        _fragmentShader = fragmentShader;
        _vertexPropertyLayout = vertexPropertyLayout;
        _fragmentPropertyLayout = fragmentPropertyLayout;
        _vertexLayouts = new VertexLayoutDescription[] { vertexShader.VertexLayoutDescription };
    }

    internal uint VertexStride => _vertexShader.VertexLayoutDescription.Stride;

    internal static Material CreateDefault(string internalAssetsPath, SharedProperty worldToClipMatrix)
    {
        var vertexPath = Path.Combine(internalAssetsPath, "default.vert");
        var fragmentPath = Path.Combine(internalAssetsPath, "default.frag");

        var vertexShader = new VertexShader<PositionColorVertex>(vertexPath);
        var fragmentShader = new FragmentShader(fragmentPath);

        var vertexPropertyLayout = new PropertyLayout(
            new PropertySlot[] { new PropertySlot("Camera", PropertyScope.Shared) },
            Array.Empty<PropertyDescriptor>(),
            new SharedProperty[] { worldToClipMatrix }
        );

        return new Material(vertexShader, fragmentShader, vertexPropertyLayout, PropertyLayout.Empty);
    }

    internal static Material CreateError(string internalAssetsPath, SharedProperty worldToClipMatrix)
    {
        var vertexPath = Path.Combine(internalAssetsPath, "error.vert");
        var fragmentPath = Path.Combine(internalAssetsPath, "error.frag");

        var vertexShader = new VertexShader<PositionVertex>(vertexPath);
        var fragmentShader = new FragmentShader(fragmentPath);

        var vertexPropertyLayout = new PropertyLayout(
            new PropertySlot[] { new PropertySlot("Camera", PropertyScope.Shared) },
            Array.Empty<PropertyDescriptor>(),
            new SharedProperty[] { worldToClipMatrix }
        );

        return new Material(vertexShader, fragmentShader, vertexPropertyLayout, PropertyLayout.Empty);
    }

    internal Pipeline CreatePipeline(Graphics graphics, Framebuffer framebuffer)
    {
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
            ResourceLayouts = _resourceLayouts,
            ShaderSet = shaderSet,
            Outputs = framebuffer.OutputDescription
        };

        var factory = graphics.ResourceFactory;
        var pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        return pipeline;
    }

    internal IEnumerable<ResourceSet> GetResourceSets()
    {
        yield return _vertexResourceSet ?? throw new Exception("Resource set is null");
    }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        var factory = graphics.ResourceFactory;

        // resource layout
        var resourceLayoutDescription = _vertexPropertyLayout.CreateResourceLayoutDescription(ShaderStages.Vertex);
        var vertexLayout = factory.CreateResourceLayout(resourceLayoutDescription);
        _resourceLayouts = new ResourceLayout[] { vertexLayout };

        // resource sets
        _vertexResources = _vertexPropertyLayout.CreateResources(factory);
        var bindableResources = _vertexResources.Select(x => x.BindableResource).ToArray();
        var resourceSetDescription = new ResourceSetDescription(vertexLayout, bindableResources);
        _vertexResourceSet = factory.CreateResourceSet(resourceSetDescription);

        // load shaders
        var vertBytes = _vertexShader.GetBytes();
        var fragBytes = _fragmentShader.GetBytes();
        var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertBytes, "main");
        var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragBytes, "main");

        _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
    }

    internal override void OnDestroy(Graphics graphics)
    {
        base.OnDestroy(graphics);

        if (_resourceLayouts != null)
        {
            foreach (var layout in _resourceLayouts) layout.Dispose();
            _resourceLayouts = null;
        }

        if (_vertexResources != null)
        {
            foreach (var resource in _vertexResources)
            {
                if (resource.Scope == PropertyScope.Shared) continue;
                resource.Dispose();
            }
        }
        _vertexResources = null;

        _vertexResourceSet?.Dispose();
        _vertexResourceSet = null;

        if (_shaders != null)
        {
            foreach (var shader in _shaders) shader.Dispose();
            _shaders = null;
        }
    }
}
