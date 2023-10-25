#version 300 es

precision highp float;
in vec2 vTextureCoord;
uniform sampler2D sTexture;
uniform vec2 uTextureSz;
uniform vec2 uFrameSz;
uniform float uBrightness;
layout(location = 0) out vec4 o_FragColor;

const mat3 MAT_YUV2RGB = mat3 (
//	r			g			b
	1.0,		1.0,		1.0,		// y
	0.0,		-0.39173,	2.017,		// u
	1.5958,		-0.81290,	0.0			// v
);

const vec3 YUV_OFFSET = vec3(0.0625, 0.5, 0.5);

#define EPS 0.00001

/*           width
 *   --------------------
 *   |                  |
 *   |         Y        | height
 *   |                  |
 *   --------------------
 *   |                  |
 *   |         U        | height
 *   |                  |
 *   --------------------
 *   |                  |
 *   |         V        | height
 *   |                  |
 *   --------------------
 *           width
 */

// YUV444pの3プレーンのデータをGL_LUMINANCEの1つのテクスチャとしてまとめて渡して換算する場合(元の1ピクセル分のデータがテクセル1個に対応)
void main() {
	vec2 tex = vec2(vTextureCoord.x, vTextureCoord.y / 3.0);
	vec2 uv_sz = (uFrameSz / uTextureSz) / 3.0;
	float y = texture(sTexture, tex).r;
	float u = texture(sTexture, vec2(tex.x, tex.y + uv_sz.y)).r;
	float v = texture(sTexture, vec2(tex.x, tex.y + uv_sz.y * 2.0)).r;
	// 輝度がEPS未満なら黒を返す(プレビューを停止した時に画面が緑になるのを防ぐため)
	if (y > EPS) {
		vec3 yuv = vec3(clamp(y + uBrightness, 0.0, 1.0), u, v) - YUV_OFFSET;
		vec3 rgb = clamp((MAT_YUV2RGB * yuv), 0.0, 1.0);
		o_FragColor = vec4(rgb, 1.0);
	} else {
		o_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
	}
}
