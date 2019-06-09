#version 330

in vec3 vertPosition;
in vec3 N;
in vec2 Tex0;

uniform sampler2D uvPattern;

uniform int renderMode;
uniform mat4 mvp;
uniform vec3 cameraPos;

out vec4 fragColor;

void main()
{
    vec3 newNormal = N;

	vec3 V = normalize(vertPosition - cameraPos);
	vec3 R = reflect(V, newNormal);
	
    vec4 uvPatternColor = texture(uvPattern, Tex0).rgba;

	fragColor = vec4(1);
	switch (renderMode)
	{
        case 1:
            fragColor.rgb = vec3(max(dot(newNormal, V), 0));
            break;
		case 2:
			fragColor = vec4(0, 0, 0, 1);//albedoColor;
			break;
		case 3:
			fragColor = vec4(0, 0, 0, 1);//prmColor;
			break;
		case 4:
			fragColor = vec4(0, 0, 0, 1);//norColor;
			break;
		case 5:
			fragColor = vec4(0, 0, 0, 1);//emiColor;
			break;
		case 6:
			fragColor = vec4(0, 0, 0, 1);//bakeLitColor;
			break;
        case 7:
            fragColor = vec4(0, 0, 0, 1);//gaoColor;
            break;
        case 8:
            fragColor = vec4(0, 0, 0, 1);//projColor;
            break;
		case 9:
			fragColor = vec4(0, 0, 0, 1);//colorSet1;
			break;
		case 10:
			fragColor = vec4(newNormal * 0.5 + 0.5, 1);
			break;
		case 11:
			fragColor = vec4(0, 0, 0, 1);//vec4(tangent * 0.5 + 0.5, 1);
			break;
        case 12:
            fragColor = vec4(0, 0, 0, 1);//vec4(bitangent * 0.5 + 0.5, 1);
            break;
		case 13:
			fragColor = vec4(0, 0, 0, 1);//vec4(bake1, 1, 1);
			break;
        case 14:
            fragColor = uvPatternColor;
            break;
		default:
			fragColor = vec4(0, 0, 0, 1);
			break;
	}

	// Don't use alpha blending with debug shading.
	fragColor.a = 1;

	/*if (renderWireframe == 1)
	{
		vec3 edgeColor = vec3(1);
		float intensity = WireframeIntensity(edgeDistance);
		fragColor.rgb = mix(fragColor.rgb, edgeColor, intensity);
	}*/
}
