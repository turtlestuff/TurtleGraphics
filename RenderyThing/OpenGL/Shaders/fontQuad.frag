#version 330 core
in vec2 texCoord;

uniform sampler2D texture1;
uniform vec4 color;

out vec4 fragColor;

void main()
{
    fragColor = vec4(color.rgb, color.a * texture(texture1, texCoord).r);
}
