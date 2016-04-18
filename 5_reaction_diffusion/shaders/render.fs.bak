#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
// shadertoy compatible uniforms

uniform float     iGlobalTime;
uniform vec2	  iResolution;

uniform sampler2D image;

const float COLOR_MIN = 0.2, COLOR_MAX = 0.4;

vec3 color_A = vec3(0.2, 0.3, 0.7);
vec3 color_B = vec3(0.5, 0.7, 0.8);

void main() {
	// draw the texture
	/*float v = (COLOR_MAX - texture2D(image, gl_FragCoord.xy / iResolution).y) / (COLOR_MAX - COLOR_MIN);*/
	float main = texture2D(image, gl_FragCoord.xy/iResolution).y;	
	/*vec3 color = mix(color_A, color_B, main);*/
	vec3 color = vec3(mix(0.0, 1.0, main));
	gl_FragColor = vec4(color, 1.0); 

}
