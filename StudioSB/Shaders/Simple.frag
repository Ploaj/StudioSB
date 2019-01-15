#version 330

in vec3 Normal;
in vec2 UV0;

uniform sampler2D colMap;

uniform vec4 paramA3;

out vec4 fragColor;

void main()
{
    fragColor = vec4(paramA3.xyz, 1);//vec4(texture(colMap, UV0).xyz, 1); //vec4(Normal, 1);
}