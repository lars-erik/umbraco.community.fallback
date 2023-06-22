/* Copyright © 2020 Lee Kelleher with small additions / changes by Lars-Erik Aabech.
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/. */

(function () {
    "use strict";

    function contentmentItemsEditorFactory(displayMode) {
        return {
            bindings: {
                addButtonLabel: "@?",
                addButtonLabelKey: "<?",
                allowAdd: "<?",
                allowEdit: "<?",
                allowRemove: "<?",
                allowSort: "<?",
                blockActions: "<?",
                defaultIcon: "<?",
                displayMode: "<?",
                getItem: "<?",
                getItemIcon: "<?",
                getItemName: "<?",
                getItemDescription: "<?",
                ngModel: "=",
                onAdd: "<?",
                onEdit: "<?",
                onRemove: "<?",
                onSort: "<?",
                propertyActions: "<?",
                previews: "<?",
            },
            controllerAs: "vm",
            controller: [
                "$scope",
                "localizationService",
                function ($scope, localizationService) {

                    var vm = this;

                    vm.$onInit = function () {

                        var _displayMode = displayMode || vm.displayMode || "list";

                        //console.log("contentmentItemsEditorFactory", _displayMode, vm);

                        vm.templateUrl = "/App_Plugins/Umbraco.Community.Fallback/" + _displayMode + "-editor.html";

                        vm.propertyAlias = vm.umbProperty.property.alias;

                        vm.sortableOptions = {
                            axis: "y",
                            containment: "parent",
                            cursor: "move",
                            disabled: vm.allowSort === false,
                            opacity: 0.7,
                            scroll: true,
                            tolerance: "pointer",
                            stop: function (e, ui) {

                                if (vm.onSort) {
                                    vm.onSort();
                                }

                                if (vm.propertyForm) {
                                    vm.propertyForm.$setDirty();
                                }
                            }
                        };

                        // NOTE: Sortable options for specific modes.
                        if (_displayMode === "blocks") {
                            Object.assign(vm.sortableOptions, {
                                cancel: "input,textarea,select,option",
                                classes: ".blockelement--dragging",
                                cursor: "grabbing",
                                distance: 5,
                                handle: ".blockelement__draggable-element"
                            });
                        } else if (_displayMode === "cards") {
                            Object.assign(vm.sortableOptions, {
                                axis: false,
                                "ui-floating": true,
                                items: ".umb-block-card",
                                cursor: "grabbing",
                                placeholder: "umb-block-card --sortable-placeholder",
                            });
                        }

                        vm.add = add;
                        vm.canEdit = canEdit;
                        vm.canRemove = canRemove;
                        vm.edit = edit;
                        vm.populate = populate;
                        vm.populateStyle = populateStyle;
                        vm.remove = remove;

                        vm.isSingle = vm.allowAdd === false && vm.ngModel.length === 1 ? "" : undefined;

                        if (vm.addButtonLabelKey) {
                            localizationService.localize(vm.addButtonLabelKey).then(function (label) {
                                vm.addButtonLabel = label;
                            });
                        }

                        if (vm.propertyActions && vm.propertyActions.length > 0) {
                            vm.umbProperty.setPropertyActions(vm.propertyActions);
                        }

                        if (vm.blockActions && vm.blockActions.length > 0) {
                            vm.blockActions.forEach(function (x) {
                                x.forEach(function (y) {
                                    localizationService.localize(y.labelKey).then(function (label) {
                                        y.label = label;
                                    });
                                });
                            });
                        }
                    };

                    function add() {
                        if (typeof vm.onAdd === "function") {
                            vm.onAdd();
                        }
                    };

                    function canEdit(item, $index) {
                        switch (typeof vm.allowEdit) {
                            case "boolean":
                                return vm.allowEdit;
                            case "function":
                                return vm.allowEdit(item, $index);
                            default:
                                return true;
                        }
                    };

                    function canRemove(item, $index) {
                        switch (typeof vm.allowRemove) {
                            case "boolean":
                                return vm.allowRemove;
                            case "function":
                                return vm.allowRemove(item, $index);
                            default:
                                return true;
                        }
                    };

                    function edit($index) {
                        if (typeof vm.onEdit === "function") {
                            vm.onEdit($index);
                        }
                    };

                    function populate(item, $index, propertyName) {
                        if (typeof vm.getItem === "function") {
                            return vm.getItem(item, $index, propertyName);
                        }

                        switch (propertyName) {
                            case "icon":
                                return typeof vm.getItemIcon === "function"
                                    ? vm.getItemIcon(item, $index)
                                    : item.icon || vm.defaultIcon;

                            case "name":
                                return typeof vm.getItemName === "function"
                                    ? vm.getItemName(item, $index)
                                    : item.name;

                            case "description":
                                return typeof vm.getItemDescription === "function"
                                    ? vm.getItemDescription(item, $index)
                                    : item.description;

                            default:
                                return item.hasOwnProperty(propertyName) === true
                                    ? item[propertyName]
                                    : undefined;
                        }
                    };

                    function populateStyle(item, $index, propertyName) {
                        var style = populate(item, $index, propertyName);
                        return style ? angular.fromJson(style) : {};
                    };

                    function remove($index) {
                        if (typeof vm.onRemove === "function") {
                            vm.onRemove($index);
                        }
                    };
                }],
            require: {
                propertyForm: "^form",
                umbProperty: "^"
            },
            template: "<ng-include src='vm.templateUrl'></ng-include>"
        };
    };

    angular.module("umbraco.directives").component("fallbackRulesEditor", contentmentItemsEditorFactory("list"));

})();

angular.module("umbraco").controller("Umbraco.Community.Fallback.ListEditorController", [
    "$scope",
    "editorService",
    "localizationService",
    "overlayService",
    function ($scope, editorService, localizationService, overlayService) {

        // console.log("data-list.editor.model", $scope.model);

        var defaultConfig = {
            confirmRemoval: 0,
            defaultIcon: "icon-stop",
            enableDevMode: 0,
            hideDescription: true,
            hideIcon: true,
            maxItems: 0,
            notes: null,
        };
        var config = Object.assign({}, defaultConfig, $scope.model.config);

        var vm = this;

        function init() {

            $scope.model.value = $scope.model.value || [];

            if (Number.isInteger(config.maxItems) === false) {
                config.maxItems = Number.parseInt(config.maxItems) || defaultConfig.maxItems;
            }

            config.confirmRemoval = Object.toBoolean(config.confirmRemoval);

            vm.allowAdd = config.maxItems === 0 || $scope.model.value.length < config.maxItems;
            vm.focusName = false;
            vm.hideDescription = Object.toBoolean(config.hideDescription);
            vm.hideIcon = Object.toBoolean(config.hideIcon);

            vm.sortableOptions = {
                axis: "y",
                containment: "parent",
                cursor: "move",
                opacity: 0.7,
                scroll: true,
                tolerance: "pointer",
                stop: (e, ui) => setDirty()
            };

            vm.notes = config.notes;

            vm.add = add;
            vm.blur = blur;
            vm.edit = edit;
            vm.open = open;
            vm.remove = remove;

            if (Object.toBoolean(config.enableDevMode) === true && $scope.umbProperty) {
                $scope.umbProperty.setPropertyActions([{
                    labelKey: "contentment_editRawValue",
                    icon: "brackets",
                    method: edit
                }, {
                    labelKey: "clipboard_labelForRemoveAllEntries",
                    icon: "trash",
                    method: () => {
                        $scope.model.value = [];
                    }
                }]);
            }

        };

        function add() {

            vm.focusName = vm.hideIcon === true;

            $scope.model.value.push({
                icon: config.defaultIcon,
                name: "",
                value: "",
                description: "",
            });

            if (config.maxItems !== 0 && $scope.model.value.length >= config.maxItems) {
                vm.allowAdd = false;
            }

            setDirty();

        };

        function blur(item) {
            if (item.name && item.value == null || item.value === "") {
                item.value = item.name.toCamelCase();
            }
        };

        function edit() {
        };

        function open(item) {

            var parts = item.icon.split(" ");

            editorService.iconPicker({
                icon: parts[0],
                color: parts[1],
                submit: function (model) {

                    item.icon = [model.icon, model.color].filter(s => s).join(" ");

                    vm.focusName = true;

                    setDirty();

                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            });
        };

        function remove($index) {
            if (config.confirmRemoval === true) {
                var keys = ["contentment_removeItemMessage", "general_remove", "general_cancel", "contentment_removeItemButton"];
                localizationService.localizeMany(keys).then(data => {
                    overlayService.open({
                        title: data[1],
                        content: data[0],
                        closeButtonLabel: data[2],
                        submitButtonLabel: data[3],
                        submitButtonStyle: "danger",
                        submit: function () {
                            removeItem($index);
                            overlayService.close();
                        },
                        close: function () {
                            overlayService.close();
                        }
                    });
                });
            } else {
                removeItem($index);
            }
        };

        function removeItem($index) {

            $scope.model.value.splice($index, 1);

            if (config.maxItems === 0 || $scope.model.value.length < config.maxItems) {
                vm.allowAdd = true;
            }

            setDirty();
        };

        function setDirty() {
            if ($scope.propertyForm) {
                $scope.propertyForm.$setDirty();
            }
        };

        init();
    }
]);