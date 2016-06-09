varying vec2 vUv;

uniform vec2 u_resolution;
uniform sampler2D u_texture;

uniform float u_screenWidth;
uniform float u_screenHeight;






void main()
{
vec2 texel = vec2(1.0/u_screenWidth, 1.0/u_screenHeight);

	float value = texture2D(u_texture, vUv).g;
	vec3 col;
	col = mix(vec3(1.0, 1.0, 1.0), vec3(0.0, 0.0, 0.0), value);
	gl_FragColor = vec4(col, 1.0)	;
}
