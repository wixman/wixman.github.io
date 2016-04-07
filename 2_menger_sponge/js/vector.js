// simple vector class
//
// only added add/subtract methods for now

function Vector(x, y, z) {
  this.x = x || 0;
  this.y = y || 0;
  this.z = z || 0;
}


Vector.prototype = {

	add: function(v) {
		if (v instanceof Vector) 
			return new Vector(this.x + v.x, this.y + v.y, this.z + v.z);
		else 
			return new Vector(this.x + v, this.y + v, this.z + v);
	},

	subtract: function(v) {
		if (v instanceof Vector) 
			return new Vector(this.x - v.x, this.y - v.y, this.z - v.z);
		else 
			return new Vector(this.x - v, this.y - v, this.z - v);
	},

	multiply: function(v) {
		if (v instanceof Vector) 
			return new Vector(this.x * v.x, this.y * v.y, this.z * v.z);
		else 
			return new Vector(this.x * v, this.y * v, this.z * v);
	},


}
