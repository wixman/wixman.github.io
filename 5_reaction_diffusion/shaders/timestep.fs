
#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
// shadertoy compatible uniforms
uniform float     iGlobalTime;
uniform vec2	  iResolution;

uniform sampler2D image;

const float COLOR_MIN = 0.2, COLOR_MAX = 0.4;

/*const float feed = 0.0545; //  */
/*const float kill = 0.062;*/
const float feed = 0.0367; //  
const float kill = 0.0649;
const float diffuseA = 0.2;
const float diffuseB = 0.1;

const float TIMESTEP = 1.0; // change in time for each iteration

void main() {
	vec2 p = gl_FragCoord.xy, // position
		 n = p + vec2(0.0, 1.0), // up
		 e = p + vec2(1.0, 0.0), // right
		 s = p + vec2(0.0, -1.0), // down
		 w = p + vec2(-1.0, 0.0); // left

	vec2 val = texture2D(image, p / iResolution).xy;
		 
		// Calculate 
	vec2 laplacian = texture2D(image, n / iResolution).xy
		+ texture2D(image, e / iResolution).xy
		+ texture2D(image, s / iResolution).xy
		+ texture2D(image, w / iResolution).xy
		- 4.0 * val;


	vec2 delta = vec2(diffuseA * laplacian.x - val.x*val.y*val.y + feed * (1.0-val.x),
			diffuseB * laplacian.y + val.x*val.y*val.y - (kill + feed) * val.y);

	gl_FragColor = vec4(val + delta * TIMESTEP, 0, 0);
}

