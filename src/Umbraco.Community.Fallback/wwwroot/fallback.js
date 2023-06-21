angular.module('umbraco').controller('umbraco.community.fallback.controller',
    [
        '$scope',
        function(scope) {
            scope.shadowModel = JSON.parse(JSON.stringify(scope.model));
            scope.shadowModel.view = scope.shadowModel.config['fallback-inner-view'];
        }
    ]);

angular.module('umbraco').controller('umbraco.community.fallback.configuration.controller',
    [
        '$scope',
        '$http',
        function (scope, http) {
            scope.dataTypeProperty = scope.preValues.filter(x => x.alias === 'dataType')[0];
            scope.ultimateFallback = {};

            scope.$watch('dataTypeProperty.value',
                function (newValue) {
                    scope.value = newValue;
                    http.get('/umbraco/backoffice/api/fallback/editormodel?dataTypeId=' + newValue)
                        .then(response => {
                            console.log(response.data);
                            scope.ultimateFallback = response.data;
                        });
                });

            scope.$watch('ultimateFallback.value', function(newValue) {
                scope.model.value = newValue;
            });
        }
    ]);
