#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
// shadertoy compatible uniforms
uniform float     iGlobalTime;
uniform vec3	  iResolution;

// my uniforms
uniform vec3	  iCamPos;
uniform vec3	  iCamFwd;
uniform vec3	  iCamUp;

// lighting toggles
uniform bool      iSunOn;
uniform bool      iSkyOn;
uniform bool      iBounceOn;
uniform bool      iOccOn;
uniform bool      iFogOn;

// CAMERA SETUP 
vec3   camRight = cross(iCamFwd, iCamUp);
float  _zNear  = 0.0; // Near plane distance from camera
float  _zFar = 15.0; // Far plane distance from camera
float  _focalLength = 1.67; // Distance between eye and image-plane

// PERFORMANCE/QUALITY VARIABLES
const int RM_ITERATIONS = 64;
const int BULB_ITERATIONS = 7;


// light setup
vec3 sunCol = vec3(258.0, 128.0, 0.0) / 3555.0;
vec3 sunDir = normalize(vec3(0.93, 1.0, -1.5));
vec3 skyCol = vec3(0.2, 0.2, 1.95);
vec3 groundCol = vec3(0.99, 0.45, 0.85);


// our rotation matrix for the animations
mat3 m1= mat3(cos(iGlobalTime * 0.02 + 1.3), 0.0, sin(iGlobalTime * 0.02 + 1.3),
			  0.0, 1.0, 0.0,
			  -sin(iGlobalTime * 0.02 + 1.3), 0.0, cos(iGlobalTime * 0.02 + 1.3) );        

mat3 m2= mat3(cos(iGlobalTime * 0.02 + 1.0), -sin(iGlobalTime * 0.02 + 1.0), 0.0,
			  sin(iGlobalTime * 0.02 + 1.0), cos(iGlobalTime * 0.02 + 1.0), 0.0,
			  0.0, 0.0, 1.0 );        


vec2 bulb(vec3 p, float power) {
	/*p.xyz = p.xzy;*/
	vec3 z = p;
	vec3 dz=vec3(0.0);
	float r, theta, phi;
	float dr = 1.0;
	
	float t0 = 1.0;
	
	for(int i = 0; i < BULB_ITERATIONS; ++i) {
		r = length(z);
		if(r > 2.0) continue;
		theta = atan(z.y / z.x);
		phi = asin(z.z / r);
		
		dr = pow(r, power - 1.0) * dr * power + 1.0;
	
		r = pow(r, power);
		theta = theta * power;
		phi = phi * power;
		
		z = r * vec3(cos(theta)*cos(phi), sin(theta)*cos(phi), sin(phi)) + p;
		
		t0 = min(t0, r);
	}
	return vec2(0.5 * log(r) * r / dr, t0);
}


vec2 distanceFunction(vec3 p) // where we composite our distance functions and create a final distance
{
	vec3 p1 = ((p + vec3(0.0, 1.0, 4.0)) * 0.5) *  m2;

	vec3 p2 = p * m1;
	p2.xyz = p2.xzy;

	float power1 = 7.0  + sin(iGlobalTime * 0.1);
	float power2 = 8.0  + cos(iGlobalTime * 0.1 + 10.0);

	vec2 bulb1 = bulb(p1, power1);
	vec2 bulb2 = bulb(p2, power2);

	float bulbDist = min(bulb1.x, bulb2.x);
	float AO = bulb1.y * bulb2.y;
	
	return vec2(bulbDist, AO);
}


vec3 intersect(vec3 rayOrigin, vec3 rayDir)
{
    float t = 0.0; // marching distance
	vec4 res = vec4(-1.0);	
	vec2 h = vec2(1.0);

    for(int i = 0; i < RM_ITERATIONS; ++i)
    {
		if( h.x < 0.0001 || t > 5.0 ) 
			break;
		
		vec3 p = rayOrigin + rayDir * t; // our position along the ray
        h = distanceFunction(p);
        t += h.x; 
	}
   
	if( t>5.0 ) 
		t=-1.0;

    return vec3(t, h.y, 0.0);
}


vec3 calcNormal(in vec3 p)
{
    vec3 eps = vec3(.001,0.0,0.0);
    vec3 n;
    n.x = distanceFunction(p+eps.xyy).x - distanceFunction(p-eps.xyy).x;
    n.y = distanceFunction(p+eps.yxy).x - distanceFunction(p-eps.yxy).x;
    n.z = distanceFunction(p+eps.yyx).x - distanceFunction(p-eps.yyx).x;
    return normalize(n);
}


float softShadow( in vec3 rayOrigin, in vec3 rayDir, float mint, float k )
{
    float res = 1.0;
    float t = mint;
	float h = 1.0;
    for( int i=0; i<32; i++ )
    {
		// raymarch along the light vector
        h = (distanceFunction(rayOrigin + rayDir*t)).x;
        res = min( res, k*h/t );
		t += clamp( h, 0.005, 0.1 );
    }
    return clamp(res,0.0,1.0);
}


vec3 GetSunColor(vec3 rayDir, vec3 sunDir)
{
	vec3 localRay = normalize(rayDir);
	float dist = 1.0 - (dot(localRay, sunDir) * 0.5 + 0.5);
	float sunIntensity = 0.05 / dist;
    sunIntensity += exp(-dist*12.0)*300.0;
	sunIntensity = min(sunIntensity, 40000.0);
	return sunCol * sunIntensity*0.025;
}


vec3 render(in vec3 rayOrigin, in vec3 rayDir)
{
	// background gradient based on normalized y component of ray direction
	vec3 col;  
	vec3 dist = (intersect(rayOrigin, rayDir));
	
	if( dist.x > 0.0 )
	{
		vec3 p = rayOrigin + dist.x * rayDir; // sample position in space
		vec3 n = calcNormal(p); 
		
		float occ = dist.y; 
		occ = pow(clamp(occ, 0.0, 1.0), 0.45);	
		float sunShadow = softShadow(p, sunDir, 0.01, 1.0); 

		occ = 1.0 - ( (1.0 - occ) * float(iOccOn) );
		sunShadow = 1.0 - ( (1.0 - sunShadow) * float(iSunOn) );


		// surface color
		col = mix(vec3(0.8, 0.1, 0.3), vec3(0.1, 0.0, 0.5), abs(cos(dist.y*8.0))) * occ;

		col += (sunCol * float(iSunOn) ) * sunShadow * 25.0 ; //* dot(sunDir, n); // main sunlight
		col += (skyCol * (n.y * 0.5 + 0.5) * occ) * float(iSkyOn); // add ambient from sky
		col += (groundCol * 0.3 * (-n.y * 0.5 + 0.5) * occ) * float(iBounceOn); // add ambient from ground
		

		// add fog
		if(iFogOn)
		col = mix(vec3(0.58) + min(vec3(0.25),GetSunColor(rayDir, sunDir))*15.0, col, 
				exp(-dist.x*0.085));

	}

	else
	{
		// render bg ramp
		col = mix(groundCol, skyCol, 0.5 + 0.5 * rayDir.y * 2.0);  
        col += GetSunColor(rayDir, sunDir) * 3.0;
	}

	return col;
}


void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
	
	vec2 uvn = fragCoord.xy / iResolution.xy; // 0 to 1 position
    vec2 uv = uvn * 2.0 - 1.0;  // transform coordinates to -1 to 1
    uv.x *= iResolution.x / iResolution.y;
    
    // ray direction
	vec3 rayDir = normalize(iCamFwd * _focalLength + (camRight * uv.x + iCamUp * uv.y));                  
	vec3 rayOrigin = iCamPos;

	vec3 col = render(rayOrigin, rayDir);

	vec4 color = vec4(col, 1.0);
    
    // vignette  
	float OuterVig = 1.0; // Position for the Outer vignette
	float InnerVig = 0.05; // Position for the inner Vignette Ring
	vec2 center = vec2(0.5, 0.5); // Center of Screen
	float dist  = distance(center, uvn) * 1.414213; // Distance  between center and the current Uv. 
	float vig = clamp((OuterVig-dist) / (OuterVig-InnerVig), 0.0, 1.0); 
	vec4 vigColor = vec4(0.062745, 0.031372, 0.02353, 1.0);

	fragColor = clamp(mix(color, vigColor, pow((1.0 - vig), 5.0)), 0.0, 1.0);
}


// stuff above to make it shadertoy compatible
void main( void ){
	vec4 color = vec4(0.0,0.0,0.0,1.0);
	mainImage( color, gl_FragCoord.xy );
	color.w = 1.0;
	gl_FragColor = color;
}	
