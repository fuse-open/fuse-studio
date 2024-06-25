var globalProxy = null;

module.exports = {
	getProxy: function() {
		return globalProxy;
	},
	setProxy: function(proxy) {
		globalProxy = proxy;
	}
};