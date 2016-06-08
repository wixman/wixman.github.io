uniform vec3 u_color;
uniform vec2 u_resolution;
uniform sampler2D u_bufferTexture;
uniform vec3 u_source;

// kill/feed rate of each chemical
const float feed = 0.0545; //  
const float kill = 0.062;

// diffuse rate of each chemical
const float diffA = 0.2;
const float diffB = 0.1;

vec3 color_A = vec3(0.0, 0.0, 0.0);
vec3 color_B = vec3(1.0, 1.0, 1.0);

void main() {
	
	vec2 p = gl_FragCoord.xy; 

	float currentValue = texture2D(u_bufferTexture, p / u_resolution).x;

	float A = currentValue;
	float B = 1.0 - currentValue;

	vec2 p1 = p + vec2(0.0, 1.0), // up
		 p2 = p + vec2(1.0, 0.0), // right
		 p3 = p + vec2(0.0, -1.0), // down
		 p4 = p + vec2(-1.0, 0.0); // left

		 
	// Calculate 2D Laplacian - the difference between the average 
	// 							of nearby cells and this cell
	float laplacian = texture2D(u_bufferTexture, p1 / u_resolution).x
			+ texture2D(u_bufferTexture, p2 / u_resolution).x
			+ texture2D(u_bufferTexture, p3 / u_resolution).x
			+ texture2D(u_bufferTexture, p4 / u_resolution).x
			- 4.0 * currentValue;

	/*gl_FragColor = texture2D( u_bufferTexture, p/u_resolution);*/


	float delta = float(diffA * laplacian - A*B*B + feed * (1.0-A));


	float dist = distance(u_source.xy, gl_FragCoord.xy);
	gl_FragColor = vec4(vec3(currentValue + delta), 1.0); 
	gl_FragColor.rgb += u_source.z * max(10.0 - dist, 0.0); // draw with 10.0 radius
	/*gl_FragColor.r += 0.01;*/
}
