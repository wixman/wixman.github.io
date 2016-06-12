varying vec2 v_uv;

uniform sampler2D u_texture;
uniform vec2 u_resolution;
uniform int u_startFrame;
uniform float u_delta;
uniform vec3 u_source;
uniform float u_kill;
uniform float u_feed;

void main() {
	float step_x = 1.0/u_resolution.x;
	float step_y = 1.0/u_resolution.y;
	vec2 texel = vec2(step_x, step_y);

	if(u_startFrame == 1)
	{
		gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
		return;
	}	

	vec2 value = texture2D(u_texture, v_uv).xy;
	vec2 up = texture2D(u_texture, v_uv+vec2(0.0, step_y)).xy;
	vec2 down = texture2D(u_texture, v_uv+vec2(0.0, -step_y)).xy;
	vec2 left = texture2D(u_texture, v_uv+vec2(-step_x, 0.0)).xy;
	vec2 right = texture2D(u_texture, v_uv+vec2(step_x, 0.0)).xy;
	
	vec2 lapl = (up + down + left + right - 4.0*value); // laplacian
	float newA = 0.2097*lapl.x - value.x*value.y*value.y + u_feed*(1.0 - value.x);
	float newB = 0.105*lapl.y + value.x*value.y*value.y - (u_feed+u_kill)*value.y;
	vec2 new_value = value + u_delta*vec2(newA, newB);

	if(u_source.z > 0.0)
	{
		vec2 diff = (v_uv - u_source.xy/u_resolution)/texel;
		float sqdist = dot(diff, diff);
		if(sqdist < 4.0)
			new_value.y = 0.9;
	}
	
	gl_FragColor = vec4(new_value.x, new_value.y, 0.0, 1.0);
}
