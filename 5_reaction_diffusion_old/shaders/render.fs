#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
// shadertoy compatible uniforms

uniform float     iGlobalTime;
uniform vec2	  iResolution;

uniform sampler2D image;

vec3 color_A = vec3(0.0, 0.0, 0.0);
vec3 color_B = vec3(1.0, 1.0, 1.0);

float fit(float value, float oMin, float oMax, float nMin, float nMax){
	return ((value - oMin)/ (oMax - oMin) + nMin) * (nMax - nMin);
}

void main() {
	// draw the texture
	vec2 uv = gl_FragCoord.xy/iResolution;
	float main = fit(texture2D(image, uv).y, 0.2, 0.4, 0.0, 1.0);	

	vec3 color = mix(color_A, color_B, 1.-main);
	gl_FragColor = vec4(color, 1.0); 

}
