#version 330

in vec3 TexCoords;

out vec4 FragColor;

uniform samplerCube skybox;
uniform int mipLevel;

void main()
{
    FragColor = textureLod(skybox, TexCoords, mipLevel);
}
