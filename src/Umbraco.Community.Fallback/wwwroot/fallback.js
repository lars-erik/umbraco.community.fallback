angular.module('umbraco').controller('umbraco.community.fallback.controller',
    [
        '$scope',
        function (scope) {
            let innerView = scope.model.config['fallback-inner-view'];
            let modelJson = JSON.stringify(scope.model);

            scope.actualModel = JSON.parse(modelJson);
            scope.actualModel.view = innerView;

            scope.shadowModel = JSON.parse(modelJson);
            scope.shadowModel.view = innerView;
            scope.shadowModel.value = scope.model.config.fallbackTemplate;

            scope.$watch("actualModel.value",
                function() {
                    scope.model.value = scope.actualModel.value;
                }, true);
        }
    ]);

angular.module('umbraco').controller('umbraco.community.fallback.configuration.controller',
    [
        '$scope',
        '$http',
        function (scope, http) {
            scope.dataTypeProperty = scope.preValues.filter(x => x.alias === 'dataType')[0];
            scope.ultimateFallback = {};
            let initialValue = scope.model.value;
            let initial = true;

            scope.$watch('dataTypeProperty.value',
                function (newValue) {
                    scope.value = newValue;
                    http.get('/umbraco/backoffice/api/fallback/editormodel?dataTypeId=' + newValue)
                        .then(response => {
                            scope.ultimateFallback = response.data;
                            if (initial) {
                                scope.ultimateFallback.value = initialValue;
                                initial = false;
                            }
                        });
                });

            scope.$watch('ultimateFallback.value', function(newValue) {
                scope.model.value = newValue;
            });
        }
    ]);

angular.module('umbraco').controller('umbraco.community.fallback.chain.controller', [
    '$scope',
    function (scope) {
    }
])
