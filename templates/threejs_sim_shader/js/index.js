if ( ! Detector.webgl ) Detector.addGetWebGLMessage();

var camera, controls, scene, renderer, textureA, textureB, bufferScene, bufferMaterial, quad;

var params = {
	speed: 10,
	outColor: "#FFAE23"
};

var uniforms = {
	"u_resolution" : {type: 'v2',value:new THREE.Vector2(window.innerWidth, window.innerHeight)},
	"u_color" : { type: "c", value: new THREE.Color( params.outColor ) }, // single Color
	"u_source" : { type:"v3", value: new THREE.Vector3(0,0,0) },
	"u_bufferTexture" : { type: "t", value: textureA }
}

// setup shaders
var fs_source = null,
	vs_source = null;
initShaders();

init();
animate();


function init() {

	// SCENE	
	scene = new THREE.Scene();
	scene.fog = new THREE.FogExp2( 0xcccccc, 0.002 );

	
	// RENDERER
	renderer = new THREE.WebGLRenderer();
	renderer.setClearColor( scene.fog.color );
	renderer.setPixelRatio( window.devicePixelRatio );
	renderer.setSize( window.innerWidth, window.innerHeight );

	var container = document.getElementById( 'container' );
	container.appendChild( renderer.domElement );

	
	// CAMERA
	camera = new THREE.OrthographicCamera( window.innerWidth / - 2, 
		window.innerWidth / 2, window.innerHeight / 2, window.innerHeight / - 2, 1, 1000 );
	camera.position.z = 2;


	// SCENE 
	//var plane = new THREE.PlaneBufferGeometry( window.innerWidth, window.innerHeight);
	//scene.add(quad);


	// BUFFER
	bufferScene = new THREE.Scene();

	textureA = new THREE.WebGLRenderTarget(window.innerWidth, window.innerHeight, {minFilter:
											   THREE.LinearFilter, magFilter: THREE.NearestFilter});
	textureB = new THREE.WebGLRenderTarget(window.innerWidth, window.innerHeight, {minFilter:
											   THREE.LinearFilter, magFilter: THREE.NearestFilter});

	// pass textureA to shader
	bufferMaterial = new THREE.ShaderMaterial({ 
					uniforms: uniforms,
					vertexShader: vs_source,
					fragmentShader: fs_source
				});

	var plane = new THREE.PlaneBufferGeometry( window.innerWidth, window.innerHeight);
	var bufferObject = new THREE.Mesh( plane, bufferMaterial );
	bufferScene.add(bufferObject);

	// draw textureB to screen
	finalMaterial = new THREE.MeshBasicMaterial({map: textureB});
	quad = new THREE.Mesh(plane, finalMaterial );
	scene.add(quad);


	// MOUSE
	var mouseDown = false;
	function UpdateMousePosition(X,Y){
		var mouseX = X;
		var mouseY = window.innerHeight - Y;
		bufferMaterial.uniforms.u_source.value.x = mouseX;
		bufferMaterial.uniforms.u_source.value.y = mouseY;
	}
	document.onmousemove = function(event){
		UpdateMousePosition(event.clientX,event.clientY)
	}

	document.onmousedown = function(event){
		mouseDown = true;
		bufferMaterial.uniforms.u_source.value.z = 0.1;
	}
	document.onmouseup = function(event){
		mouseDown = false;
		bufferMaterial.uniforms.u_source.value.z = 0;
	}


	// GUI
	var gui = new dat.GUI({
		height : 5 * 32 - 1					
		});
	gui.add(params, 'speed');
	gui.addColor(params, 'outColor');

	
	// STATS	
	stats = new Stats();
	stats.domElement.style.position = 'absolute';
	stats.domElement.style.top = 'auto';
	stats.domElement.style.bottom = '0px';
	stats.domElement.style.zIndex = 100;
	container.appendChild( stats.domElement );

	renderer.setClearColor( 0xdddddd, 1);
	renderer.render( scene, camera );
		
	window.addEventListener( 'resize', onWindowResize, false );
}


function initShaders()
{
	//get shader sources with XMLHttpRequestObject

	var xhr = new XMLHttpRequest();
	//synchronous request - false third parameter
	xhr.open('GET', './shaders/base.vs', false); // UPDATE THE PATH HERE
	//overriding the mime type is required 
	xhr.overrideMimeType('text/plain');
	xhr.send(null);

	if (xhr.readyState == xhr.DONE) {
		if(xhr.status === 200)
		{
			vs_source = xhr.responseText;
		} else {  
			console.error("Error: " + xhr.statusText);  
		}  
	}

	xhr.open('GET', './shaders/base.fs', false);  // UPDATE THE PATH HERE
	xhr.send(null);

	if (xhr.readyState == xhr.DONE) {
		if(xhr.status === 200)
		{
			fs_source = xhr.responseText;
		} else {  
			console.error("Error: " + xhr.statusText);  
		}
	}  
}


function onWindowResize() {

	camera.aspect = window.innerWidth / window.innerHeight;
	camera.updateProjectionMatrix();

	renderer.setSize( window.innerWidth, window.innerHeight );

};


function animate() {

	requestAnimationFrame( animate );

	stats.update();

	uniforms.u_color.value = new THREE.Color( params.outColor );

	var timer = Date.now() * 0.0001;
	
	render();


}


function render() {
	// Draw to textureB
	renderer.render(bufferScene, camera, textureB, true);

	// Swap textureA and B
	var t = textureA;
	textureA = textureB;
	textureB = t;
	quad.material.map = textureB;
	uniforms.u_bufferTexture.value = textureA;

	renderer.render( scene, camera );
}
