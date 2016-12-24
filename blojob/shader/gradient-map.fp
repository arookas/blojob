
uniform sampler2D texture;
uniform vec4 fromColor;
uniform vec4 toColor;
uniform int transparency;

varying vec4 vertexColor;

void main() {
	
	vec4 cZero = vec4(0, 0, 0, 0);
	vec4 cOne = vec4(1, 1, 1, 1);

	vec4 texel = texture2D(texture, gl_TexCoord[0].st);

	if (transparency == 0) {
		texel.a = 1.0;
	}
	
	if (fromColor != cZero || toColor != cOne) {
		texel = (fromColor * (1.0 - texel) + toColor * texel);
	}
	
	gl_FragColor = (texel * vertexColor);
}
