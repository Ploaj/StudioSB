#version 330

in vec4 Position;

uniform mat4 mvp;

uniform mat4 transform1;
uniform mat4 transform2;

uniform float Size;

void main()
{
	vec3 position = Position.xyz * Size;

	if(Position.w == 1)
	{
		position = (transform2 * vec4(position, 1)).xyz;
	}
	else
	{
		position = (transform1 * vec4(position, 1)).xyz;
	}

    gl_Position = mvp * vec4(position, 1);
}