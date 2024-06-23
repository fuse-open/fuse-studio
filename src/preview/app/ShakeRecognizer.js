var accelerometer = require("Accelerometer");
var Observable = require('FuseJS/Observable');
var rx = require('rx.all');

function add(a,b) {
	return a + b;
}

function ShakeRecognizer() {
	this.shaked = new Observable(false); 
	this.sampleData = new rx.Subject();
	this.onCount = 0;
	this.isStarted = false;

	var self = this;
	this.sampleData
		.scan(function(acc,x) 
		{ 
			if(acc.length >= 7)
				acc.shift();
			acc.push(Math.sqrt(x[1]*x[1] + x[2]*x[2]));
			return acc;
		}, [])
		.subscribe(function(y) {
			var avg = y.reduce(add, 0);
			avg /= y.length;

			var variance = y.map(function(a) { return (a-avg)*(a-avg); })
				.reduce(add, 0);
			variance /= y.length;

			if(variance > 70. && !self.shaked.value)
				self.shaked.value = true;
			else
				self.shaked.value = false;
		});

	accelerometer.on("update", function(time, x, y, z) {
		self.sampleData.onNext([time,x,y,z]);
	});
}

ShakeRecognizer.prototype.start = function() {
	++this.onCount;
	if(this.onCount > 1 || this.isStarted) return;

	accelerometer.start();
	this.isStarted = true;
}

ShakeRecognizer.prototype.stop = function() {
	--this.onCount;
	if(this.onCount > 0 || this.isStarted == false)
		return;

	accelerometer.stop();
	this.onCount = 0;
	this.isStarted = false;
}

var recognizer = new ShakeRecognizer();
module.exports = recognizer;