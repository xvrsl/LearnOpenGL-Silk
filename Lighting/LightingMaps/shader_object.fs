#version 330 core
struct Material {
    vec3 ambient;
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
    sampler2D emission;
};
uniform Material material;

struct Light {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
uniform Light light;

out vec4 FragColor;

uniform vec3 viewPos;

in vec2 TexCoords;
in vec3 Normal;
in vec3 Position;

void main() {

    vec3 normal = normalize(Normal);

    vec3 lightDir = normalize(light.position - Position);
    vec3 viewDir = normalize(viewPos - Position);
    vec3 reflectDir = reflect(-lightDir, normal);

    float diff = max(dot(normal, lightDir), 0.0);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    vec3 albedoTex = vec3(texture(material.diffuse, TexCoords));
    vec3 specularTex = vec3(texture(material.specular, TexCoords));
    vec3 ambient = light.ambient * albedoTex;
    vec3 diffuse = light.diffuse * diff * albedoTex;
    vec3 specular = light.specular * spec * specularTex;
    vec3 emission = vec3(texture(material.emission, TexCoords));

    vec3 result = ambient + diffuse + specular + emission;
    FragColor = vec4(result, 1.0);

}