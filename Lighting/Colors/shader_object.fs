#version 330 core
out vec4 FragColor;

uniform float ambientStrength;
uniform vec3 ambientColor;
uniform vec3 objectColor;
uniform vec3 lightColor;

in vec3 Normal;

void main()
{
    vec3 ambient = ambientStrength * ambientColor;
    vec3 result = ambient * objectColor;
    FragColor = vec4(result,1.0f);
}