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

// TODO: Cubemap loading doesn't work yet.
uniform int hasDifCubemap;
uniform samplerCube difCubemap;

uniform int hasDiffuse;
uniform sampler2D difMap;

uniform int hasDiffuse2;
uniform sampler2D dif2Map;

uniform int hasDiffuse3;
uniform sampler2D dif3Map;

uniform samplerCube diffusePbrCube;
uniform samplerCube specularPbrCube;

// UV scrolling animations.
uniform int paramEE;
uniform int paramED;
uniform float currentFrame;

uniform float paramC4;

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
    if (paramEE == 1 || paramED == 1)
        translate *= currentFrame / 60.0;

    vec2 scale = transform.xy;
    vec2 result = (uv + translate) * scale;

    // TODO: du dv map?
    vec2 textureOffset = vec2(1) - texture(norMap, uv).xy;
    textureOffset = textureOffset * 2 - 1; // Remap [0,1] to [-1,1]
    result = result + (textureOffset * paramC4 * renderExperimental);

    return result;
}

vec4 GetEmissionColor(vec2 uv1, vec2 uv2, vec4 transform1, vec4 transform2)
{
    vec2 uvLayer1 = TransformUv(uv1, transform1);
    vec4 emissionColor = texture(emiMap, uvLayer1).rgba;

    vec2 uvLayer2 = TransformUv(uv2, transform2);
    vec4 emission2Color = texture(emi2Map, uvLayer2).rgba;

    // TODO: ???
    emissionColor.rgb += emission2Color.rgb;
    return emissionColor;
}

vec4 GetAlbedoColor(vec2 uv1, vec2 uv2, vec2 uv3, vec4 transform1, vec4 transform2, vec4 transform3, vec4 colorSet5)
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
    vec4 diffuse3Color = texture(dif3Map, uvLayer2).rgba;

    // Vertex color alpha is used for some stages.
    if (hasCol2Map == 1)
        albedoColor.rgb = Blend(albedoColor, albedoColor2 * vec4(vec3(1), colorSet5.a));

    // Materials won't have col and diffuse cubemaps.
    // if (hasDifCubemap == 1)
    //     albedoColor.rgb = texture(difCubemap, uvLayer1).rgb;

    if (hasDiffuse == 1)
        albedoColor.rgb = Blend(albedoColor, diffuseColor);
    if (hasDiffuse2 == 1)
        albedoColor.rgb += diffuse2Color.rgb;
    // TODO: Is the blending always additive?
    if (hasDiffuse3 == 1)
        albedoColor.rgb += diffuse3Color.rgb;

    return albedoColor;
}
