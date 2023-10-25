#version 300 es

precision highp float;
in vec2 vTextureCoord;
uniform sampler2D sTexture;
uniform vec2 uTextureSz;
uniform vec2 uFrameSz;
uniform float uBrightness;
layout(location = 0) out vec4 o_FragColor;

/*const mat3 MAT_YUV2RGB = mat3 (
//	r			g			b			// こっちの方が少し色調が暗い
	1.0,		1.0,		1.0,		// y
	0.0,		-0.337633,	1.732446,	// u
	1.370705,	-0.698001,	0.0			// v
); */
const mat3 MAT_YUV2RGB = mat3 (
//	r			g			b
	1.0,		1.0,		1.0,		// y
	0.0,		-0.39173,	2.017,		// u
	1.5958,		-0.81290,	0.0			// v
);

const vec3 YUV_OFFSET = vec3(0.0625, 0.5, 0.5);
const float Y_SCALE = 2.0 / 3.0;
const float UV_OFFSET_Y = Y_SCALE;
const float V_OFFSET_Y = 1.0 / 6.0;

#define EPS 0.00001

/*           width
 *  (0.0,0.0)
 *   --------------------
 *   |                  |
 *   |         Y        | height
 *   |                  |
 *   |                  |
 *   -------------------- (1.0,1.0)
 *   |    U    |   U    | height/4
 *   --------------------
 *   |    V    |   V    | height/4
 *   --------------------
 *     width/2
 */

// YUV422pの3プレーンのデータをGL_LUMINANCEの1つのテクスチャとしてまとめて渡して換算する場合(元の1ピクセル分のデータがテクセル1個に対応)
// YUYVをRGBAのテクスチャとして渡して描画する時よりも遅くなってしまう(転送量が4倍&アライメントが1だからみたい)
void main() {
	vec2 uv_tex = vec2(vTextureCoord.x / 2.0, vTextureCoord.y / 6.0 + UV_OFFSET_Y);
	vec2 uv_sz = (uFrameSz / uTextureSz) / 2.0;
	float y, u, v;
	y = texture(sTexture, vec2(vTextureCoord.x, vTextureCoord.y * Y_SCALE)).r;
	float u1 = texture(sTexture, vec2(uv_tex.x,        uv_tex.y)).r;
	float u2 = texture(sTexture, vec2(uv_tex.x + 0.5,  uv_tex.y)).r;
	u = (u1 + u2) / 2.0;
	uv_tex.y = uv_tex.y + V_OFFSET_Y;
	float v1 = texture(sTexture, vec2(uv_tex.x,       uv_tex.y)).r;
	float v2 = texture(sTexture, vec2(uv_tex.x + 0.5, uv_tex.y)).r;
	v = (v1 + v2) / 2.0;
	// 輝度がEPS未満なら黒を返す(プレビューを停止した時に画面が緑になるのを防ぐため)
	if (y > EPS) {
		vec3 yuv = vec3(clamp(y + uBrightness, 0.0, 1.0), u, v) - YUV_OFFSET;
		vec3 rgb = clamp((MAT_YUV2RGB * yuv), 0.0, 1.0);
		o_FragColor = vec4(rgb, 1.0);
	} else {
		o_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
	}
}
