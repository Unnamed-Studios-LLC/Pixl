#version 450

layout(set = 1, binding = 0) uniform texture2D MainTexture;
layout(set = 1, binding = 1) uniform sampler MainSampler;

layout(location = 0) in vec4 in_Color;
layout(location = 1) in vec2 in_TexCoord;

layout(location = 0) out vec4 out_Color;

void main()
{
    out_Color = in_Color * texture(sampler2D(MainTexture, MainSampler), in_TexCoord);
}