#version 330

in vec2 texCoord;

uniform sampler2D image;
uniform int isSrgb;
uniform int monoChannel;
uniform int enableR;
uniform int enableG;
uniform int enableB;
uniform int enableA;
uniform int LOD;

out vec4 fragColor;

// Defined in Gamma.frag.
vec3 GetSrgb(vec3 linear);

void main()
{
    fragColor = textureLod(image, vec2(texCoord.x, 1 - texCoord.y), LOD);

	if(enableR == 0)
		fragColor.r = 0;
	if(enableG == 0)
		fragColor.g = 0;
	if(enableB == 0)
		fragColor.b = 0;
	if(enableA == 0)
		fragColor.a = 1;

	if(enableR == 1 && monoChannel == 1)
		fragColor = vec4(fragColor.r, fragColor.r, fragColor.r, 1);

	if(enableG == 1 && monoChannel == 1)
		fragColor = vec4(fragColor.g, fragColor.g, fragColor.g, 1);

	if(enableB == 1 && monoChannel == 1)
		fragColor = vec4(fragColor.b, fragColor.b, fragColor.b, 1);

	if(enableA == 1 && monoChannel == 1)
		fragColor = vec4(fragColor.a, fragColor.a, fragColor.a, 1);

    if (isSrgb == 1)
        fragColor.rgb = GetSrgb(fragColor.rgb);
}
