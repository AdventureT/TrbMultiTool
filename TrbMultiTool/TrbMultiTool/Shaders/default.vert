#version 330 core

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;

void main()
{
    gl_Position = vec4(Position, 1.0) * model * view * projection;
    Normal = aNormal;
}