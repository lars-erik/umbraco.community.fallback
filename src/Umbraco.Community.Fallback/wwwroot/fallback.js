(function () {
    function isBlank(value) {
        // TODO: Need blank strategies
        return value === undefined ||
            value === null ||       // really null
            value === "" ||         // empty string
            value?.length === 0 ||  // empty array
            value === "1900-01-01"; // TODO: Figure mindate pattern
    }

    angular.module('umbraco').controller('umbraco.community.fallback.controller',
        [
            '$scope',
            'angularHelper',
            function (scope, angularHelper) {
                const innerView = scope.model.config['fallback-inner-view'];
                const modelJson = JSON.stringify(scope.model);
                const chain = scope.model?.config?.fallbackChain;

                const elementScope = angularHelper.traverseScopeChain(scope, s => s.hasOwnProperty('content'));
                const content = elementScope.content;
                const props = content.tabs.reduce((c, n) => c.concat(n.properties), []);

                const actions = [
                    {
                        labelKey: 'fallback_resetToFallback',
                        labelTokens: [],
                        icon: 'arrow-left',
                        method: reset,
                        isDisabled: false
                    },
                    {
                        labelKey: 'fallback_copyFallback',
                        labelTokens: [],
                        icon: 'documents',
                        method: copy,
                        isDisabled: false
                    },
                ];

                function reset() {
                    // TODO: Need reset strategies
                    if (scope.actualModel.value instanceof Array) {
                        scope.actualModel.value = [];
                    } else {
                        scope.actualModel.value = null;
                    }
                }

                function copy() {
                    scope.actualModel.value = JSON.parse(JSON.stringify(scope.shadowModel.value));
                }

                function evaluate() {
                    let fallbackValue = null;

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

                this.$onInit = function () {
                    if (scope.umbProperty) {
                        scope.umbProperty.setPropertyActions(actions);
                    }
                }

                scope.edit = function (evt) {
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
                scope.shadowModel.value = null;
                scope.shadowModel.view = innerView;

                scope.fallback = isBlank(scope.model.value);

                [
                    'name',
                    'createdBy',
                    'createdDate',
                    'publishedDate'
                ].forEach(x => scope.$watch(() => content[x], evaluate));

                props.forEach(x => scope.$watch(() => x.value, evaluate));

                scope.$watch("actualModel.value",
                    function () {
                        let isFallback = isBlank(scope.actualModel.value);
                        scope.model.value = scope.actualModel.value;
                        scope.fallback = isFallback;
                        actions[0].isDisabled = isFallback;

                        if (scope.umbProperty && scope.umbProperty.propertyActions.filter(x => x.labelKey.indexOf('fallback_') === 0).length === 0) {
                            scope.umbProperty.setPropertyActions(
                                scope.umbProperty.propertyActions.concat(actions)
                            );
                        }
                    }, true);

                evaluate();
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

                scope.$watch('ultimateFallback.value', function (newValue) {
                    scope.model.value = newValue;
                });
            }
        ]);

    angular.module('umbraco').controller('umbraco.community.fallback.chain.controller',
        [
            '$scope',
            function (scope) {
            }
        ]);
})();