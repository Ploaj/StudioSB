#version 330

in vec4 point;

uniform mat4 mvpMatrix;
uniform mat4 bone;
uniform mat4 parent;
uniform mat4 rotation;
uniform int hasParent;

const float scale = 0.4;

void main()
{
    vec4 position = bone * rotation * vec4(point.xyz * scale, 1);
    if (hasParent == 1)
    {
        if (point.w == 0)
            position = parent * rotation * vec4(point.xyz * scale, 1);
        else
            position = bone * rotation * vec4((point.xyz - vec3(0, 1, 0)) * scale, 1);
    }

    gl_Position = mvpMatrix * vec4(position.xyz, 1);
}
