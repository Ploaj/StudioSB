#version 330

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

in vec3 geomVertexNormal[];
in vec3 geomPosition[];
in vec3 geomTangent[];
in vec3 geomBitangent[];
in vec2 geomMap1[];
in vec2 geomUvSet[];
in vec2 geomUvSet1[];
in vec2 geomUvSet2[];
in vec4 geomColorSet1[];
in vec4 geomColorSet5[];
in vec2 geomBake1[];

out vec3 vertexNormal;
out vec3 tangent;
out vec3 bitangent;
out vec2 map1;
out vec2 uvSet;
out vec2 uvSet1;
out vec2 uvSet2;
out vec4 colorSet1;
out vec4 colorSet5;
out vec2 bake1;
out vec3 position;

noperspective out vec3 edgeDistance;

// Adapted from code in David Wolff's "OpenGL 4.0 Shading Language Cookbook"
// https://gamedev.stackexchange.com/questions/136915/geometry-shader-wireframe-not-rendering-correctly-glsl-opengl-c
vec3 EdgeDistances()
{
    float a = length(gl_in[1].gl_Position.xyz - gl_in[2].gl_Position.xyz);
    float b = length(gl_in[2].gl_Position.xyz - gl_in[0].gl_Position.xyz);
    float c = length(gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz);

    float alpha = acos((b*b + c*c - a*a) / (2.0*b*c));
    float beta = acos((a*a + c*c - b*b) / (2.0*a*c));
    float ha = abs(c * sin(beta));
    float hb = abs(c * sin(alpha));
    float hc = abs(b * sin(alpha));
    return vec3(ha, hb, hc);
}

void main()
{
    vec3 distances = EdgeDistances();

    // Create a triangle and assign the vertex attributes.
    for (int i = 0; i < 3; i++)
    {
        gl_Position = gl_in[i].gl_Position;
        vertexNormal = geomVertexNormal[i];
        tangent = geomTangent[i];
        bitangent = geomBitangent[i];

        map1 = geomMap1[i];
        uvSet = geomUvSet[i];
        uvSet1 = geomUvSet1[i];
        uvSet2 = geomUvSet2[i];
        colorSet1 = geomColorSet1[i];
        colorSet5 = geomColorSet5[i];
        position = geomPosition[i];
        bake1 = geomBake1[i];

        // The distance from a point to each of the edges.
        if (i == 0)
            edgeDistance = vec3(distances.x, 0, 0);
        else if (i == 1)
            edgeDistance = vec3(0, distances.y, 0);
        else if (i == 2)
            edgeDistance = vec3(0, 0, distances.z);

        EmitVertex();
    }

    EndPrimitive();
}