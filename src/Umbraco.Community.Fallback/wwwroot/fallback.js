angular.module('umbraco').controller('umbraco.community.fallback.controller', [
    '$scope',
    function (scope) {
        scope.shadowModel = JSON.parse(JSON.stringify(scope.model));
        scope.shadowModel.view = scope.shadowModel.config['fallback-inner-view'];
    }
])