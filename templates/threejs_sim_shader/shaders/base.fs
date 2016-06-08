uniform vec3 u_color;
uniform vec2 u_resolution;
uniform sampler2D u_bufferTexture;
uniform vec3 u_source;

void main() {
	vec2 p = gl_FragCoord.xy / u_resolution.xy;
	gl_FragColor = texture2D( u_bufferTexture, p);

	float dist = distance(u_source.xy, gl_FragCoord.xy);
	gl_FragColor.rgb += u_source.z * max(10.0 - dist, 0.0); // draw with 10.0 radius

	/*gl_FragColor.r += 0.01;*/
}
