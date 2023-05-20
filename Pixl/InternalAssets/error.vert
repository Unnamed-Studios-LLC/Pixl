#version 450

layout(set = 0, binding = 0) uniform CameraData {
    mat4 WorldToClipMatrix;
} Camera;

layout(location = 0) in vec3 Position;

void main()
{
    gl_Position = Camera.WorldToClipMatrix * vec4(Position, 1);
}