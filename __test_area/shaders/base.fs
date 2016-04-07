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



// return maximum component of a 3f vector
float maxcomp(in vec3 p ) 
{ 
	return max(p.x,max(p.y,p.z));
}

// create an sdf of a sphere
float sdSphere( in vec3 p, in vec4 s )
{
    return length(p-s.xyz) - s.w;
}

// create an sdf of a ellipsoid
float sdEllipsoid( in vec3 p, in vec3 c, in vec3 r )
{
    return (length( (p-c)/r ) - 1.0) * min(min(r.x,r.y),r.z);
}

// create an sdf of a torus
float sdTorus( vec3 p, vec2 t )
{
    return length( vec2(length(p.xz)-t.x,p.y) )-t.y;
}

// capped cylinder
float sdCappedCylinder( vec3 p, vec2 h )
{
  vec2 d = abs(vec2(length(p.xz),p.y)) - h;
  return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

// cylinder
float sdCylinder( vec3 p, vec2 s)
{
    return max(abs(p.z) - s.y / 2.0,length(p.xy) - s.x);
}


// capped cone sdf
float sdCappedCone( in vec3 p, in vec3 c )
{
    vec2 q = vec2( length(p.xz), p.y );
    vec2 v = vec2( c.z*c.y/c.x, -c.z );
    vec2 w = v - q;
    vec2 vv = vec2( dot(v,v), v.x*v.x );
    vec2 qv = vec2( dot(v,w), v.x*w.x );
    vec2 d = max(qv,0.0)*qv/vv;
    return sqrt( dot(w,w) - max(d.x,d.y) )* sign(max(q.y*v.x-q.x*v.y,w.y));
}

// create an sdf of a box
float sdBox( vec3 p, vec3 b )
{
  vec3 d = abs(p) - b;
  return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
}

// repetition!
vec3 opRep( vec3 p, vec3 c )
{
    return mod(p,c)-0.5*c;
}

// angular repetition
vec3 opAngRep( vec3 p, float a )
{
	a = 6.2831/a; 

	vec2 polar = vec2(atan(p.y, p.x), length(p));
    polar.x = mod(polar.x + a / 2.0, a) - a / 2.0;
    
    return vec3(polar.y * vec2(cos(polar.x),sin(polar.x)), p.z);
}

// union
float opU( float d1, float d2 )
{
    return min(d1,d2);
}

// smooth union
float opSU( float a, float b, float k )
{
    float h = clamp( 0.5+0.5*(b-a)/k, 0.0, 1.0 );
    return mix( b, a, h ) - k*h*(1.0-h);
}

// subtraction
float opS( float d1, float d2 )
{
    return max(-d1,d2);
}

// intersection
float opI( float d1, float d2 )
{
    return max(d1,d2);
}

float cog(vec3 p)
{
	float gear = sdCylinder(p , vec2(1.0,0.3)); // main gear shape 

	if(gear > 0.5) // proxy optimization
		return gear;

	vec3 repTeeth = opAngRep(p, 20.0); // repeated space for teeth
	vec3 repSpokes = opAngRep(p, 6.0); // repeated space for spokes
	
	float spokes = sdBox(repSpokes - vec3(0.0, 0.0, 0.0), vec3(0.8, 0.1, 0.05));

	float teeth = opI(sdCylinder(repTeeth - vec3(1.0,0.4,0), vec2(0.5,0.25)), 
		 sdCylinder(repTeeth - vec3(1.0,-0.4,0), vec2(0.5,0.25)));
	teeth = opS(-sdCylinder(p,vec2(1.2,2.0)), teeth);
	
	gear = opU(gear, teeth);
	gear = opS(sdCylinder(p, vec2(0.7, 0.55)), gear);
   	gear = opSU(spokes, gear, 0.1);  
	gear = opSU(sdCylinder(p , vec2(0.3,0.55)), gear, 0.1); 

	// gear += sin(p.y*500.0) * 0.0002;

	gear += sin(length(p.xy) * 800.0) * 0.0005;


	return gear; 
}


// our rotation matrix for the animated cube
mat3 m1= mat3(cos(iGlobalTime * 0.6), -sin(iGlobalTime * 0.6), 0.0,
			  sin(iGlobalTime * 0.6), cos(iGlobalTime * 0.6), 0.0,
			  0.0, 0.0, 1.0 );        

// main distance function
float distanceFunction(vec3 p )
{
	vec3 rotP = p * m1;  // create rotation matrix
	float cog = cog(rotP);
    
	return cog;
}


float intersect(vec3 rayOrigin, vec3 rayDir)
{
    float t = 0.0; // marching distance
	float h = 1.0;

    for(int i = 0; i < 64; ++i)
    {
		if( h < 0.005 || t > 10.0 ) 
			break;
		
		vec3 p = rayOrigin + rayDir * t; // our position along the ray
        h = distanceFunction(p);
        t += h; 
	}
   
	if( t > 10.0 )
		t = -1.0;

    return t;
}


vec3 calcNormal(in vec3 p)
{
    vec3 eps = vec3(.001,0.0,0.0);
    vec3 n;
    n.x = distanceFunction(p+eps.xyy) - distanceFunction(p-eps.xyy);
    n.y = distanceFunction(p+eps.yxy) - distanceFunction(p-eps.yxy);
    n.z = distanceFunction(p+eps.yyx) - distanceFunction(p-eps.yyx);
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
        h = distanceFunction(rayOrigin + rayDir*t);
        res = min( res, k*h/t );
		t += clamp( h, 0.005, 0.1 );
    }
    return clamp(res,0.0,1.0);
}

vec3 sunCol = vec3(258.0, 228.0, 170.0) / 3555.0;
vec3 sunDir = normalize(vec3(0.93, 1.0, -1.5));
vec3 skyCol = vec3(0.6, 0.6, 0.6);
vec3 groundCol = vec3(0.4, 0.4, 0.4);


vec3 GetSunColor(vec3 rayDir, vec3 sunDir)
{
	vec3 localRay = normalize(rayDir);
	float dist = 1.0 - (dot(localRay, sunDir) * 0.5 + 0.5);
	float sunIntensity = 0.05 / dist;
	sunIntensity += exp(-dist*12.0)*300.0;
	sunIntensity = min(sunIntensity, 40000.0);
	return sunCol * sunIntensity*0.025;
}


float GetOcc(vec3 p, vec3 n)
{
	float stepSize = 0.01;
	float t = stepSize;
	float occ = 0.0;
	for(int i = 0; i < 10; ++i)
	{
		float d = distanceFunction(p + n * t);
		occ += t - d; // Actual distance to surface - distance field value
		t += stepSize;
	}

	return clamp(occ, 0.0, 1.0);
}


vec3 render(in vec3 rayOrigin, in vec3 rayDir)
{
	// background gradient based on normalized y component of ray direction
	vec3 col;  
	float dist = intersect(rayOrigin, rayDir);


	vec3 bgcol = mix(groundCol, skyCol, 0.5 + 0.5 * rayDir.y * 2.0);  
		 bgcol += GetSunColor(rayDir, sunDir);

	if( dist > 0.0 )
	{
		vec3 p = rayOrigin + dist * rayDir; // sample position in space
		vec3 n = calcNormal(p); 
		float occ = 1.0 - GetOcc(p, n);
		float sunShadow = softshadow(p, sunDir, 0.01, 1.0); 

		occ = 1.0 - ( (1.0 - occ) * float(iOccOn) );
		sunShadow = 1.0 - ( (1.0 - sunShadow) * float(iSunOn) );

		col = (sunCol * float(iSunOn) ) * sunShadow * 25.0 ; //* dot(sunDir, n); // main sunlight
		col += (skyCol * (n.y * 0.5 + 0.5) * occ) * float(iSkyOn); // add ambient from sky
		col += (groundCol * 0.3 * (-n.y * 0.5 + 0.5) * occ) * float(iBounceOn); // add ambient from ground

		// bronze inside	
		/*col *= mix(vec3(0.5, 0.4, 0.1), vec3(0.6, 0.6, 1.0), abs(n.z));*/

		// add fog
		if(!iFogOn)
		{
			bgcol = clamp(bgcol, vec3(0.0), vec3(1.0));
			col = mix(bgcol,  col, exp(-dist*0.085));
		}
		/*col = mix(vec3(0.98) + min(vec3(0.25),GetSunColor(rayDir, sunDir))*2.0, col, */
				/*exp(-dist*0.085));*/

	}

	else
	{
		// render bg ramp
		col = bgcol;	
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
