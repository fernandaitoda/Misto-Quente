#version 100

precision highp float;
varying vec2 vTextureCoord;
uniform sampler2D sTexture;
uniform vec2 uTextureSz;
uniform vec2 uFrameSz;


void main() {
	gl_FragColor = texture2D(sTexture, vTextureCoord);
}
