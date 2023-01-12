#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec4 vertexColor;

// Input uniform values
uniform mat4 matProjection;
uniform mat4 mvp;

out vec2 fragTexCoord;
out vec4 fragColor;

void main()
{
	fragTexCoord = vertexTexCoord;
	fragColor = vertexColor;

	gl_Position = matProjection * (mvp * vec4(0,0,0,1)) + vec4(vertexPosition.xy, 0, 0);
}
