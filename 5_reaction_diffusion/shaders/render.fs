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

void main()
{
	float value = texture2D(u_texture, v_uv).g;

    // Gradient Normals
    float dx = dFdx(pow(value, 1.0)*1.0)*u_resolution.x * value * 0.2;
    float dy = dFdy(pow(value, 1.0)*1.0)*u_resolution.y * value * 0.2;
    vec3 normal = normalize(vec3(dx,dy,sqrt(clamp(1.0-dx*dx-dy*dy,0.0,1.0))));
	
	vec3 lightcol = vec3(1.0, 1.0, 1.0); // light color
    /*vec3 light = normalize(vec3(u_resolution.xy-u_source.xy,48.0)-vec3(u_resolution.xy/2.0,0.0));*/
    vec3 light = normalize(vec3(1.0, 1.0, 0.0)); // light vector
    vec3 i = vec3(0.0,0.0,1.0); // viewing vector

    float shading = clamp(dot(normal,light),0.5,1.0);
    vec3 finalshading = pow(shading, 1.0) * u_mcolor *vec3(1.0); 

    float spec = pow(clamp(dot(light,reflect(vec3(0.0,0.0,-1.0),normal)),0.0,1.0),5.0);
	vec3 finalspec = spec * value * vec3(5.0);

	float bgmask = fit(value, 0.05, 0.1, 0.0, 1.0); 
	float ssmask = (1.0 - fit(value, 0.05, 0.3, 0.0, 1.0)) * bgmask; 
	vec3 ss = ssmask * u_mcolor * 1.0; // fake sub surface

	vec3 finalcol = mix(u_bgcolor, vec3(0.0), bgmask) + finalshading * bgmask + finalspec + ss ; 

	gl_FragColor = vec4(finalcol, 1.0);
}
