#version 330

in vec3 Position0;
in vec3 Normal0;
in vec2 map1;

uniform mat4 mvp;
uniform mat4 transform;

out vec3 Normal;
out vec2 UV0;

void main()
{
    // Single bind transform
    vec4 position = transform * vec4(Position0, 1);
    vec4 transformedNormal = transform * vec4(Normal0, 0);
	
	Normal = transformedNormal.xyz * 0.5 + 0.5;
	UV0 = map1;

    gl_Position = mvp * vec4(position.xyz, 1);
}