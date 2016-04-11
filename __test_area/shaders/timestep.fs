#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
// shadertoy compatible uniforms
uniform float     iGlobalTime;
uniform vec3	  iResolution;


void main() {
    vec2 st = gl_FragCoord.xy/iResolution.xy;
    st.x *= iResolution.x/iResolution.y;

    vec3 color = vec3(1.0);
    color = vec3(st.x,st.y,abs(sin(iGlobalTime)));
	color = vec3(0.5, 1.0, 0.2);
    gl_FragColor = vec4(color,1.0);
}
