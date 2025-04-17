#version 330 core
out vec4 FragColor;

in vec2 TexCoord;
in vec4 ourColor;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform float texture1Visibility;

void main()
{
    //FragColor = texture(texture0, TexCoord) ;
    vec2 tex1Coord = vec2(TexCoord.x,TexCoord.y);
    //vec2 tex1Coord = TexCoord;
    //FragColor = mix(texture(texture0, TexCoord), texture(texture1, tex1Coord), texture1Visibility)*ourColor;
    FragColor = mix(texture(texture0, TexCoord), texture(texture1, tex1Coord), texture1Visibility);
} 