
uniform sampler2D texture;
uniform vec4 fromColor;
uniform vec4 toColor;
uniform int transparency;

varying vec4 vertexColor;

void main() {
	
	vec4 texel = texture2D(texture, gl_TexCoord[0].st);
	if (transparency == 0) {
		texel.a = 1;
	}
	gl_FragColor = ((fromColor * (1 - texel) + toColor * texel) * vertexColor);
	
}
