varying vec2 vUv;
uniform sampler2D u_texture;
uniform vec2 u_resolution;
uniform int u_startFrame;

void main() {
	/*if(u_startFrame)*/
	/*{*/
		/*gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);*/
		/*return;*/
	/*}	*/

	col = vec3(vUv, 1.0);
	gl_FragColor = vec4(col, 1.0);
}
