#version 330 core //Using version GLSL version 3.3
layout (location = 0) in vec2 vPos;
layout (location = 1) in vec2 uvCoord;
        
uniform mat4 model;
uniform mat4 projection;
        
out vec2 texCoord;

void main()
{
    texCoord = uvCoord;
    gl_Position = projection * model * vec4(vPos.xy, 0.0, 1.0);
}
