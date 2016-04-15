
#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
// shadertoy compatible uniforms
uniform float     iGlobalTime;
uniform vec2	  iResolution;

uniform sampler2D image;

const float feed = 0.0545; //  
const float kill = 0.062;
/*const float feed = 0.0367; //  */
/*const float kill = 0.0649;*/

vec2 p = gl_FragCoord.xy, // position
     currentValue = texture2D(image, p / iResolution).xy; // current texture value

// current values of chemical A and B
float A = currentValue.x, 
	  B = currentValue.y;
	
// diffuse rate of each chemical
const float diffA = 0.2;
const float diffB = 0.1;

const float TIMESTEP = 1.0; // change in time for each iteration


void main() {
	vec2 p1 = p + vec2(0.0, 1.0), // up
		 p2 = p + vec2(1.0, 0.0), // right
		 p3 = p + vec2(0.0, -1.0), // down
		 p4 = p + vec2(-1.0, 0.0); // left

		 
	// Calculate 2D Laplacian - the difference between the average 
	// 							of nearby cells and this cell
	vec2 laplacian = texture2D(image, p1 / iResolution).xy
			+ texture2D(image, p2 / iResolution).xy
			+ texture2D(image, p3 / iResolution).xy
			+ texture2D(image, p4 / iResolution).xy
			- 4.0 * currentValue;


	vec2 delta = vec2(diffA * laplacian.x - A*B*B + feed * (1.0-A),
			diffB * laplacian.y + A*B*B - (kill + feed) * B);

	gl_FragColor = vec4(currentValue + delta * TIMESTEP, 0, 0);
}


