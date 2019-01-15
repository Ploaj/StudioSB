#version 330

in vec2 point;

out vec2 UV;

uniform float letterIndex;
uniform float letterSize;
uniform vec2 letterPosition;
uniform vec2 windowSize;
uniform vec2 windowPosition;
uniform mat4 mvpMatrix;
uniform mat4 transform;
uniform int relativeToWorld;

void main()
{
	vec4 worldPosition = mvpMatrix * transform * vec4(0, 0, 0, 1);

	vec2 ScreenPoint = (worldPosition.xy / worldPosition.w);
	vec2 PointSize = point * (letterSize / (windowSize.yx / 2));

	vec2 LetterIndex = vec2(int(letterIndex) % 16, int(letterIndex / 16)) / 16;
	UV = LetterIndex + vec2(point.x, 1 - point.y) / 16;

	vec2 WindowPoint = (vec2(-1, -1) + (windowPosition / windowSize.yx) * 2) * (1-relativeToWorld);
	WindowPoint.y *= -1;

	gl_Position = vec4(ScreenPoint * relativeToWorld + WindowPoint + PointSize + letterPosition / (windowSize.yx / 2), 0, 1);
}