if ( ! Detector.webgl ) Detector.addGetWebGLMessage();

var camera, controls, scene, renderer, textureA, textureB, bufferScene, timestepMaterial, 
	renderMaterial, quad; 
	
var time = 0; 
var lastTime = 0;

var params = {
	speed: 10,
	outColor: "#FFAE23"
};



var uniforms = {
	"u_screenWidth" : { type:"f", value: undefined },
	"u_screenHeight" : { type:"f", value: undefined },
	"u_color" : { type: "c", value: new THREE.Color( params.outColor ) }, // single Color
	"u_source" : { type:"v3", value: new THREE.Vector3(0,0,0) },
	"u_timestep" : { type:"f", value: 1.0 },
	"brush" : {type: "v2", value: new THREE.Vector2(-10, -10)},
	"u_texture" : { type: "t", value: undefined }

}

// setup shaders
var vs_source = null,
	fs_render_source = null;
	fs_timestep_source = null;

initShaders();
init();


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
	camera = new THREE.OrthographicCamera(-0.5, 0.5, 0.5, -0.5, -100, 100);
    camera.position.z = 100;
	scene.add(camera);


	// SCENE 
	//var plane = new THREE.PlaneBufferGeometry( window.innerWidth, window.innerHeight);
	//scene.add(quad);


	// TEXTURES
	textureA = new THREE.WebGLRenderTarget(window.innerWidth/2, window.innerHeight/2, {
											minFilter: THREE.LinearFilter, 
											magFilter: THREE.NearestFilter,
											format: THREE.RGBAFormat, 
											type: THREE.FloatType});
	textureB = new THREE.WebGLRenderTarget(window.innerWidth/2, window.innerHeight/2, {
											minFilter: THREE.LinearFilter, 
											magFilter: THREE.NearestFilter,
											format: THREE.RGBAFormat, 
											type: THREE.FloatType});

	textureA.texture.wrapS = THREE.RepeatWrapping;
    textureA.texture.wrapT = THREE.RepeatWrapping;
    textureB.texture.wrapS = THREE.RepeatWrapping;
    textureB.texture.wrapT = THREE.RepeatWrapping;

	uniforms.u_screenWidth.value = window.innerWidth/2;
	uniforms.u_screenHeight.value = window.innerHeight/2;



	// pass textureA to shader
	timestepMaterial = new THREE.ShaderMaterial({ 
					uniforms: uniforms,
					vertexShader: vs_source,
					fragmentShader: fs_timestep_source
				});

	renderMaterial = new THREE.ShaderMaterial({ 
					uniforms: uniforms,
					vertexShader: vs_source,
					fragmentShader: fs_render_source
				});

	// make geo
	var plane = new THREE.PlaneGeometry( 1.0, 1.0);
	renderQuad = new THREE.Mesh(plane, renderMaterial );
	scene.add(renderQuad);


	// MOUSE
	var mouseDown = false;
	function UpdateMousePosition(X,Y){
		var mouseX = X;
		var mouseY = window.innerHeight - Y;
		uniforms.u_source.value.x = mouseX;
		uniforms.u_source.value.y = mouseY;
	}
	document.onmousemove = function(event){
		UpdateMousePosition(event.clientX,event.clientY)
	}

	document.onmousedown = function(event){
		mouseDown = true;
		uniforms.u_source.value.z = 0.1;
	}
	document.onmouseup = function(event){
		mouseDown = false;
		uniforms.u_source.value.z = 0;
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

	lastTime = new Date().getTime();	
	var time = 0.0;

	renderer.setClearColor( 0x00dddd, 1);
	renderer.render( scene, camera );
		

	window.addEventListener( 'resize', onWindowResize, false );


	uniforms.brush.value = new THREE.Vector2(0.5, 0.5); 

	animate();
}


function initShaders()
{
	//get shader sources with XMLHttpRequestObject

	var xhr = new XMLHttpRequest();
	//synchronous request - false third parameter
	xhr.open('GET', './shadethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</titlethree.js base shader setup</title>>>>>>>>>>>>rs/base.vs', false); // UPDATE THE PATH HERE
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

	xhr.open('GET', './shaders/render.fs', false);  // UPDATE THE PATH HERE
	xhr.send(null);

	if (xhr.readyState == xhr.DONE) {
		if(xhr.status === 200)
		{
			fs_render_source = xhr.responseText;
		} else {  
			console.error("Error: " + xhr.statusText);  
		}
	}  

	xhr.open('GET', './shaders/timestep.fs', false);  // UPDATE THE PATH HERE
	xhr.send(null);

	if (xhr.readyState == xhr.DONE) {
		if(xhr.status === 200)
		{
			fs_timestep_source = xhr.responseText;
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

var mToggled = false;
function render() {
	time = new Date().getTime();
	//time = Date().getTime();  
	var dt = (time - lastTime)/20.0;
	//console.log(dt);
	if(dt > 0.8 || dt<=0)
		 dt = 0.8;
	

	renderQuad.material = timestepMaterial;
	uniforms.u_timestep.value = dt;	
		
	for(var i=0; i<18; ++i)
	{
		if(i%2)
		{
			uniforms.u_texture.value = textureA;
			renderer.render(scene, camera, textureB, true);
			uniforms.u_texture.value = textureB;
		}
		else
		{
			uniforms.u_texture.value = textureB;
			renderer.render(scene, camera, textureA, true);
			uniforms.u_texture.value = textureA;
		}

		uniforms.brush.value = new THREE.Vector2(-1, -1);

	}

	renderQuad.material = renderMaterial;
	renderer.render(scene, camera);
	lastTime = time;	
}
