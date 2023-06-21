angular.module('umbraco').controller('umbraco.community.fallback.controller', [
    '$scope',
    'assetsService',
    'contentResource',
    'editorState',
    'fallbackService',
    'notificationsService', 
    function (scope, assetsService, contentResource, editorState, fallbackTextService, notificationsService) {
        scope.shadowModel = JSON.parse(JSON.stringify(scope.model));
        scope.shadowModel.view = scope.shadowModel.config['fallback-inner-view'];

        assetsService
            .load([
                '~/App_Plugins/Umbraco.Community.Fallback/mustache.min.js'
            ])
            .then(function () {
                init();
            });

        let templateDictionary = {};
        let block = null;
        let template = "";

        function init() {
            template = scope.model.config.fallbackTemplate || '';

            getFallbackDictionary()
                .then(() => {
                    addToDictionary(getContent());
                    updateFallbackValue();
                });
        }
        function updateFallbackValue() {
            // We need to avoid HTML encoding of things like ampersand
            var config = {
                escape: function (text) { return text; }
            }

            scope.fallback = Mustache.render(template, templateDictionary, null, config);
        }

        function addToDictionary(node, addPrefix) {
            var prefix = addPrefix ? `${node.id}:` : '';
            var variant = getVariant(node);
            templateDictionary[buildKey('name', prefix)] = variant.name;
            for (var tab of variant.tabs) {
                for (var property of tab.properties) {
                    templateDictionary[buildKey(property.alias, prefix)] = property.value;
                }
            }
        }

        function getContent() {
            return editorState.getCurrent();
        }

        function getVariant(node) {
            // Blocks don't have language variants
            if (!scope.model.culture || block) {
                return node.variants[0];
            }
            return node.variants.find(v => v.language.culture === scope.model.culture);
        }

        function getBlockId() {
            return block ? block.key : null;
        }

        function getFallbackDictionary() {
            return fallbackTextService.getTemplateData(getContent().key, getBlockId(), scope.model.dataTypeKey, scope.model.culture).then(function (data) {
                templateDictionary = data;
            }, function (error) {
                notificationsService.error('Fallback error', `Couldn\'t load dictionary for property (alias: \"${scope.model.alias}\", node: ${getContent().key})`);
                templateDictionary = {};
            });
        }

        function buildKey(alias, prefix) {
            return `${prefix}${alias}`;
        }
    }
])


/*
 * Wholething stuff
 */
var umbraco = angular.module('umbraco');

umbraco.factory('fallbackService', ['$http', 'eventsService', 'notificationsService', function ($http, eventsService, notificationsService) {
    var baseUrl = '/Umbraco/Backoffice/Fallback';

    var block = null;

    var editorOpenUnsubscribe = eventsService.on(
        'appState.editors.open',
        function (event, args) {
            if (args.editor.view.includes('blockeditor')) {
                if (block) {
                    notificationsService.error(
                        'Fallback editor',
                        'Block editor opened from inside a block, fallback editor will not function correctly'
                    );
                }
                block = args.editor.content;
            }
        }
    );

    var editorCloseUnsubscribe = eventsService.on(
        'appState.editors.close',
        function (event, args) {
            if (args.editor.view.includes('blockeditor')) {
                block = null;
            }
        }
    );

    function getTemplateData(nodeId, blockId, dataTypeKey, culture) {
        var url = `${baseUrl}/TemplateData/Get?nodeId=${nodeId}&dataTypeKey=${dataTypeKey}&culture=${culture}`;
        if (blockId) {
            url += `&blockId=${blockId}`;
        }
        return $http.get(url).then(function (data) {
            return data.data;
        });
    };

    function getBlock() {
        return block;
    }

    var service = {
        getTemplateData: getTemplateData,
        getBlock: getBlock
    };
    return service;
}]);