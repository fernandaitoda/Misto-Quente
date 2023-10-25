#version 100

precision highp float;
varying vec2 vTextureCoord;
uniform sampler2D sTexture;
uniform vec2 uTextureSz;
uniform vec2 uFrameSz;
uniform float uBrightness;

#define EPS 0.00001

void main() {
	float rg = texture2D(sTexture, vTextureCoord).a * 255.0;
	float gb = texture2D(sTexture, vTextureCoord).r * 255.0;
	float r = clamp(rg / 8.0, 0.0, 31.0) / 31.0;
	float g = (mod(rg, 8.0) * 8.0 + clamp(gb / 32.0, 0.0, 31.0)) / 63.0;
	if (g < EPS) g = 0.0;
	float b = clamp(mod(gb, 32.0), 0.0, 31.0) / 31.0;
	gl_FragColor = clamp(vec4(r, g, b, 1.0), 0.0, 1.0);
}