#version 300 es
#extension GL_OES_EGL_image_external_essl3 : require
precision highp float;
in vec2 vTextureCoord;
uniform samplerExternalOES sTexture;
uniform vec2 uTextureSz;
uniform vec2 uFrameSz;
layout(location = 0) out vec4 o_FragColor;

void main() {
	o_FragColor = texture(sTexture, vTextureCoord);
}
