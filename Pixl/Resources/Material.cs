using Veldrid;
using Veldrid.SPIRV;

namespace Pixl;

public class Material : GraphicsResource
{
    private readonly VertexShader _vertexShader;
    private readonly FragmentShader _fragmentShader;
    private readonly VertexLayoutDescription[] _vertexLayouts;
    private readonly List<ResourceLayoutElementDescription> _workingElementList = new();
    private readonly List<BindableResource> _workingResourceList = new();

    private ResourceLayout? _vertexLayout;
    private ResourceLayout? _fragmentLayout;
    private Veldrid.Shader[]? _shaders;

    internal Material(VertexShader vertexShader, FragmentShader fragmentShader, Property[] vertexProperties, Property[] fragmentProperties)
    {
        _vertexShader = vertexShader;
        _fragmentShader = fragmentShader;
        VertexProperties = vertexProperties;
        FragmentProperties = fragmentProperties;
        _vertexLayouts = new VertexLayoutDescription[] { vertexShader.VertexLayoutDescription };
        MainTextureProperty = fragmentProperties.FirstOrDefault(x => x.Descriptor.Type == PropertyType.Texture2d && x.Name.Equals("main", StringComparison.OrdinalIgnoreCase));
    }

    public bool DepthTestEnabled { get; set; } = true;
    public bool ClipRectEnabled { get; set; } = false;
    public IReadOnlyList<Property> VertexProperties { get; }
    public IReadOnlyList<Property> FragmentProperties { get; }
    public Property? MainTextureProperty { get; }

    internal uint VertexStride => _vertexShader.VertexLayoutDescription.Stride;

    private IEnumerable<Property> LocalProperties => Properties.Where(x => x.Scope == PropertyScope.Local);
    private IEnumerable<Property> Properties => VertexProperties.Concat(FragmentProperties);

    internal static Material CreateDefault(Files files, Property worldToClipMatrix)
    {
        var vertexHandle = FileHandle.CreateInternal("default.vert");
        var fragmentHandle = FileHandle.CreateInternal("default.frag");

        var vertexShader = new VertexShader<PositionTexColorVertex>(files, vertexHandle);
        var fragmentShader = new FragmentShader(files, fragmentHandle);

        var vertexProperties = new Property[]
        { 
            worldToClipMatrix
        };

        var fragmentProperties = new Property[]
        {
            new Property("Main", PropertyScope.Local, PropertyDescriptor.CreateTexture2d())
        };

        return new Material(vertexShader, fragmentShader, vertexProperties, fragmentProperties)
        {
            Name = "DefaultMaterial"
        };
    }

    internal static Material CreateError(Files files, Property worldToClipMatrix)
    {
        var vertexHandle = FileHandle.CreateInternal("error.vert");
        var fragmentHandle = FileHandle.CreateInternal("error.frag");

        var vertexShader = new VertexShader<PositionVertex>(files, vertexHandle);
        var fragmentShader = new FragmentShader(files, fragmentHandle);

        var vertexProperties = new Property[]
        {
            worldToClipMatrix
        };

        return new Material(vertexShader, fragmentShader, vertexProperties, Array.Empty<Property>())
        {
            Name = "ErrorMaterial"
        };
    }

    internal static Material CreateGui(Files files, Property worldToClipMatrix)
    {
        var vertexHandle = FileHandle.CreateInternal("gui.vert");
        var fragmentHandle = FileHandle.CreateInternal("gui.frag");

        var vertexShader = new VertexShader<GuiVertex>(files, vertexHandle);
        var fragmentShader = new FragmentShader(files, fragmentHandle);

        var vertexProperties = new Property[]
        {
            worldToClipMatrix
        };

        var fragmentProperties = new Property[]
        {
            new Property("Main", PropertyScope.Local, PropertyDescriptor.CreateTexture2d())
        };

        var material = new Material(vertexShader, fragmentShader, vertexProperties, fragmentProperties)
        {
            Name = "GuiMaterial",
            DepthTestEnabled = false,
            ClipRectEnabled = true
        };
        return material;
    }

    internal Pipeline CreatePipeline(Graphics graphics, Framebuffer framebuffer)
    {
        var rasterizerState = new RasterizerStateDescription
        {
            CullMode = FaceCullMode.None,
            FillMode = PolygonFillMode.Solid,
            FrontFace = FrontFace.Clockwise,
            DepthClipEnabled = DepthTestEnabled,
            ScissorTestEnabled = ClipRectEnabled
        };

        var shaderSet = new ShaderSetDescription
        {
            Shaders = _shaders,
            VertexLayouts = _vertexLayouts
        };

        var pipelineDescription = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = new DepthStencilStateDescription(false, false, ComparisonKind.Always),
            RasterizerState = rasterizerState,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = GetResourceLayouts().ToArray(),
            ShaderSet = shaderSet,
            Outputs = framebuffer.OutputDescription
        };

        var factory = graphics.ResourceFactory;
        var pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        return pipeline;
    }

    internal IEnumerable<ResourceLayout> GetResourceLayouts()
    {
        if (_vertexLayout != null) yield return _vertexLayout;
        if (_fragmentLayout != null) yield return _fragmentLayout;
    }

    internal IEnumerable<ResourceSet> CreateResourceSets(Graphics graphics)
    {
        // TODO look into caching this result
        // must consider properties being "dirty" after Set is called

        var factory = graphics.ResourceFactory;
        if (_vertexLayout != null)
        {
            var description = CreateResourceSetDescription(VertexProperties, _vertexLayout, graphics);
            yield return factory.CreateResourceSet(description);
        }

        if (_fragmentLayout != null)
        {
            var description = CreateResourceSetDescription(FragmentProperties, _fragmentLayout, graphics);
            yield return factory.CreateResourceSet(description);
        }
    }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        var factory = graphics.ResourceFactory;

        // layouts
        var vertexResourceLayoutDescription = CreateResourceLayoutDescription(VertexProperties, ShaderStages.Vertex);
        _vertexLayout = factory.CreateResourceLayout(vertexResourceLayoutDescription);

        var fragmentResourceLayoutDescription = CreateResourceLayoutDescription(FragmentProperties, ShaderStages.Fragment);
        _fragmentLayout = factory.CreateResourceLayout(fragmentResourceLayoutDescription);

        // local properties
        foreach (var property in LocalProperties) property.Create(graphics);

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

        // layouts
        _vertexLayout?.Dispose();
        _vertexLayout = null;

        _fragmentLayout?.Dispose();
        _fragmentLayout = null;

        // local properties
        foreach (var property in LocalProperties) property.Destroy();

        if (_shaders != null)
        {
            foreach (var shader in _shaders) shader.Dispose();
            _shaders = null;
        }
    }

    private ResourceLayoutDescription CreateResourceLayoutDescription(IEnumerable<Property> properties, ShaderStages stage)
    {
        _workingElementList.Clear();
        foreach (var property in properties)
        {
            var elements = property.GetResourceLayoutElementDescriptions(stage);
            if (elements.Length == 0) continue;
            _workingElementList.AddRange(elements);
        }
        var description = new ResourceLayoutDescription(_workingElementList.ToArray());
        return description;
    }

    private ResourceSetDescription CreateResourceSetDescription(IEnumerable<Property> properties, ResourceLayout layout, Graphics graphics)
    {
        _workingResourceList.Clear();
        foreach (var property in properties)
        {
            var resources = property.GetBindableResources(graphics);
            _workingResourceList.AddRange(resources);
        }
        var description = new ResourceSetDescription(layout, _workingResourceList.ToArray());
        return description;
    }
}
