#version 450

layout(set = 0, binding = 0) uniform CameraData {
    mat4 WorldToClipMatrix;
} Camera;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec2 in_TexCoord;
layout(location = 2) in vec4 in_Color;

layout(location = 0) out vec4 out_Color;
layout(location = 1) out vec2 out_TexCoord;

void main()
{
    gl_Position = Camera.WorldToClipMatrix * vec4(in_Position, 1);
    out_TexCoord = in_TexCoord;
    out_Color = in_Color;
}