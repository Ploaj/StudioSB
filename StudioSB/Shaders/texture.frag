#version 330

in vec2 texCoord;

uniform sampler2D image;
uniform int isSrgb;

out vec4 fragColor;

// Defined in Gamma.frag.
vec3 GetSrgb(vec3 linear);

void main()
{
    fragColor = vec4(texture(image, vec2(texCoord.x, 1 - texCoord.y)).rgb, 1);
    if (isSrgb == 1)
        fragColor.rgb = GetSrgb(fragColor.rgb);
}
