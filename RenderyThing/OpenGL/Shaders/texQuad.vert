#version 330 core //Using version GLSL version 3.3
in vec2 vPos;
        
uniform mat4 model;
uniform mat4 projection;
        
out vec2 texCoord;

void main()
{
    texCoord = vPos.xy;
    gl_Position = projection * model * vec4(vPos.xy, 0.0, 1.0);
}
