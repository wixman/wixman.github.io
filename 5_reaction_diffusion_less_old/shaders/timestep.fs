varying vec2 vUv;

uniform sampler2D u_texture;
uniform vec3 u_source;
uniform float u_timestep;
uniform vec2 brush;
uniform float u_screenWidth;
uniform float u_screenHeight;
uniform vec2 u_resolution;

// kill/feed rate of each chemical
/*const float feed = 0.0545;*/
/*const float kill = 0.062;*/
float feed = 0.037; 
float kill = 0.06;

vec2 texel = vec2(1.0/u_screenWidth, 1.0/u_screenHeight);
float step_x = 1.0/u_screenWidth;
float step_y = 1.0/u_screenHeight;
	
// diffuse rate of each chemical
/*const float diffA = 0.2097;*/
/*const float diffB = 0.105;*/

float delta = 1.0;
/*float delta = 1.0;*/

void main() {
	vec2 p = gl_FragCoord.xy; // position

	if(brush.x < -5.0)
	{
		gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
		return;
	}

// RETRY
    vec2 uv = texture2D(u_texture, vUv).rg;
	vec2 uv0 = texture2D(u_texture, vUv+vec2(-step_x, 0.0)).rg;
	vec2 uv1 = texture2D(u_texture, vUv+vec2(step_x, 0.0)).rg;
	vec2 uv2 = texture2D(u_texture, vUv+vec2(0.0, -step_y)).rg;
	vec2 uv3 = texture2D(u_texture, vUv+vec2(0.0, step_y)).rg;

	vec2 lapl = (uv0 + uv1 + uv2 + uv3 - 4.0*uv);//10485.76;
	float du = /*0.00002*/0.2097*lapl.r - uv.r*uv.g*uv.g + feed*(1.0 - uv.r);
	float dv = /*0.00001*/0.105*lapl.g + uv.r*uv.g*uv.g - (feed+kill)*uv.g;
	vec2 dst = uv + delta*vec2(du, dv);

	if(brush.x > 0.0)
                {
                    vec2 diff = (vUv - brush)/texel;
                    float dist = dot(diff, diff);
                    if(dist < 5.0)
                        dst.g = 0.9;
                }

	gl_FragColor = vec4(dst.r, dst.g, 0.0, 1.0);

}
