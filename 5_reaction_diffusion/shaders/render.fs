varying vec2 vUv;
uniform sampler2D u_texture;


void main()
{
	/*float value = texture2D(u_texture, vUv).g;*/
	vec3 col;
	/*col = mix(vec3(1.0, 1.0, 1.0), vec3(0.0, 0.0, 0.0), value);*/
	col = vec3(vUv, 1.0);
	gl_FragColor = vec4(col, 1.0);
}
