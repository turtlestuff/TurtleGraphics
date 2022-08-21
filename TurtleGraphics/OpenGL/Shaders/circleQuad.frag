#version 330 core
in vec2 texCoord;

uniform vec4 color;

out vec4 fragColor;

void main()
{
    fragColor = (sqrt(texCoord.x * texCoord.x + texCoord.y * texCoord.y) <= 1.0f) ? color : vec4(0.0f);
}
