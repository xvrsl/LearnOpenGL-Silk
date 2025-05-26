#version 330 core

layout (std140) uniform Matrices
{
    mat4 projection;
    mat4 view;
};
uniform mat4 model;
uniform vec4 color;
out vec4 FragColor;

void main()
{
    FragColor = color;
}  