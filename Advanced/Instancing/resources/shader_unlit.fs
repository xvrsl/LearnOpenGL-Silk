#version 330 core

struct Material {
    vec3 ambient;
    sampler2D texture_diffuse;
    sampler2D specular;
    float shininess;
    sampler2D emission;
    sampler2D texture_diffuse_1;
};
uniform Material material;

out vec4 FragColor;

uniform vec3 viewPos;

in vec2 TexCoords;
in vec3 Normal;
in vec3 Position;

uniform float near;
uniform float far;

void main() {
    FragColor = texture(material.texture_diffuse_1, TexCoords);
    return;
}
