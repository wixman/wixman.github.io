if ( ! Detector.webgl ) Detector.addGetWebGLMessage();



// multiplies the texture resolution 
var scale = 2;

var camera, controls, scene, renderer, textureA, textureB, quad;

var params = {
	//speed: 10,
	mainColor: "#FFAE23",
	bgColor: "#00AEF3"
};

var uniforms = {
	"u_resolution" : {type: 'v2',value:new THREE.Vector2(window.innerWidth/scale, window.innerHeight/scale)},
	"u_mcolor" : { type: "c", value: new THREE.Color( params.outColor ) },
	"u_bgcolor" : { type: "c", value: new THREE.Color( params.outColor ) },
	"u_startFrame" : { type: "i", value: 1 },
	"u_delta" : { type: "f", value: 1.0 },
	"u_source" : { type:"v3", value: new THREE.Vector3(0,0,0) },
	"u_texture" : { type: "t", value: undefined }
};

var fs_render_source = null;
var fs_timestep_source = null;
var vs_source = null;

init();
render();

function init() {
	initShaders();

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


	// MATERIALS 
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

	// GEO
	var geometry = new THREE.PlaneGeometry( window.innerWidth, window.innerHeight);
	quad = new THREE.Mesh( geometry, renderMaterial );
	scene.add(quad);


	// TEXTURES
	textureA = new THREE.WebGLRenderTarget(window.innerWidth/scale, window.innerHeight/scale, {
											minFilter: THREE.LinearFilter, 
											magFilter: THREE.NearestFilter,
											format: THREE.RGBAFormat, 
											type: THREE.FloatType});
	textureB = new THREE.WebGLRenderTarget(window.innerWidth/scale, window.innerHeight/scale, {
											minFilter: THREE.LinearFilter, 
											magFilter: THREE.NearestFilter,
											format: THREE.RGBAFormat, 
											type: THREE.FloatType});


	// MOUSE
	var mouseDown = false;
	function UpdateMousePosition(X,Y){
		var mouseX = X;
		var mouseY = window.innerHeight - Y;
		uniforms.u_source.value.x = mouseX/scale;
		uniforms.u_source.value.y = mouseY/scale;
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
	//gui.add(params, 'speed');
	gui.addColor(params, 'mainColor');
	gui.addColor(params, 'bgColor');

	
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
}


function onWindowResize() {

	camera.aspect = window.innerWidth / window.innerHeight;
	camera.updateProjectionMatrix();

	renderer.setSize( window.innerWidth, window.innerHeight );

	uniforms.u_resolution.value.x = window.innerWidth/scale;
	uniforms.u_resolution.value.x = window.innerHeight/scale;
};


var time;
function render() {
	requestAnimationFrame( render );

	// ‘delta’ time-based animation
    var now = new Date().getTime(),
        dt = now - (time || now);
    time = now;
	dt *= 0.05; // scale it down an arbitrary amount
	if(dt>0.8 || dt < 0.0)
		dt = 0.8;	
	
	uniforms.u_delta.value = dt;


	// run sim timestep
	quad.material = timestepMaterial;	

	for(var i=0; i<10; ++i)
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

		//uniforms.brush.value = new THREE.Vector2(-1, -1);
	}

	uniforms.u_startFrame.value = 0;

	quad.material = renderMaterial;
	renderer.render( scene, camera );
	



	stats.update();
	uniforms.u_mcolor.value = new THREE.Color( params.mainColor );
	uniforms.u_bgcolor.value = new THREE.Color( params.bgColor );
}

