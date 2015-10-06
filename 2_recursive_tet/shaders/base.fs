#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
// shadertoy compatible uniforms
uniform float     iGlobalTime;
uniform vec3	  iResolution;

// my uniforms
uniform vec3	  iCamPos;
uniform bool       iNotST;


float d1;
float d2;

float map(vec3 p )
{
    const int Iterations = 14;
    const float Scale = 1.85;
    const float Offset = 0.45;
   
	for (int n = 0; n < Iterations; n++)
	{
      if(p.x+p.y < 0.) 
          p.xy = -p.yx; // fold 1
      if(p.x+p.z < 0.) 
          p.xz = -p.zx; // fold 2
      if(p.y+p.z < 0.) 
          p.zy = -p.yz; // fold 3	
      p = p*Scale - Offset*(Scale-1.0);
	}
	return length(p) * pow(Scale, -float(Iterations));
}

float trace(vec3 ro, vec3 rd)
{
    float t = 0.0; // marching distance
    for(int i = 0; i < 32; ++i)
    {
		vec3 p = ro + rd * t; // our position along the ray
    	
        mat3 m;
        
        mat3 m1 = mat3(cos(iGlobalTime * 0.6), 0.0, sin(iGlobalTime * 0.6),
                      0.0, 1.0, 0.0,
                      -sin(iGlobalTime * 0.6), 0.0, cos(iGlobalTime * 0.6) );        

       	mat3 m2 = mat3(cos(-iGlobalTime * 0.6), 0.0, sin(-iGlobalTime * 0.6),
                      0.0, 1.0, 0.0,
                      -sin(-iGlobalTime * 0.6), 0.0, cos(-iGlobalTime * 0.6) );        
        
        vec3 p1 = mod(p,2.5)-0.5*2.5;
        vec3 p2 = mod(p + vec3(1.25, -1.7, 0.0),2.5)-0.5*2.5;
        p1 *= m1;
        p2 *= -m2;
                
        d1 = map(p1);
        d2 = map(p2);
        float d = min(d1, d2);
        
        t += d * 0.5; }
    
    return t;

}


void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
	vec2 uvn = fragCoord.xy / iResolution.xy; // 0 to 1 position
    vec2 uv = uvn * 2.0 - 1.0;  // transform coordinates to -1 to 1
    uv.x *= iResolution.x / iResolution.y;
    
    // ray direction
    vec3 rd = normalize(vec3(uv, 1.0)); // 1.0 = 90deg FOV
                   
    float theta = iGlobalTime * 0.25;
    /*rd.xz *= mat2(cos(theta), -sin(theta), sin(theta), cos(theta));*/
   
	vec3 ro = iCamPos;
    
    float t = trace(ro, rd);
    
    // simple fog style rendering
    float fog = 1.0 / (1.0 + t * t * 0.5);
    
    // vignette  
    float OuterVig = 1.0; // Position for the Outer vignette
    float InnerVig = 0.05; // Position for the inner Vignette Ring
    vec2 center = vec2(0.5, 0.5); // Center of Screen
    float dist  = distance(center, uvn) * 1.414213; // Distance  between center and the current Uv. 
    float vig = clamp((OuterVig-dist) / (OuterVig-InnerVig), 0.0, 1.0); 
    vec4 vigColor = vec4(0.062745, 0.031372, 0.02353, 1.0);
    
    // color based on which distance field the ray ends closest to
    // multiplying above 1 gives me a nice glow effects when the shapes get closer to camera
    if(d2 < d1)
    {
		vec4 color = vec4(fog* 1.4, fog*1.3, fog * 1.8, 1.0);
        fragColor = mix(color, vigColor, 1.0 - vig);
    }
    
	else
    {
		vec4 color = vec4(fog * 1.7, fog * 1.2, fog * 1.3, 1.0);
        fragColor = mix(color, vigColor, 1.0 - vig);

    }
    
}

// stuff above to make it shadertoy compatible
void main( void ){
	vec4 color = vec4(0.0,0.0,0.0,1.0);
	mainImage( color, gl_FragCoord.xy );
	color.w = 1.0;
	gl_FragColor = color;
}	
