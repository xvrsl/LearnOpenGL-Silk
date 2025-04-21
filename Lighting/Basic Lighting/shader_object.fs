#version 330 core
out vec4 FragColor;
  
uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 viewPos;

in vec3 Normal;
in vec3 Position;


void main()
{
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;

    vec3 normal = normalize(Normal);
    vec3 lightDir = normalize(lightPos - Position);
    float diff = max(dot(normal,lightDir), 0.0);
    vec3 diffuse = diff*lightColor;


    float specularStrength = 0.5;
    int shininess = 32; 
    vec3 viewDir = normalize(viewPos - Position);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir,reflectDir),0.0),shininess);
    vec3 specular = specularStrength * spec * lightColor;

    vec3 result = (ambient+diffuse+specular) * objectColor;
    FragColor = vec4(result, 1.0);
}