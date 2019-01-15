#version 330

vec3 GetBumpMapNormal(vec3 N, vec3 tangent, vec3 bitangent, vec4 norColor)
{
    // Calculate the resulting normal map.
    float x = 2 * norColor.x - 1;
    float y = 2 * norColor.y - 1;
    float z = sqrt(1 - ((x * x) + (y * y))) * 0.5 + 0.5;
    vec3 normalMapColor = vec3(norColor.rg, z);

    // Remap the normal map to the correct range.
    vec3 normalMapNormal = 2.0 * normalMapColor - vec3(1);

    mat3 tbnMatrix = mat3(tangent, bitangent, N);

    vec3 newNormal = tbnMatrix * normalMapNormal;
    return normalize(newNormal);
}
