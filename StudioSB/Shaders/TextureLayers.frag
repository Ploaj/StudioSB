#version 330

uniform sampler2D colMap;

uniform int hasCol2Map;
uniform sampler2D col2Map;

uniform sampler2D prmMap;
uniform sampler2D norMap;
uniform sampler2D emiMap;
uniform sampler2D emi2Map;
uniform sampler2D bakeLitMap;
uniform sampler2D gaoMap;
uniform sampler2D inkNorMap;

uniform int hasDifCubeMap;
uniform samplerCube difCubeMap;

uniform int hasDiffuse;
uniform sampler2D difMap;

uniform int hasDiffuse2;
uniform sampler2D dif2Map;

uniform int hasDiffuse3;
uniform sampler2D dif3Map;

uniform samplerCube diffusePbrCube;
uniform samplerCube specularPbrCube;


vec4 CustomVector0;
vec4 CustomVector3;
vec4 CustomVector6;
vec4 CustomVector8;
vec4 CustomVector11;
vec4 CustomVector13;
vec4 CustomVector14;
vec3 CustomVector18;
vec4 CustomVector30;
vec4 CustomVector31;
vec4 CustomVector32;
vec4 CustomVector42;
vec4 CustomVector47;
vec4 CustomVector44;
vec4 CustomVector45;

vec4 vec4Param;

int CustomBoolean1;
int CustomBoolean2;
int CustomBoolean3;
int CustomBoolean4;
int CustomBoolean9;
int CustomBoolean11;

float CustomFloat1;
float CustomFloat4;
float CustomFloat8;
float CustomFloat10;
float CustomFloat19;


uniform float floatTestParam;

uniform int emissionOverride;

uniform int renderExperimental;

vec3 Blend(vec4 a, vec4 b)
{
    return mix(a.rgb, b.rgb, b.a);
}

vec2 TransformUv(vec2 uv, vec4 transform)
{
    vec2 translate = vec2(-1.0 * transform.z, transform.w);

    // TODO: Does this affect all layers?
    // if (CustomBoolean5 == 1 || CustomBoolean6 == 1)
    //     translate *= currentFrame / 60.0;

    vec2 scale = transform.xy;
    vec2 result = (uv + translate) * scale;

    if (renderExperimental == 1)
    {
        // dUdV Map.
        // Remap [0,1] to [-1,1].
        vec2 textureOffset = texture(norMap, uv*2).xy * 2 - 1; 
        result += (textureOffset) * CustomFloat4;
    }

    return result;
}

vec4 GetEmissionColor(vec2 uv1, vec2 uv2, vec4 transform1, vec4 transform2)
{
    vec2 uvLayer1 = TransformUv(uv1, transform1);
    vec4 emissionColor = texture(emiMap, uvLayer1).rgba;

    vec2 uvLayer2 = TransformUv(uv2, transform2);
    vec4 emission2Color = texture(emi2Map, uvLayer2).rgba;

    // TODO: Blending?
    vec4 result = emissionColor;
    result.rgb += emission2Color.rgb;
    return result;
}

vec4 GetAlbedoColor(vec2 uv1, vec2 uv2, vec2 uv3, vec3 R, vec4 transform1, vec4 transform2, vec4 transform3, vec4 colorSet5)
{
    // HACK: The default albedo color is white, which won't work with emission.
    if (emissionOverride == 1)
        return vec4(0, 0, 0, 1);

    vec2 uvLayer1 = TransformUv(uv1, transform1);
    vec2 uvLayer2 = TransformUv(uv2, transform2);
    vec2 uvLayer3 = TransformUv(uv3, transform3);

    vec4 albedoColor = texture(colMap, uvLayer1).rgba;
    vec4 albedoColor2 = texture(col2Map, uvLayer2).rgba;

    vec4 diffuseColor = texture(difMap, uvLayer1).rgba;
    vec4 diffuse2Color = texture(dif2Map, uvLayer2).rgba;
    vec4 diffuse3Color = texture(dif3Map, uvLayer3).rgba;

    // TODO: Vertex color alpha is used for some stages.
    if (hasCol2Map == 1)
        albedoColor.rgb = Blend(albedoColor, albedoColor2);

    // Materials won't have col and diffuse cubemaps.
    if (hasDifCubeMap == 1)
        albedoColor.rgb = texture(difCubeMap, R).rgb;

    if (hasDiffuse == 1)
        albedoColor.rgb = Blend(albedoColor, diffuseColor);
    if (hasDiffuse2 == 1)
        albedoColor.rgb += diffuse2Color.rgb;
    // TODO: Is the blending always additive?
    if (hasDiffuse3 == 1)
        albedoColor.rgb += diffuse3Color.rgb;

    return albedoColor;
}
