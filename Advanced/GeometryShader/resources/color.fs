#version 330 core
out vec4 FragColor;
uniform vec4 color;
in vec4 fColor;

void main() {
    FragColor = fColor; //* fColor;
}