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

    //Normals
    float dx = dFdx(value*2.0)*u_resolution.x/80.0*value*10.0;
    float dy = dFdy(value*2.0)*u_resolution.x/80.0*value *10.0;
    vec3 normal = normalize(vec3(dx,dy,sqrt(clamp(1.0-dx*dx-dy*dy,0.0,1.0))));
	
	/*value = fit(value, 0.0, 0.4, 0.0, 1.0);*/

	/*vec3 col2 = mix(vec3(0.9, 0.9, 0.9), vec3(0.2, 0.2, 0.3), pow(value,2.0)*2.);*/

	vec3 bgcolor = vec3(0.3,0.3,0.3);

    vec3 light = normalize(vec3(u_resolution.xy-u_source.xy,48.0)-vec3(u_resolution.xy/2.0,0.0));
    vec3 i = vec3(0.0,0.0,1.0);

    float shading = clamp(dot(normal,light),0.0,1.0);

    vec3 spec = pow(clamp(dot(light,reflect(vec3(0.0,0.0,-1.0),normal)),0.0,1.0),25.0)*vec3(0.5,0.5,0.6);

    vec3 finalcol = bgcolor+vec3(0.4,0.4,0.35)*shading+spec*value;

	gl_FragColor = vec4(finalcol, 1.0);

}
