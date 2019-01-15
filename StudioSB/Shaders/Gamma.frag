#version 330

float GetSrgb(float linear)
{
    if (linear <= 0.00031308)
        return 12.92 * linear;
    else
        return 1.055 * pow(linear, (1.0 / 2.4)) - 0.055;
}

vec3 GetSrgb(vec3 linear)
{
    return vec3(GetSrgb(linear.x), GetSrgb(linear.y), GetSrgb(linear.z));
}
