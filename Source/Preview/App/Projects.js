var Observable = require('FuseJS/Observable');
var simulatorAPI = require('SimulatorAPI');

var projects = new Observable();
simulatorAPI.listenForRecentProjects(function(projs) {
	projects.replaceAll(projs);
});

module.exports = {
	getProjects: function() {
		return projects;
	}
};