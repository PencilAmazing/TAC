#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Texture offset
// (x, width) since height is always 0->1
uniform vec2 texCoordShift;

// Output fragment color
out vec4 finalColor;

void main()
{
	finalColor = vec4(1,0,0,1);
    //vec4 texelColor = texture(texture0, fragTexCoord);
    vec4 texelColor = texture(texture0, vec2(texCoordShift.x + texCoordShift.y*fragTexCoord.x, fragTexCoord.y));
    if (texelColor.a <= 0.1) discard;
    finalColor = texelColor * fragColor * colDiffuse;
}