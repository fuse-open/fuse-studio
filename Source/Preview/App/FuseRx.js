var Rx = require('rx.all');
var Observable = require('FuseJS/Observable');

var rxObservableProto = Rx.Observable.prototype;
rxObservableProto.toFuseObservable = function(errorObservable) {
	var obs = Observable();
	this.subscribe(function (a) {
		obs.value = a;
	}, function (e) {
		if(errorObservable != null)
			errorObservable.value = e;
	},
	function () {
		// TODO: OnCompleted?
	}); // What to do with the subscription (dispose)? 
	return obs;
};

var fuseObservableProto = Observable.prototype;
fuseObservableProto.toRxObservable = function() {
	var self = this;
	return Rx.Observable.create(function (observer) {
		var onNext = function (a) {
			observer.onNext(a.value);
		};

		self.addSubscriber(onNext);

		return function () {
			self.removeSubscriber(onNext);
		};
	});
};

module.exports = Rx;

