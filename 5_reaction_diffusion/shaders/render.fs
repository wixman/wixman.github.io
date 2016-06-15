#extension GL_OES_standard_derivatives : enable

varying vec2 v_uv;

uniform vec2 u_resolution;
uniform sampler2D u_texture;
uniform vec3 u_mcolor;
uniform vec3 u_bgcolor;
uniform vec3 u_source;


// fit
float fit(float value, float oMin, float oMax, float nMin, float nMax){
	return clamp(((value - oMin)/ (oMax - oMin) + nMin) * (nMax - nMin), nMin, nMax);
}


float tex(const in vec2 p)
{
    return texture2D(u_texture, p).y;
} 


void main()
{
	vec2 step = 1.0 / u_resolution.xy;
	vec2 step_x = vec2(step.x, 0.0);
	vec2 step_y = vec2(0.0, step.y);

	float value = tex(v_uv);

	// calculate normal
	float power = 3.0;
	float v1 = pow(tex(v_uv + step_x), power);
	float v2 = pow(tex(v_uv - step_x), power);
	float v3 = pow(tex(v_uv - step_y), power);
	float v4 = pow(tex(v_uv - step_y), power);
	float dx = v2 - v1;
	float dy = v3 - v4;
	vec3 normal = normalize(vec3(dx, dy, 0.01));

	vec3 lightcol = vec3(1.0, 1.0, 1.0); // light color
    vec3 light = normalize(vec3(1.0, 1.0, 0.0)); // light vector
    vec3 i = vec3(0.0,0.0,1.0); // viewing vector

    float shading = clamp(dot(normal,light),0.5,1.0);
    vec3 finalshading = pow(shading, 2.0) * u_mcolor *vec3(2.0); 

    float spec = pow(clamp(dot(light,reflect(vec3(0.0,0.0,-1.0),normal)),0.0,1.0), 8.0);
	vec3 finalspec = spec * value * vec3(35.0);


	float bgmask = fit(value, 0.05, 0.1, 0.0, 1.0); 
	float ssmask = (1.0 - fit(value, 0.05, 0.3, 0.0, 1.0)) * bgmask; 
	vec3 ss = ssmask * u_mcolor * 1.0; // fake sub surface

	vec3 finalcol = mix(u_bgcolor, u_mcolor * 0.3, bgmask) + finalshading * bgmask + finalspec + ss ; 

	gl_FragColor = vec4(finalcol, 1.0);
}
