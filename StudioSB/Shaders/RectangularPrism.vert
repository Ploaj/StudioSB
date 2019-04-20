#version 330

in vec4 point;

uniform mat4 mvp;
uniform mat4 transform;

uniform vec3 size;
uniform vec3 offset;

void main()
{
    vec4 position = transform * vec4((point.xyz - 0.5) * size, 1);

    gl_Position = mvp * vec4(position.xyz + offset, 1);
}