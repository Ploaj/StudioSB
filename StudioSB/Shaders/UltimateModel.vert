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
in vec2 uvSet2;

in ivec4 boneIndices;
in vec4 boneWeights;

out vec3 geomVertexNormal;
out vec3 geomTangent;
out vec3 geomBitangent;
out vec2 geomMap1;
out vec2 geomUvSet;
out vec2 geomUvSet1;
out vec2 geomUvSet2;
out vec4 geomColorSet1;
out vec4 geomColorSet5;
out vec2 geomBake1;
out vec3 geomPosition;

uniform mat4 mvp;
uniform mat4 transform;

uniform Bones
{
    mat4 transforms[300];
};

uniform vec4 CustomVector0;
uniform vec4 CustomVector3;
uniform vec4 CustomVector6;
uniform vec4 CustomVector8;
uniform vec4 CustomVector11;
uniform vec4 CustomVector13;
uniform vec4 CustomVector14;
uniform vec3 CustomVector18;
uniform vec4 CustomVector30;
uniform vec4 CustomVector31;
uniform vec4 CustomVector32;
uniform vec4 CustomVector42;
uniform vec4 CustomVector47;
uniform vec4 CustomVector44;
uniform vec4 CustomVector45;

uniform vec4 vec4Param;

uniform int CustomBoolean1;
uniform int CustomBoolean2;
uniform int CustomBoolean3;
uniform int CustomBoolean4;
uniform int CustomBoolean9;
uniform int CustomBoolean11;

uniform float CustomFloat1;
uniform float CustomFloat4;
uniform float CustomFloat8;
uniform float CustomFloat10;
uniform float CustomFloat19;

uniform int hasCustomVector11;
uniform int hasCustomVector47;
uniform int hasCustomVector44;
uniform int hasCustomFloat10;

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
            position += transforms[boneIndices[i]] * vec4(Position0, 1) * boneWeights[i];
            transformedNormal.xyz += (inverse(transpose(transforms[boneIndices[i]])) * vec4(Normal0, 1) * boneWeights[i]).xyz;
        }
    }

    // Assign geometry inputs
    geomVertexNormal = transformedNormal.xyz;
    geomColorSet1 = colorSet1;
    geomColorSet5 = colorSet5;
    geomBake1 = bake1;
    geomPosition = position.xyz;

    // Sprite sheet uvs.
    geomMap1 = map1;
    if (CustomBoolean9 == 1)
        geomMap1 /= CustomVector18.xy;

    geomUvSet = uvSet;
    geomUvSet1 = uvSet1;
    geomUvSet2 = uvSet2;

    geomTangent = Tangent0.xyz;
    geomBitangent = Bitangent0.xyz;

    gl_Position = mvp * vec4(position.xyz, 1);
}