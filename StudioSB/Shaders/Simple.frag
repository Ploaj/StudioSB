#version 330

in vec3 Normal;
in vec2 UV0;

uniform int hasTexture;
uniform sampler2D tex;

out vec4 fragColor;

void main()
{
	fragColor = vec4(Normal / 2 + vec3(0.5), 1);
	if(hasTexture == 1)
		fragColor = vec4(tex(colMap, UV0).xyz, 1);
}