#version 300 es
#extension GL_OES_EGL_image_external_essl3 : require
precision highp float;
in vec2 vTextureCoord;
uniform samplerExternalOES sTexture;
uniform vec2 uTextureSz;
uniform vec2 uFrameSz;
#define KERNEL_SIZE 9
uniform float uKernel[KERNEL_SIZE];
uniform vec2 uTexOffset[KERNEL_SIZE];
uniform float uColorAdjust;
layout(location = 0) out vec4 o_FragColor;

void main() {
	int i = 0;
	vec4 sum = vec4(0.0);
	if (vTextureCoord.x < vTextureCoord.y - 0.005) {
		for (i = 0; i < KERNEL_SIZE; i++) {
			vec4 texc = texture(sTexture, vTextureCoord + uTexOffset[i]);
			sum += texc * uKernel[i];
		}
		sum += uColorAdjust;
	} else if (vTextureCoord.x > vTextureCoord.y + 0.005) {
		sum = texture(sTexture, vTextureCoord);
	} else {
		sum.r = 1.0;
	}
	o_FragColor = sum;
}
