#version 330

in vec2 fragTexCoord;

flat in int face;
out vec4 finalColor;      

uniform sampler2D front;
uniform sampler2D top;
uniform sampler2D bottom;
uniform sampler2D back;
uniform sampler2D right;

uniform sampler2D left;

void main()
{   
    switch(face) {
    case 0: // right
    finalColor = texture(right, fragTexCoord);
    break;
    case 1: // left
    finalColor = texture(left, fragTexCoord);
    break;
    case 2: // top
    finalColor = texture(top, fragTexCoord);
    break;
    case 3: // bottom
    finalColor = texture(bottom, fragTexCoord);
    break;
    case 4: // back
    finalColor = texture(back, fragTexCoord);
    break;
    case 5:
    finalColor = texture(front, fragTexCoord);
    break;
    default:
    finalColor = vec4(1,0,0,1);
    }
    return;
}
