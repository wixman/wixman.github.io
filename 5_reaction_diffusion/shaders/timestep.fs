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

	vec2 uv = texture2D(u_texture, v_uv).rg;
	vec2 uv0 = texture2D(u_texture, v_uv+vec2(-step_x, 0.0)).rg;
	vec2 uv1 = texture2D(u_texture, v_uv+vec2(step_x, 0.0)).rg;
	vec2 uv2 = texture2D(u_texture, v_uv+vec2(0.0, -step_y)).rg;
	vec2 uv3 = texture2D(u_texture, v_uv+vec2(0.0, step_y)).rg;
	
	vec2 lapl = (uv0 + uv1 + uv2 + uv3 - 4.0*uv);//10485.76;
	float du = 0.2097*lapl.r - uv.r*uv.g*uv.g + u_feed*(1.0 - uv.r);
	float dv = 0.105*lapl.g + uv.r*uv.g*uv.g - (u_feed+u_kill)*uv.g;
	vec2 dst = uv + u_delta*vec2(du, dv);

	if(u_source.z > 0.0)
	{
		vec2 diff = (v_uv - u_source.xy/u_resolution)/texel;
		float dist = dot(diff, diff);
		if(dist < 5.0)
			dst.g = 0.9;
	}
	
	gl_FragColor = vec4(dst.x, dst.y, 0.0, 1.0);
}
