#extension GL_OES_standard_derivatives : enable

varying vec2 vUv;

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
	float value = texture2D(u_texture, vUv).g;

    //Normals
    float dx = dFdx(value*1.0)*u_resolution.x/80.0*value*10.0;
    float dy = dFdy(value*1.0)*u_resolution.x/80.0*value*10.0;
    vec3 normal = normalize(vec3(dx,dy,sqrt(clamp(1.0-dx*dx-dy*dy,0.0,1.0))));
	
	vec3 lightcol = vec3(0.5, 0.5, 0.5);
    vec3 light = normalize(vec3(u_resolution.xy-u_source.xy,48.0)-vec3(u_resolution.xy/2.0,0.0));
    vec3 i = vec3(0.0,0.0,1.0);

    float shading = clamp(dot(normal,light),0.0,1.0);
    vec3 finalshading = pow(shading, 2.0) * u_mcolor; 

    vec3 spec = pow(clamp(dot(light,reflect(vec3(0.0,0.0,-1.0),normal)),0.0,1.0),25.0)*vec3(0.5,0.5,0.6);
	vec3 finalspec = spec* value * 4.0;	

	float mask = fit(value, 0.01, 0.14, 0.0, 1.0); 
	vec3 finalcol = mix(u_bgcolor, u_mcolor, mask) + finalshading * value + finalspec; 

	gl_FragColor = vec4(finalcol, 1.0);
}
