#version 330 core
in vec2 vPos;
        
uniform mat4 model;
uniform mat4 projection;
        
void main()
{
    gl_Position = projection * model * vec4(vPos.xy, 0.0, 1.0);
}
