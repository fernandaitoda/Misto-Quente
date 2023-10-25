#version 300 es
in vec4 aPosition;
in vec4 aTextureCoord;

out vec2 vTextureCoord;

uniform mat4 uMVPMatrix;
uniform mat4 uTexMatrix;

void main() {
	vTextureCoord = (uTexMatrix * aTextureCoord).xy;
	gl_Position = uMVPMatrix * aPosition;
}
