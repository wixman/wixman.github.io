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
uniform bool      iNotST;

// lighting toggles
uniform bool      iSunOn;
uniform bool      iSkyOn;
uniform bool      iBounceOn;
uniform bool      iOccOn;
uniform bool      iFogOn;

vec3   camRight = cross(iCamFwd, iCamUp);
float  _zNear  = 0.0; // Near plane distance from camera
float  _zFar = 15.0; // Far plane distance from camera
float  _focalLength = 1.67; // Distance between eye and image-plane

float d1;
float d2;

// our rotation matrix for the animated cube
mat3 m1= mat3(cos(iGlobalTime * 0.6), 0.0, sin(iGlobalTime * 0.6),
			  0.0, 1.0, 0.0,
			  -sin(iGlobalTime * 0.6), 0.0, cos(iGlobalTime * 0.6) );        

mat3 m2= mat3(cos(iGlobalTime * 0.6), -sin(iGlobalTime * 0.6), 0.0,
			  sin(iGlobalTime * 0.6), cos(iGlobalTime * 0.6), 0.0,
			  0.0, 0.0, 1.0 );        

// return maximum component of a 3f vector
float maxcomp(in vec3 p ) 
{ 
	return max(p.x,max(p.y,p.z));
}

// create a signed distance field of a box
float sdBox( vec3 p, vec3 b )
{
  vec3  di = abs(p) - b;
  float mc = maxcomp(di);
  return min(mc,length(max(di,0.0)));
}


// main distance function
vec4 distanceFunction(vec3 p )
{
   	p *= m1 * m2; 
	
	float dist = sdBox( p, vec3(1.0) );

	vec4 res = vec4(dist, 1.0, 0.0, 0.0);

    float s = 1.0; // initialize scale to zero
    
    for( int m=0; m<4; m++ )
    {
        vec3 a = mod( p*s, 2.0 )-1.0; // repeat
        s *= 3.0; // shrink cross every iteration
        vec3 r = abs(1.0 - 3.0*abs(a));
        float dista = max(r.x,r.y);
        float distb = max(r.y,r.z);
        float distc = max(r.z,r.x);
        float c = (min(dista,min(distb,distc))-1.0)/s;

        if( c>dist )
        {
          dist = c;
		  // X component is distance
		  // Y component is occlusion
		  res = vec4( dist, min(res.y, 0.2*dista*distb*distc), (1.0+float(m))/4.0, 0.0);
        }
    }

    return res;
}


vec4 intersect(vec3 rayOrigin, vec3 rayDir)
{
    float t = 0.0; // marching distance
	vec4 res = vec4(-1.0);	
	vec4 h = vec4(1.0);

	// create 3x3 rotation matrix
    for(int i = 0; i < 64; ++i)
    {
		if( h.x < 0.005 || t > 10.0 ) 
			break;
		
		vec3 p = rayOrigin + rayDir * t; // our position along the ray
        h = distanceFunction(p);
		res = vec4(t, h.yzw);
        t += h.x; 
	}
   
	if( t > 10.0 )
		res = vec4(-1.0);

    return res;
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


float softshadow( in vec3 rayOrigin, in vec3 rayDir, float mint, float k )
{
    float res = 1.0;
    float t = mint;
	float h = 1.0;
    for( int i=0; i<32; i++ )
    {
		// raymarch along the light vector
        h = distanceFunction(rayOrigin + rayDir*t).x;
        res = min( res, k*h/t );
		t += clamp( h, 0.005, 0.1 );
    }
    return clamp(res,0.0,1.0);
}

vec3 sunCol = vec3(258.0, 228.0, 170.0) / 3555.0;
vec3 sunDir = normalize(vec3(0.93, 1.0, -1.5));
vec3 skyCol = vec3(0.09, 0.43, 0.95);
vec3 groundCol = vec3(0.99, 0.95, 0.85);


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
	vec4 distAndMat = intersect(rayOrigin, rayDir);
	
	if( distAndMat.x > 0.0 )
	{
		vec3 p = rayOrigin + distAndMat.x * rayDir; // sample position in space
		vec3 n = calcNormal(p); 
		float occ = distAndMat.y;
		float sunShadow = softshadow(p, sunDir, 0.01, 1.0); 

		occ = 1.0 - ( (1.0 - occ) * float(iOccOn) );
		sunShadow = 1.0 - ( (1.0 - sunShadow) * float(iSunOn) );

		col = (sunCol * float(iSunOn) ) * sunShadow * 25.0 ; //* dot(sunDir, n); // main sunlight
		col += (skyCol * (n.y * 0.5 + 0.5) * occ) * float(iSkyOn); // add ambient from sky
		col += (groundCol * 0.3 * (-n.y * 0.5 + 0.5) * occ) * float(iBounceOn); // add ambient light from ground
		// add fog
		if(iFogOn)
		col = mix(vec3(0.98) + min(vec3(0.25),GetSunColor(rayDir, sunDir))*2.0, col, 
				exp(-distAndMat.x*0.085));

	}

	else
	{
		// render bg ramp
		col = mix(groundCol, skyCol, 0.5 + 0.5 * rayDir.y * 2.0);  
        col += GetSunColor(rayDir, sunDir);
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

	fragColor = mix(color, vigColor, pow((1.0 - vig), 5.0));
}


// stuff above to make it shadertoy compatible
void main( void ){
	vec4 color = vec4(0.0,0.0,0.0,1.0);
	mainImage( color, gl_FragCoord.xy );
	color.w = 1.0;
	gl_FragColor = color;
}	
