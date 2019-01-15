#version 330

in vec3 Position0;

in vec3 Tangent0;
in vec3 Bitangent0;
in vec3 Normal0;

in vec4 colorSet1;
in vec4 colorSet5;

in vec2 bake1;
in vec2 map1;
in vec2 uvSet;
in vec2 uvSet1;

in ivec4 boneIndices;
in vec4 boneWeights;

out vec3 geomN;
out vec3 geomTangent;
out vec3 geomBitangent;
out vec2 geomMap1;
out vec2 geomUvSet;
out vec2 geomUvSet1;
out vec4 geomColorSet1;
out vec4 geomColorSet5;
out vec2 geomBake1;
out vec3 geomPosition;

uniform mat4 mvp;
uniform mat4 transform;

// Sprite sheet animations.
uniform vec4 paramAA;
uniform int paramF1;

uniform Bones
{
    mat4 transforms[200];
} bones;

void main()
{
    // Single bind transform
    vec4 position = transform * vec4(Position0, 1);
    vec4 transformedNormal = transform * vec4(Normal0, 0);

    // Vertex skinning
    if (boneWeights.x != 0) {
        position = vec4(0);
        transformedNormal = vec4(0);

        for (int i = 0; i < 4; i++)
        {
            position += bones.transforms[boneIndices[i]] * vec4(Position0, 1) * boneWeights[i];
            transformedNormal.xyz += (inverse(transpose(bones.transforms[boneIndices[i]])) * vec4(Normal0, 1) * boneWeights[i]).xyz;
        }
    }

    // Assign geometry inputs
    geomN = transformedNormal.xyz;
    geomColorSet1 = colorSet1;
    geomColorSet5 = colorSet5;
    geomBake1 = bake1;
    geomPosition = position.xyz;

    // Sprite sheet uvs.
    geomMap1 = map1;
    if (paramF1 == 1)
        geomMap1 /= paramAA.xy;
    geomUvSet = uvSet;
    geomUvSet1 = uvSet1;

    geomTangent = Tangent0;
    geomBitangent = Bitangent0;

    gl_Position = mvp * vec4(position.xyz, 1);
}
