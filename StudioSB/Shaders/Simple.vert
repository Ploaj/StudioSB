#version 330

in vec3 Position;

uniform mat4 mvp;
uniform mat4 transform;

void main()
{
    vec4 position = transform * vec4(inPosition, 1);

    gl_Position = mvp * vec4(position.xyz, 1);
}