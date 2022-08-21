#version 330 core //Using version GLSL version 3.3
in vec2 vPos;

uniform mat4 model;
uniform mat4 projection;

out vec2 texCoord;

void main()
{
    texCoord = (vPos.xy - vec2(0.5f, 0.5f)) * 2.0f;
    gl_Position = projection * model * vec4(vPos.xy, 0.0, 1.0);
}
