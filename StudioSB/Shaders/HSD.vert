#version 330

in vec3 POS;
in vec3 NRM;
in vec2 UV0;
in vec4 Clr0;
in vec4 Bone;
in vec4 Weight;

out vec3 vertPosition;
out vec3 normal;
out vec2 tex0;
out vec4 color;
out vec3 bitangent;
out vec3 tangent;

// specular
out vec3 specularPass;

uniform int hasSpecular;
uniform sampler2D specularTex;
uniform vec2 specularScale;
uniform vec4 specularColor;
uniform float glossiness;
uniform vec3 cameraPos;
uniform int enableSpecular;
// end specular

uniform BoneTransforms
{
    mat4 transforms[200];
} bones2;

uniform mat4 singleBind;
uniform mat4 mvp;

vec3 SpecularPass(vec3 N, vec3 V)
{
    // Specular
    float phong = clamp(dot(N, V), 0, 1);

    phong = pow(phong, glossiness);

    vec3 specularTerm = vec3(phong) * specularColor.rgb;

    if (hasSpecular == 1)
        specularTerm *= texture(specularTex, tex0 * specularScale).rgb;

    specularTerm *= enableSpecular;

    return specularTerm;
}

void main()
{
	vec3 position = (singleBind * vec4(POS, 1)).xyz;
	normal = (inverse(transpose(singleBind)) * vec4(NRM, 1)).xyz;
	if(Weight.x == 1)
	{
		position = (bones2.transforms[int(Bone.x)] * vec4(position, 1)).xyz;
		normal = (inverse(transpose(bones2.transforms[int(Bone.x)])) * vec4(normal, 1)).xyz;
	}
	vertPosition = position;
	tex0 = UV0;
	color = Clr0;
	tangent = vec3(0);
	bitangent = vec3(0);
	specularPass = SpecularPass(NRM, normalize((position.xyz - cameraPos)));
    gl_Position = mvp * vec4(position, 1);
}