#ifdef GL_ES
precision highp float;
#endif
#extension GL_OES_standard_derivatives : enable
uniform float     iGlobalTime;


void main(void) {
   gl_FragColor = vec4(abs(sin(iGlobalTime * 10.0)), 0.2, 0.3, 1.0);
}
