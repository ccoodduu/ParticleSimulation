#version 330

in vec3 vertexPosition;
in mat4 instanceTransform;

uniform mat4 mvp;

out vec2 localCoord;

void main()
{
    localCoord = vec2(vertexPosition.x, -vertexPosition.z);

    gl_Position = mvp * instanceTransform * vec4(vertexPosition, 1.0);
}
