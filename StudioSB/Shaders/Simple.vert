#version 330

in vec3 inPosition;
in vec3 inNormal;
in vec2 inUV;

uniform mat4 mvp;
uniform mat4 transform;

out vec3 Normal;
out vec2 UV0;

void main()
{
    vec4 position = transform * vec4(inPosition, 1);
    vec4 transformedNormal = transform * vec4(inNormal, 0);
	
	Normal = transformedNormal.xyz * 0.5 + 0.5;
	UV0 = inUV;

    gl_Position = mvp * vec4(position.xyz, 1);
}