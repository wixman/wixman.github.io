varying vec2 vUv;

uniform vec2 u_resolution;
uniform sampler2D u_texture;
uniform vec3 u_mcolor;
uniform vec3 u_bgcolor;
uniform vec3 u_source;


void main()
{
	/*vec3 tex = texture2D(u_texture, vUv).xyz;*/
	float value = texture2D(u_texture, vUv).g;
	vec3 col;
	vec3 col2 = mix(vec3(0.1, 0.1, 0.4), vec3(1.0, 0.2, 0.2), value*2.);
	col = mix(u_bgcolor, u_mcolor, value*2.);
	/*col = mix(col, col2, u_source.x/u_resolution.x);*/
	/*col = vec3(vUv, 1.0);*/
	gl_FragColor = vec4(col, 1.0);
}
