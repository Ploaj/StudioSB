#version 330

in vec3 N;
in vec2 Tex0;

out vec4 fragColor;

uniform sampler2D diffuse;


void main()
{
	vec3 normalizedN = vec3(0.5) + N / 2;

	fragColor = texture(diffuse, Tex0);
}