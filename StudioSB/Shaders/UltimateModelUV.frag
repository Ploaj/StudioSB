#version 330

in vec3 N;
in vec3 tangent;
in vec3 bitangent;
in vec2 UV0;
in vec4 colorSet;
in vec2 bake1;
noperspective in vec3 edgeDistance;

uniform mat4 mvp;

out vec4 fragColor;

// Defined in Wireframe.frag.
float WireframeIntensity(vec3 distanceToEdges);

void main()
{
    vec3 edgeColor = vec3(1);
    float intensity = WireframeIntensity(edgeDistance);
    fragColor = vec4(vec3(1), intensity);
}
