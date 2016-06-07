uniform vec3 u_color;

void main() {
  /*gl_FragColor = vec4(1.0, 1.0, 0.0, 1.0); // A*/
  gl_FragColor = vec4(u_color, 1.0); // A
}
