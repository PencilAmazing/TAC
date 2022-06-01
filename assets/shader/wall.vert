#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;

out vec2 fragTexCoord;
flat out int face;

uniform mat4 mvp;

void main()
{
    fragTexCoord = vertexTexCoord;
    face = -1;
    if(vertexNormal.x > 0) {
    face = 0;
    } else if(vertexNormal.x < 0) {
    face = 1;
    } else if(vertexNormal.y > 0) {
    face = 2;
    } else if(vertexNormal.y < 0) {
    face = 3;
    } else if(vertexNormal.z > 0) {
    face = 4;
    } else if(vertexNormal.z < 0) {
    face = 5;
    };
    gl_Position = mvp*vec4(vertexPosition, 1.0);
}
