#extension GL_OES_standard_derivatives : enable

varying vec2 vUv;

uniform vec2 u_resolution;
uniform sampler2D u_texture;
uniform vec3 u_mcolor;
uniform vec3 u_bgcolor;
uniform vec3 u_source;


// fit
float fit(float value, float oMin, float oMax, float nMin, float nMax){
	return ((value - oMin)/ (oMax - oMin) + nMin) * (nMax - nMin);
}

void main()
{
	// steps for sampling
	float step_x = 1.0/(u_resolution.x);
	float step_y = 1.0/(u_resolution.y);

	/*vec3 tex = texture2D(u_texture, vUv).xyz;*/
	/*float value = texture2D(u_texture, vUv).g;*/
	float value = texture2D(u_texture, vUv).g;


	// get the gradient
	float sample_x1 = texture2D(u_texture, vUv+vec2(step_x, 0.0)).g;
	float sample_y1 = texture2D(u_texture, vUv+vec2(0.0, step_y)).g;
	float sample_x2 = texture2D(u_texture, vUv+vec2(step_x * 2.0, 0.0)).g;
	float sample_y2 = texture2D(u_texture, vUv+vec2(0.0, step_y* 2.0)).g;
	float sample_x3 = texture2D(u_texture, vUv+vec2(step_x * 2.0, 0.0)).g;
	float sample_y3 = texture2D(u_texture, vUv+vec2(0.0, step_y* 2.0)).g;

    
    //Normals
    
    float dx = dFdx(value*2.0)*u_resolution.x/80.0*value*5.0;
    float dy = dFdy(value*2.0)*u_resolution.x/80.0*value *5.0;
     
    vec3 vNormal = normalize(vec3(dx,dy,sqrt(clamp(1.0-dx*dx-dy*dy,0.0,1.0))));
    

	float sample_x = (sample_x1 + sample_x2 + sample_x3) / 3.0;
	float sample_y = (sample_y1 + sample_y2 + sample_y3) / 3.0;
	vec2 grad = normalize(vec2(value-sample_x1, value - sample_y1));
	
	value = fit(value, 0.0, 0.4, 0.0, 1.0);
	vec3 col2 = mix(vec3(0.9, 0.9, 0.9), vec3(0.2, 0.2, 0.3), pow(value,2.0)*2.);
	/*vec3 col = mix(u_bgcolor, u_mcolor, value*2.5);*/
	/*col = mix(col, col2, u_source.x/u_resolution.x);*/
	/*col = vec3(vUv, 1.0);*/


	// sample the gradient


	// perterb normal with gradient

	// light light light!

	
	// ok lets try and make a normal map	
	/*col = vec3(grad, 1.0) * value;*/
	/*if(vUv.x < 0.5)*/
		/*col = vec3(sample_y1);*/
	/*else*/
		/*col = vec3(value);*/

	/*grad.x = max(grad.x, 0.0);*/
	/*vec3 spec = vec3(grad.x, grad.x, grad.x)* 0.01;*/
	/*col2 += vec3(spec);*/

	/*value = clamp(value, 0.0, 1.0);*/

	gl_FragColor = vec4(vNormal, 1.0);
	/*gl_FragColor = vec4(grad.x) * value;*/

}
