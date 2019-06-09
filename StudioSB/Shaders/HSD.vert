#version 330

in vec3 POS;
in vec3 NRM;
in vec2 UV0;
in vec4 Clr0;
in vec4 Bone;
in vec4 Weight;

out vec3 vertPosition;
out vec3 N;
out vec2 Tex0;

uniform BoneTransforms
{
    mat4 transforms[200];
} bones2;

uniform vec2 textureScale;
uniform mat4 singleBind;
uniform mat4 mvp;

void main()
{
	vec3 position = (singleBind * vec4(POS, 1)).xyz;
	vec3 normal = (inverse(transpose(singleBind)) * vec4(NRM, 1)).xyz;
	if(Weight.x == 1)
	{
		position = (bones2.transforms[int(Bone.x)] * vec4(position, 1)).xyz;
		normal = (inverse(transpose(bones2.transforms[int(Bone.x)])) * vec4(normal, 1)).xyz;
	}
	Tex0 = UV0 * textureScale;
	N = normal;
	vertPosition = position;
    gl_Position = mvp * vec4(position, 1);
}