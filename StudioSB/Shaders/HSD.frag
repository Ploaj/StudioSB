#version 330
in vec3 vertPosition;
in vec3 normal;
in vec3 bitangent;
in vec3 tangent;
in vec4 color;
in vec2 tex0;
in vec3 specularPass;

uniform int hasSphere0;
uniform int hasDiffuse0;
uniform sampler2D diffuseTex0;
uniform vec2 diffuseScale0;

uniform int hasSphere1;
uniform int hasDiffuse1;
uniform sampler2D diffuseTex1;
uniform vec2 diffuseScale1;

uniform int hasSpecular;
uniform sampler2D specularTex;
uniform vec2 specularScale;

uniform int hasBumpMap;
uniform int bumpMapWidth;
uniform int bumpMapHeight;
uniform sampler2D bumpMapTex;
uniform vec2 bumpMapTexScale;

uniform vec4 diffuseColor;
uniform vec4 ambientColor;
uniform vec4 specularColor;

uniform int flags;
uniform int enableSpecular;
uniform int enableDiffuseLighting;

uniform float glossiness;
uniform float transparency;

uniform mat4 mvp;
uniform mat4 sphereMatrix;

uniform int renderDiffuse;
uniform int renderSpecular;

uniform int renderAlpha;

uniform int renderNormalMap;

uniform vec3 cameraPos;

out vec4 fragColor;

vec3 CalculateBumpMapNormal(vec3 normal, vec3 tangent, vec3 bitangent,
    int hasBump, sampler2D bumpMap, int width, int height, vec2 texCoords)
{
    if (hasBump != 1)
        return normal;

    // Compute a normal based on difference in height.
    float offsetX = 1.0 / width;
    float offsetY = 1.0 / height;
    float a = texture2D(bumpMap, texCoords).r;
    float b = texture2D(bumpMap, texCoords + vec2(offsetX, 0)).r;
    float c = texture2D(bumpMap, texCoords + vec2(0, offsetY)).r;
    vec3 bumpNormal = normalize(vec3(b-a, c-a, 0.1));

    mat3 tbnMatrix = mat3(tangent, bitangent, normal);
    vec3 newNormal = tbnMatrix * bumpNormal;
    return normalize(newNormal);
}

vec2 GetSphereCoords(vec3 N)
{
    vec3 viewNormal = mat3(sphereMatrix) * normal.xyz;
    return viewNormal.xy * 0.5 + 0.5;
}

vec3 DiffusePass(vec3 N, vec3 V)
{
    // Diffuse
    //float blend = 0.1; // TODO: Use texture's blend.
    float lambert = clamp(dot(N, V), 0, 1);

    vec4 diffuseMap = vec4(1);

    vec2 diffuseCoords0 = tex0;
    if (hasSphere0 == 1)
        diffuseCoords0 = GetSphereCoords(N);

    vec2 diffuseCoords1 = tex0;
    if (hasSphere1 == 1)
        diffuseCoords1 = GetSphereCoords(N);

    if (hasDiffuse0 == 1)
        diffuseMap = texture(diffuseTex0, diffuseCoords0 * diffuseScale0).rgba;

    if (hasDiffuse1 == 1)
        diffuseMap = mix(diffuseMap, texture(diffuseTex1, diffuseCoords1 * diffuseScale1), 0.1);

    vec3 diffuseTerm = diffuseMap.rgb;
    if (enableDiffuseLighting == 1)
        diffuseTerm *= ambientColor.rgb + diffuseColor.rgb * lambert;//mix(ambientColor.rgb, diffuseColor.rgb, lambert);

    return diffuseTerm;
}

vec3 SpecularPass(vec3 N, vec3 V)
{
    // Specular
    float phong = clamp(dot(N, V), 0, 1);

    phong = pow(phong, glossiness);

    vec3 specularTerm = vec3(phong) * specularColor.rgb;

    //if (hasSpecular == 1)
    //    specularTerm *= texture(specularTex, tex0 * specularScale).rgb;

    specularTerm *= enableSpecular;

    return specularTerm;
}

void main()
{
	fragColor = vec4(0, 0, 0, 1);
	
	vec3 V = normalize(vertPosition - cameraPos);
    vec3 N = normal;
    /*if (renderNormalMap == 1)
    {
        // This seems to only affect diffuse.
        N = CalculateBumpMapNormal(normal, tangent, bitangent, hasBumpMap,
            bumpMapTex, bumpMapWidth, bumpMapHeight, tex0  * bumpMapTexScale);
    }*/

	// Render passes
	fragColor.rgb += DiffusePass(N, V) * renderDiffuse;
	fragColor.rgb += specularPass * renderSpecular;//SpecularPass(N, V) * renderSpecular;

	//fragColor.rgb *= color.rgb;

	// Set alpha
    if (renderAlpha == 1)
        fragColor.a = texture(diffuseTex0, tex0 * diffuseScale0).a * transparency;
}