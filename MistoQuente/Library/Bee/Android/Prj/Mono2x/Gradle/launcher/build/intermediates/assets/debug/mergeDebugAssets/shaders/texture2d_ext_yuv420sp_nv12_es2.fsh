#version 100
#extension GL_OES_EGL_image_external : require
precision highp float;
varying vec2 vTextureCoord;
uniform samplerExternalOES sTexture;
uniform samplerExternalOES sTexture2;
uniform vec2 uTextureSz;
uniform vec2 uFrameSz;
uniform float uBrightness;


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

#define EPS 0.00001

/*           width
 *  (0.0,0.0)
 *   --------------------
 *   |                  |
 *   |         Y        | height
 *   |                  |
 *   |                  |
 *   -------------------- (1.0,1.0)
 *   |   UV   | height/2
 *   |        |
 *   ----------
 *     width/2
 */

// YUV420p(I420)の3プレーンのデータをGL_LUMINANCEの1つのテクスチャとしてまとめて渡して換算する場合(元の1ピクセル分のデータがテクセル1個に対応)
void main() {
	float y = texture2D(sTexture, vTextureCoord).r;
	float v = texture2D(sTexture2, vTextureCoord).a;
	float u = texture2D(sTexture2, vTextureCoord).r;
	// 輝度がEPS未満なら黒を返す(プレビューを停止した時に画面が緑になるのを防ぐため)
	if (y > EPS) {
		vec3 yuv = vec3(clamp(y + uBrightness, 0.0, 1.0), u, v) - YUV_OFFSET;
		vec3 rgb = clamp((MAT_YUV2RGB * yuv), 0.0, 1.0);
		gl_FragColor = vec4(rgb, 1.0);
	} else {
		gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
	}
}
