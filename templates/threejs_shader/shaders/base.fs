uniform vec3 u_color;
uniform vec2 u_resolution;

void main() {
	vec2 p = gl_FragCoord.xy / u_resolution.xy;
	/*gl_FragColor = vec4(u_color, 1.0); // A*/
	gl_FragColor = vec4(p, 0.0,  1.0); // A
}
