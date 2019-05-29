#version 330

in vec3 N;
in vec3 tangent;
in vec3 bitangent;
in vec2 map1;
in vec2 uvSet;
in vec2 uvSet1;
in vec4 colorSet1;
in vec4 colorSet5;
in vec2 bake1;
in vec3 position;
noperspective in vec3 edgeDistance;

uniform sampler2D colMap;

uniform int hasCol2Map;
uniform sampler2D col2Map;

uniform sampler2D prmMap;
uniform sampler2D norMap;
uniform sampler2D emiMap;
uniform sampler2D emi2Map;
uniform sampler2D bakeLitMap;
uniform sampler2D gaoMap;

uniform int hasInkNorMap;
uniform sampler2D inkNorMap;

// TODO: Cubemap loading doesn't work yet.
uniform int hasDifCubemap;
uniform sampler2D difCubemap;

uniform int hasDiffuse;
uniform sampler2D difMap;

uniform int hasDiffuse2;
uniform sampler2D dif2Map;

uniform int hasDiffuse3;
uniform sampler2D dif3Map;

uniform sampler2D iblLut;

uniform samplerCube diffusePbrCube;
uniform samplerCube specularPbrCube;

uniform int emissionOverride;

uniform int renderDiffuse;
uniform int renderSpecular;
uniform int renderEmission;
uniform int renderRimLighting;
uniform int renderExperimental;

uniform int renderWireframe;
uniform int renderVertexColor;
uniform int renderNormalMaps;

uniform vec4 paramA6;

uniform vec4 paramA3;
uniform vec4 param145;

uniform vec4 paramA5;
uniform vec4 paramA0;
uniform vec4 param98;
uniform vec4 param9B;

uniform int hasParam151;
uniform vec4 param151;

// UV transforms.
uniform vec4 param146;
uniform vec4 param147;
uniform vec4 param9E;

uniform int hasParam156;
uniform vec4 param156;

uniform int hasParam153;
uniform vec4 param153;
uniform vec4 param154;

uniform int paramE9;
uniform int paramEA;

uniform float paramC8;
uniform float paramCA;

uniform float transitionFactor;
uniform int transitionEffect;

uniform vec3 chrLightDir;

uniform mat4 mvp;
uniform vec3 V;

out vec4 fragColor;

const float directLightIntensity = 1.00;
const float iblIntensity = 1.00;

// Matrices for ordered dithering
// https://en.wikipedia.org/wiki/Ordered_dithering
mat4 thresholdMatrix16 = mat4(
0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0, 9.0 / 16.0,
15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0);

// Defined in Wireframe.frag.
float WireframeIntensity(vec3 distanceToEdges);

// Defined in NormalMap.frag.
vec3 GetBumpMapNormal(vec3 N, vec3 tangent, vec3 bitangent, vec4 norColor);

float LambertShading(vec3 N, vec3 V)
{
    float lambert = max(dot(N, V), 0);
    return lambert;
}

// Defined in Gamma.frag.
vec3 GetSrgb(vec3 linear);

vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

// GGX calculations adapted from https://learnopengl.com/PBR/IBL/Specular-IBL
float Ggx(vec3 N, vec3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float nDotH = max(dot(N, H), 0.0);
    float nDotH2 = nDotH * nDotH;

    float numerator = a2;
    float denominator = (nDotH2 * (a2 - 1.0) + 1.0);
    denominator = 3.14159 * denominator * denominator;

    return numerator / denominator;
}

// Code adapted from equations listed here:
// http://graphicrants.blogspot.com/2013/08/specular-brdf-reference.html
float GgxAnisotropic(vec3 N, vec3 H, vec3 tangent, vec3 bitangent, float roughX, float roughY)
{
    float normalization = 1 / (3.14159 * roughX * roughY);

    float nDotH = max(dot(N, H), 0.0);
    float nDotH2 = nDotH * nDotH;

    // Square input roughness to look correct.
    roughX *= roughX;
    roughY *= roughY;

    float roughX2 = roughX * roughX;
    float roughY2 = roughY * roughY;

    float xDotH = dot(tangent, H);
    float xTerm = (xDotH * xDotH) / roughX2;

    float yDotH = dot(bitangent, H);
    float yTerm = (yDotH * yDotH) / roughY2;

    float denominator = xTerm + yTerm + nDotH2;

    return 1.0 / (normalization * denominator * denominator);
}

// Defined in TextureLayers.frag.
vec4 GetEmissionColor(vec2 uv1, vec2 uv2, vec4 transform1, vec4 transform2);
vec4 GetAlbedoColor(vec2 uv1, vec2 uv2, vec2 uv3, vec4 transform1, vec4 transform2, vec4 transform3, vec4 colorSet5);

vec3 DiffuseTerm(vec4 albedoColor, vec3 diffuseIbl, vec3 N, vec3 V, vec3 kDiffuse, float metalness)
{
    vec3 diffuseTerm = kDiffuse * albedoColor.rgb;

    // Some sort of fake SSS color.
    // TODO: The SSS color may be part of a separate render pass
    // and not take into account diffuse lighting.
    diffuseTerm += paramA3.rgb * metalness * renderExperimental;

    // Baked ambient lighting.
    vec3 diffuseLight = vec3(0);
    diffuseLight += diffuseIbl;
    diffuseLight += texture(bakeLitMap, bake1).rgb;

    // Direct lighting.
    diffuseLight += LambertShading(N, V) * directLightIntensity;

    diffuseTerm *= diffuseLight;

    // Color multiplier param.
    diffuseTerm *= paramA5.rgb;

    if (hasParam153 == 1)
        diffuseTerm = param153.rgb + param154.rgb;

    return diffuseTerm;
}

vec3 RimLightingTerm(vec3 N, vec3 V, vec3 specularIbl)
{
    // TODO: How does this work?
    float facingRatio = (1 - max(dot(N, V), 0));
    return mix(vec3(1), paramA6.rgb, pow(facingRatio, 2));
}

vec3 SpecularTerm(vec3 N, vec3 V, vec3 tangent, vec3 bitangent, float roughness, vec3 specularIbl, vec3 kSpecular, float occlusion)
{
    // Specular calculations adapted from https://learnopengl.com/PBR/IBL/Specular-IBL
    vec2 brdf  = texture(iblLut, vec2(max(dot(N, V), 0.0), roughness)).rg;
    vec3 specularTerm = vec3(0);
    specularTerm += specularIbl * ((kSpecular * brdf.x) + brdf.y);

    // TODO: Does image based lighting consider anisotropy?
    // This probably works differently in game.
    // https://developer.blender.org/diffusion/B/browse/master/intern/cycles/kernel/shaders/node_anisotropic_bsdf.osl
    float roughnessX = roughness * (1.0 + paramCA);
    float roughnessY = roughness / (1.0 + paramCA);

    // Direct lighting.
    // The two BRDFs look very different so don't just use anisotropic for everything.
    float specularBrdf = 0;
    if (paramCA != 0)
        specularBrdf = GgxAnisotropic(N, V, tangent, bitangent, roughnessX, roughnessY) * directLightIntensity;
    else
        specularBrdf = pow(Ggx(N, V, roughness), param145.x) * directLightIntensity;

    // TODO: Some sort of fake SSS.
    if (renderExperimental == 1)
        specularBrdf = pow(specularBrdf, param145.x);

    specularTerm += kSpecular * vec3(specularBrdf);

    // Cavity Map used for specular occlusion.
    if (paramE9 == 1)
        specularTerm.rgb *= occlusion;

    // TODO: Specular color?
    specularTerm *= paramA0.rgb;

    vec3 rimTerm = RimLightingTerm(N, V, specularIbl);
    specularTerm *= mix(vec3(1), rimTerm, renderRimLighting);

    return specularTerm;
}

vec3 EmissionTerm(vec4 emissionColor)
{
    return emissionColor.rgb * param9B.rgb;
}

float GetF0(float ior)
{
    return pow((1 - ior) / (1 + ior), 2);
}

float GetTransitionBlend(float blendMap, float transitionFactor)
{
    if (blendMap <= (1 - transitionFactor))
        return 1.0;
    else
        return 0.0;
}

void main()
{
    fragColor = vec4(0, 0, 0, 1);

    vec4 norColor = texture(norMap, map1).xyzw;
    if (hasInkNorMap == 1)
        norColor.rgb = texture(inkNorMap, map1).rga;

    vec3 newNormal = N;
    if (renderNormalMaps == 1)
        newNormal = GetBumpMapNormal(N, tangent, bitangent, norColor);

    vec3 R = reflect(V, newNormal);

    // Get texture color.
    vec4 albedoColor = GetAlbedoColor(map1, uvSet, uvSet, param9E, param146, param147, colorSet5);

    vec4 emissionColor = GetEmissionColor(map1, uvSet, param9E, param146);
    vec4 prmColor = texture(prmMap, map1).xyzw;

    // Probably some sort of override for PRM color.
    if (hasParam156 == 1)
        prmColor = param156;
    // if (hasParam151 == 1)
    //     prmColor.bga = param151.bga;

    // Material masking.
    float transitionBlend = GetTransitionBlend(norColor.b, transitionFactor);

    switch (transitionEffect)
    {
        case 0:
            // Ditto
            albedoColor.rgb = mix(vec3(0.302, 0.242, 0.374), albedoColor.rgb, transitionBlend);
            prmColor = mix(vec4(0, 0.65, 1, 1), prmColor, transitionBlend);
            break;
        case 1:
            // Ink
            albedoColor.rgb = mix(vec3(0.75, 0.10, 0), albedoColor.rgb, transitionBlend);
            prmColor = mix(vec4(0, 0.075, 1, 1), prmColor, transitionBlend);
            break;
        case 2:
            // Gold
            albedoColor.rgb = mix(vec3(0.6, 0.5, 0.1), albedoColor.rgb, transitionBlend);
            prmColor = mix(vec4(1, 0.25, 1, 0.3), prmColor, transitionBlend);
            break;
        case 3:
            // Metal
            albedoColor.rgb = mix(vec3(0.35), albedoColor.rgb, transitionBlend);
            prmColor = mix(vec4(1, 0.25, 1, 0.3), prmColor, transitionBlend);
            break;
    }

    float roughness = prmColor.g;
    float metalness = prmColor.r;

    // Image based lighting.
    vec3 diffuseIbl = textureLod(diffusePbrCube, N, 0).rrr * iblIntensity;
    int maxLod = 10;
    vec3 specularIbl = textureLod(specularPbrCube, R, roughness * maxLod).rrr * iblIntensity;

    fragColor = vec4(0, 0, 0, 1);

    float f0Dialectric = GetF0(paramC8 + 1);
    vec3 f0Final = mix(prmColor.aaa * f0Dialectric, albedoColor.rgb, metalness);
    float nDotV = max(dot(newNormal, V), 0.0);
    vec3 kSpecular = FresnelSchlickRoughness(nDotV, f0Final, roughness);

    // Only use one component to prevent discoloration of diffuse.
    vec3 kDiffuse = (1 - kSpecular.rrr);

    // Metals have no diffuse component.
    // This includes most skin materials.
    kDiffuse *= (1 - metalness);

    // Render passes.
    vec3 diffuseTerm = DiffuseTerm(albedoColor, diffuseIbl, newNormal, V, kDiffuse, metalness);
    fragColor.rgb += diffuseTerm * renderDiffuse;

    vec3 specularTerm = SpecularTerm(newNormal, V, tangent, bitangent, roughness, specularIbl, kSpecular, norColor.a);
    fragColor.rgb += specularTerm * renderSpecular;

    // Ambient Occlusion
    fragColor.rgb *= prmColor.b;
    fragColor.rgb *= texture(gaoMap, bake1).rgb;

    // Emission
    vec3 emissionTerm = EmissionTerm(emissionColor);
    //fragColor.rgb += emissionTerm * renderEmission;

    // HACK: Some models have black vertex color for some reason.
    if (renderVertexColor == 1 && dot(colorSet1.rgb, vec3(1)) != 0)
        fragColor.rgb *= colorSet1.rgb;

    if (renderWireframe == 1)
    {
        vec3 edgeColor = vec3(1);
        float intensity = WireframeIntensity(edgeDistance);
        fragColor.rgb = mix(fragColor.rgb, edgeColor, intensity);
    }

    // Gamma correction.
    fragColor.rgb = GetSrgb(fragColor.rgb);

    // Alpha calculations
    fragColor.a = albedoColor.a;
    fragColor.a *= emissionColor.a;


    // HACK: Some models have black vertex color for some reason.
    if (renderVertexColor == 1 && colorSet1.a != 0)
        fragColor.a *= colorSet1.a;

    // Alpha testing.
    if ((fragColor.a + param98.x) < 0.1)
        discard;

    // TODO: Transparency seems to use some sort of stipple pattern.
    // int x = int(mod(gl_FragCoord.x - 0.5, 4));
    // int y = int(mod(gl_FragCoord.y - 0.5, 4));
    // if ((fragColor.a < (thresholdMatrix16[x][y])))
    //     discard;

    // TODO: How does this work?
    if (hasInkNorMap == 1 && transitionBlend < 1)
        discard;
}