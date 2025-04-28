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
    vec3 direction;
    float cutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

uniform Light light;

out vec4 FragColor;

uniform vec3 viewPos;

in vec2 TexCoords;
in vec3 Normal;
in vec3 Position;

void main() {

    vec3 normal = normalize(Normal);

    vec3 delta = light.position - Position;
    vec3 lightDir = normalize(delta);
    float distance = length(delta);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * distance * distance);
    //vec3 lightDir = normalize(-light.direction);

    float theta = dot(lightDir, normalize(-light.direction));
    float cutOffFactor = 0;
    if(theta > light.cutOff) {
        cutOffFactor = 1;
    }
    vec3 viewDir = normalize(viewPos - Position);
    vec3 reflectDir = reflect(-lightDir, normal);

    float diff = max(dot(normal, lightDir), 0.0);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    vec3 albedoTex = vec3(texture(material.diffuse, TexCoords));
    vec3 specularTex = vec3(texture(material.specular, TexCoords));
    vec3 ambient = light.ambient * albedoTex;
    vec3 diffuse = light.diffuse * diff * albedoTex * cutOffFactor;
    vec3 specular = light.specular * spec * specularTex * cutOffFactor;

    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    vec3 emission = vec3(texture(material.emission, TexCoords));

    vec3 result = ambient + diffuse + specular + emission;
    FragColor = vec4(result, 1.0);
    //FragColor = vec4(vec3(theta), 1.0);
    FragColor = vec4(vec3(lightDir), 1.0);

}