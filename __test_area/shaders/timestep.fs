#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable

// shadertoy compatible uniforms
uniform float     iGlobalTime;
uniform vec3	  iResolution;

uniform sampler2D u_image;
uniform vec2 u_size;

const float F = 0.0545, K = 0.062,
	D_a = 0.2, D_b = 0.1;

const float TIMESTEP = 1.0;

void main() {
	vec2 p = gl_FragCoord.xy,
	     n = p + vec2(0.0, 1.0),
	     e = p + vec2(1.0, 0.0),
	     s = p + vec2(0.0, -1.0),
	     w = p + vec2(-1.0, 0.0);

	vec2 val = texture2D(u_image, p / u_size).xy,
	     laplacian = texture2D(u_image, n / u_size).xy
		+ texture2D(u_image, e / u_size).xy
		+ texture2D(u_image, s / u_size).xy
		+ texture2D(u_image, w / u_size).xy
		- 4.0 * val;

	vec2 delta = vec2(D_a * laplacian.x - val.x*val.y*val.y + F * (1.0-val.x),
		D_b * laplacian.y + val.x*val.y*val.y - (K+F) * val.y);

	gl_FragColor = vec4(val + delta * TIMESTEP, 0, 0);
}
</script>
