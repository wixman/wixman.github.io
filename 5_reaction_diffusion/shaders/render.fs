varying vec2 vUv;
uniform sampler2D u_texture;
uniform vec3 u_mcolor;
uniform vec3 u_bgcolor;


void main()
{
	/*vec3 tex = texture2D(u_texture, vUv).xyz;*/
	float value = texture2D(u_texture, vUv).g;
	vec3 col;
	col = mix(vec3(1.0, 1.0, 1.0), vec3(0.0, 0.0, 0.0), value);
	col = mix(u_bgcolor, u_mcolor, value*2.);
	/*col = vec3(vUv, 1.0);*/
	gl_FragColor = vec4(col, 1.0);
	/*gl_FragColor = vec4(tex, 1.0);*/
}
