if ( ! Detector.webgl ) Detector.addGetWebGLMessage();

var camera, controls, scene, renderer, textureA, textureB, quad;

var params = {
	speed: 10,
	outColor: "#FFAE23"
};

var uniforms = {
	"u_resolution" : {type: 'v2',value:new THREE.Vector2(window.innerWidth, window.innerHeight)},
	"u_color" : { type: "c", value: new THREE.Color( params.outColor ) },
	"u_startFrame" : { type: "i", value: 1 },
	"u_texture" : { type: "t", value: undefined }
};


var fs_render_source = null;
var fs_timestep_source = null;
var vs_source = null;

init();
animate();

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
	var geometry = new THREE.PlaneBufferGeometry( window.innerWidth, window.innerHeight);
	quad = new THREE.Mesh( geometry, renderMaterial );
	scene.add(quad);


	// TEXTURES
	textureA = new THREE.WebGLRenderTarget(window.innerWidth, window.innerHeight, {
											minFilter: THREE.LinearFilter, 
											magFilter: THREE.NearestFilter,
											format: THREE.RGBAFormat, 
											type: THREE.FloatType});
	textureB = new THREE.WebGLRenderTarget(window.innerWidth, window.innerHeight, {
											minFilter: THREE.LinearFilter, 
											magFilter: THREE.NearestFilter,
											format: THREE.RGBAFormat, 
											type: THREE.FloatType});

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

};


function animate() {

	requestAnimationFrame( animate );

	stats.update();

	uniforms.u_color.value = new THREE.Color( params.outColor );

	var timer = Date.now() * 0.0001;
	
	render();


}

function render() {
	quad.material = timestepMaterial;	

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

	uniforms.u_startFrame.value = 0;

	quad.material = renderMaterial;
	renderer.render( scene, camera );
}

