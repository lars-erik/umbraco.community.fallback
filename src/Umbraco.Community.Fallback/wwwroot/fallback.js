(function() {
function isBlank(value) {
    return value === undefined ||
        value === null ||
        value === "" ||
        value === "1900-01-01"; // TODO: Figure mindate pattern
}

angular.module('umbraco').controller('umbraco.community.fallback.controller',
    [
        '$scope',
        'angularHelper',
        function (scope, angularHelper) {
            let innerView = scope.model.config['fallback-inner-view'];
            let modelJson = JSON.stringify(scope.model);
            let chain = scope.model?.config?.fallbackChain;

            let elementScope = angularHelper.traverseScopeChain(scope, s => s.hasOwnProperty('content'));
            let content = elementScope.content;

            let fallbackValue = null;
            let values = [];
            let props = content.tabs.reduce((c, n) => c.concat(n.properties), []);

            scope.edit = function(evt) {
                scope.fallback = false;
                let target = evt.target;
                setTimeout(() => {
                        let actual = target.closest('.fallback-container').querySelector('.actual');
                        let firstInput = actual.querySelector('input, button, select');
                        firstInput?.focus();
                    },
                    50);
            }

            scope.actualModel = JSON.parse(modelJson);
            scope.actualModel.view = innerView;

            scope.shadowModel = JSON.parse(modelJson);
            scope.shadowModel.view = innerView;

            scope.fallback = isBlank(scope.model.value);

            function evaluate() {
                fallbackValue = null;
                if (chain && chain.length) {
                    for (let i = 0; i < chain.length; i++) {
                        let rule = chain[i];
                        let parts = rule.value.split('.');
                        if (!parts || parts.length !== 2) return;
                        // TODO: Watch or just contiously digest?
                        if (parts[0] === 'content' && !isBlank(content[parts[1]])) {
                            fallbackValue = content[parts[1]];
                            break;
                        } else {
                            let prop = props.filter(x => x.alias === parts[1])[0];
                            if (isBlank(prop?.value)) continue;
                            fallbackValue = prop.value;
                            break;
                        }
                    };
                }

                if (isBlank(fallbackValue)) {
                    // Is really ultimateFallback
                    fallbackValue = scope.model.config.fallbackTemplate;
                }

                scope.shadowModel.value = fallbackValue;
            }

            evaluate();

            [
                'name',
                'createdBy',
                'createdDate',
                'publishedDate'
            ].forEach(x => scope.$watch(() => content[x], evaluate));

            props.forEach(x => scope.$watch(() => x.value, evaluate));

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

angular.module('umbraco').controller('umbraco.community.fallback.chain.controller',
    [
        '$scope',
        function(scope) {
        }
    ]);
})();