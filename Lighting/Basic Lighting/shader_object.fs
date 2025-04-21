#version 330 core
out vec4 FragColor;
  
uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos;

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

    vec3 result = (ambient+diffuse) * objectColor;
    FragColor = vec4(result, 1.0);
}