#version 330
in vec3 Position;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    TexCoords = Position;
    vec4 pos = projection * view * vec4(Position, 1.0);
    gl_Position = pos.xyww;
}  