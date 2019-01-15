#version 330

in vec2 UV;

out vec4 fragColor;

uniform vec3 letterColor;
uniform sampler2D fontSheet;

void main()
{
	vec4 texColor = texture2D(fontSheet, UV);

	float alpha = texColor.r;

	vec3 col = letterColor;
	
	if(alpha < 0.3)
		discard;

	fragColor = vec4(col, alpha);
}