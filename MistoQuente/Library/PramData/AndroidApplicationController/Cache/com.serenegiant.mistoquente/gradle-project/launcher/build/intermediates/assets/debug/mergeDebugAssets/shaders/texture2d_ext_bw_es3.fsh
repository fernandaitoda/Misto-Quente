#version 300 es
#extension GL_OES_EGL_image_external_essl3 : require
precision highp float;
in vec2 vTextureCoord;
uniform samplerExternalOES sTexture;
uniform vec2 uTextureSz;
uniform vec2 uFrameSz;
layout(location = 0) out vec4 o_FragColor;

const vec3 bw = vec3(0.3, 0.59, 0.11);

void main() {
	vec3 tc = texture(sTexture, vTextureCoord).rgb;
	float color = dot(tc, bw);
	o_FragColor = vec4(color, color, color, 1.0);
}
