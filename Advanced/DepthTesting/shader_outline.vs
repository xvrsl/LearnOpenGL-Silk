#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform float offset;
out vec3 Normal;
out vec3 Position;
out vec2 TexCoords;

void main() {
    vec3 pos = aPos + aNormal * offset;
    gl_Position = projection * view * model * vec4(pos,1.0);
    Normal = mat3(transpose(inverse(model))) * aNormal;
    Position = vec3(model * vec4(pos, 1.0));
    TexCoords = aTexCoords;
}