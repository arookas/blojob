
uniform sampler2D texture[4];
uniform vec4 blendColorFactor, blendAlphaFactor;
uniform int transparency[4];
uniform int textureCount;
uniform vec4 fromColor;
uniform vec4 toColor;

varying vec4 vertexColor;

void main() {
	
	vec4 cZero = vec4(0, 0, 0, 0);
	vec4 cOne = vec4(1, 1, 1, 1);
	vec2 uv = gl_TexCoord[0].st;

	vec4 output = texture2D(texture[0], uv);
	if (transparency[0] == 0) {
		output.a = 1.0;
	}
	
	if (textureCount >= 2) {
		vec4 texel = texture2D(texture[1], uv);
		if (transparency[1] == 0) {
			texel.a = 1.0;
		}
		output.rgb = mix(output.rgb, texel.rgb, blendColorFactor[1]);
		output.a = mix(output.a, texel.a, blendAlphaFactor[1]);
	}
	
	if (textureCount >= 3) {
		vec4 texel = texture2D(texture[2], uv);
		if (transparency[2] == 0) {
			texel.a = 1.0;
		}
		output.rgb = mix(output.rgb, texel.rgb, blendColorFactor[1]);
		output.a = mix(output.a, texel.a, blendAlphaFactor[1]);
	}
	
	if (textureCount >= 4) {
		vec4 texel = texture2D(texture[3], uv);
		if (transparency[3] == 0) {
			texel.a = 1.0;
		}
		output.rgb = mix(output.rgb, texel.rgb, blendColorFactor[3]);
		output.a = mix(output.a, texel.a, blendAlphaFactor[3]);
	}

	if (fromColor != cZero || toColor != cOne) {
		output = (fromColor * (1.0 - output) + toColor * output);
	}
	
	gl_FragColor = (output * vertexColor);
}
