#version 330

in vec3 vertexNormal;
in vec3 tangent;
in vec3 bitangent;
in vec2 map1;
in vec2 uvSet;
in vec2 uvSet1;
//in vec2 uvSet2;
in vec4 colorSet1;
in vec4 colorSet2;
in vec4 colorSet21;
in vec4 colorSet22;
in vec4 colorSet23;
in vec4 colorSet3;
in vec4 colorSet4;
in vec4 colorSet5;
in vec4 colorSet6;
in vec4 colorSet7;
in vec2 bake1;
in vec3 position;
noperspective in vec3 edgeDistance;

uniform sampler2D colMap;
uniform sampler2D col2Map;
uniform sampler2D prmMap;
uniform sampler2D norMap;
uniform sampler2D emiMap;
uniform sampler2D emi2Map;
uniform sampler2D bakeLitMap;
uniform sampler2D gaoMap;

uniform int hasInkNorMap;
uniform sampler2D inkNorMap;

uniform int hasDifCubeMap;
uniform samplerCube difCubeMap;

uniform int hasDiffuse;
uniform sampler2D difMap;

uniform int hasDiffuse2;
uniform sampler2D dif2Map;

uniform int hasDiffuse3;
uniform sampler2D dif3Map;

uniform sampler2D projMap;

uniform sampler2D uvPattern;

uniform sampler2D iblLut;

uniform samplerCube diffusePbrCube;
uniform samplerCube specularPbrCube;

uniform vec4 renderChannels;
uniform int renderMode;

uniform int renderWireframe;
uniform int renderNormalMaps;

uniform vec4 CustomVector6;
uniform vec4 CustomVector31;
uniform vec4 CustomVector32;

uniform mat4 mvp;
uniform vec3 cameraPos;

out vec4 fragColor;

// Defined in Wireframe.frag
float WireframeIntensity(vec3 distanceToEdges);

// Defined in NormalMap.frag.
vec3 GetBumpMapNormal(vec3 N, vec3 tangent, vec3 bitangent, vec4 norColor);

// Defined in Gamma.frag.
vec3 GetSrgb(vec3 linear);

float LambertShading(vec3 N, vec3 V)
{
	float lambert = max(dot(N, V), 0);
	return lambert;
}

vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

float GgxShading(vec3 N, vec3 H, float roughness)
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

// Defined in TextureLayers.frag.
vec4 GetEmissionColor(vec2 uv1, vec2 uv2, vec4 transform1, vec4 transform2);
vec4 GetAlbedoColor(vec2 uv1, vec2 uv2, vec2 uv3, vec3 R, vec4 transform1, vec4 transform2, vec4 transform3, vec4 colorSet5);

void main()
{
	vec4 norColor = texture(norMap, map1).xyzw;
    if (hasInkNorMap == 1)
        norColor.rgb = texture(inkNorMap, map1).rga;

    vec3 fragmentNormal = vertexNormal;
    if (renderNormalMaps == 1)
        fragmentNormal = GetBumpMapNormal(vertexNormal, tangent, bitangent, norColor);

	vec3 V = normalize(position - cameraPos);
	vec3 R = reflect(V, fragmentNormal);

    // Get texture colors.
	vec4 albedoColor = GetAlbedoColor(map1, uvSet, uvSet, R, CustomVector6, CustomVector31, CustomVector32, colorSet5);
	vec4 prmColor = texture(prmMap, map1).xyzw;
	vec4 emiColor = GetEmissionColor(map1, uvSet, CustomVector6, CustomVector31);
	vec4 bakeLitColor = texture(bakeLitMap, bake1).rgba;
    vec4 gaoColor = texture(gaoMap, bake1).rgba;
    vec4 projColor = texture(projMap, map1).rgba;

	// Invert glossiness?
	float roughness = clamp(1 - prmColor.g, 0, 1);

    vec4 uvPatternColor = texture(uvPattern, map1).rgba;

	// Image based lighting.
	vec3 diffuseIbl = textureLod(diffusePbrCube, R, 0).rgb * 2.5;
	int maxLod = 10;
	vec3 specularIbl = textureLod(specularPbrCube, R, roughness * maxLod).rgb * 2.5;

	float metalness = prmColor.r;

	// Just gamma correct albedo maps.
	fragColor = vec4(1);

	switch (renderMode)
	{
        case 1:
            fragColor.rgb = vec3(0.218) * max(dot(fragmentNormal, V), 0);
            fragColor.rgb = GetSrgb(fragColor.rgb);
            break;
		case 2:
			fragColor = albedoColor;
			fragColor.rgb = GetSrgb(fragColor.rgb);
			break;
		case 3:
			fragColor = prmColor;
			break;
		case 4:
			fragColor = norColor;
			break;
		case 5:
			fragColor = emiColor;
			fragColor.rgb = GetSrgb(fragColor.rgb);
			break;
		case 6:
			fragColor = bakeLitColor;
			fragColor.rgb = GetSrgb(fragColor.rgb);
			break;
        case 7:
            fragColor = gaoColor;
            fragColor.rgb = GetSrgb(fragColor.rgb);
            break;
        case 8:
            fragColor = projColor;
            fragColor.rgb = GetSrgb(fragColor.rgb);
            break;
		case 9:
			fragColor = colorSet1;
			break;
		case 10:
			fragColor = colorSet2;
			break;
		case 11:
			fragColor = colorSet21;
			break;
		case 12:
			fragColor = colorSet22;
			break;
		case 13:
			fragColor = colorSet23;
			break;
		case 14:
			fragColor = colorSet3;
			break;
		case 15:
			fragColor = colorSet4;
			break;
		case 16:
			fragColor = colorSet5;
			break;
		case 17:
			fragColor = colorSet6;
			break;
		case 18:
			fragColor = colorSet7;
			break;
		case 19:
			fragColor = vec4(fragmentNormal * 0.5 + 0.5, 1);
			break;
		case 20:
			fragColor = vec4(tangent * 0.5 + 0.5, 1);
			break;
        case 21:
            fragColor = vec4(bitangent * 0.5 + 0.5, 1);
            break;
		case 22:
			fragColor = vec4(bake1, 1, 1);
			break;
		case 23:
			fragColor = vec4(map1, 1, 1);
			break;
		case 24:
			fragColor = vec4(uvSet, 1, 1);
			break;
		case 25:
			fragColor = vec4(uvSet1, 1, 1);
			break;
		case 26:
			//fragColor = vec4(uvSet1, 1, 1);
			break;
        case 27:
            fragColor = uvPatternColor;
            break;
		default:
			fragColor = vec4(0, 0, 0, 1);
			break;
	}

    fragColor.rgb *= renderChannels.rgb;
    if (renderChannels.r == 1 && renderChannels.g == 0 && renderChannels.b == 0)
        fragColor.rgb = fragColor.rrr;
    else if (renderChannels.g == 1 && renderChannels.r == 0 && renderChannels.b == 0)
        fragColor.rgb = fragColor.ggg;
    else if (renderChannels.b == 1 && renderChannels.r == 0 && renderChannels.g == 0)
        fragColor.rgb = fragColor.bbb;

    if (renderChannels.a == 1 && renderChannels.r == 0 && renderChannels.g == 0 && renderChannels.b == 0)
        fragColor = vec4(fragColor.aaa, 1);

	// Don't use alpha blending with debug shading.
	fragColor.a = 1;

	if (renderWireframe == 1)
	{
		vec3 edgeColor = vec3(1);
		float intensity = WireframeIntensity(edgeDistance);
		fragColor.rgb = mix(fragColor.rgb, edgeColor, intensity);
	}
}