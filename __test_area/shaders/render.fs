#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
// shadertoy compatible uniforms
uniform float     iGlobalTime;
uniform vec3	  iResolution;

uniform sampler2D u_image;
uniform vec2 u_size;

const float COLOR_MIN = 0.2, COLOR_MAX = 0.4;

void main() {
	float v = (COLOR_MAX - texture2D(u_image, gl_FragCoord.xy / u_size).y) / (COLOR_MAX - COLOR_MIN);
	gl_FragColor = vec4(v, v, v, 1);
}
