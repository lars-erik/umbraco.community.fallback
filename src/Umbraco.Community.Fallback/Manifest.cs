using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;
using DataType = Umbraco.Cms.Core.Models.DataType;

namespace Umbraco.Community.Fallback
{
    /*
     * Loads of this is totally stolen from Lotte and somehow it reeks of Lee. 👼
     * https://github.com/LottePitcher/umbraco-admin-only-property/blob/develop/src/AdminOnlyProperty/AdminOnlyPropertyConfigurationEditor.cs
     */
    public class Manifest : IManifestFilter
    {
        private readonly IDataValueEditorFactory editorFactory;

        public Manifest(IDataValueEditorFactory editorFactory)
        {
            this.editorFactory = editorFactory;
        }

        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "Umbraco Community Fallback",
                Scripts = new []
                {
                    "/App_Plugins/Umbraco.Community.Fallback/fallback.js"
                }
            });
        }
    }

    public class ManifestComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<Manifest>();
        }
    }

    //[DataEditor("Umbraco.Community.Fallback", EditorType.PropertyValue, "Fallback", "~/App_Plugins/Umbraco.Community.Fallback/fallback.html")]
    public class FallbackEditor : IDataEditor
    {
        const string DataEditorAlias = "Umbraco.Community.FallbackProperty";
        const string DataEditorName = "Fallback Property";
        public const string DefaultInnerViewPath = "readonlyvalue";
        const string DataEditorIcon = "icon-user";

        private readonly PropertyEditorCollection propertyEditors;
        private readonly ILocalizedTextService localizedTextService;
        private readonly IDataTypeService dataTypeService;
        private readonly IShortStringHelper shortStringHelper;
        private readonly IJsonSerializer jsonSerializer;
        private IDataValueEditor innerEditor;
        private ShadowEditor shadowedEditor;

        public FallbackEditor(
            PropertyEditorCollection propertyEditors,
            ILocalizedTextService localizedTextService,
            IDataTypeService dataTypeService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IDataValueEditorFactory dataValueEditorFactory, 
            EditorType type = EditorType.PropertyValue
        )
        {
            this.propertyEditors = propertyEditors;
            this.localizedTextService = localizedTextService;
            this.dataTypeService = dataTypeService;
            this.shortStringHelper = shortStringHelper;
            this.jsonSerializer = jsonSerializer;
        }

        public string Name => DataEditorName;

        public string Alias => DataEditorAlias;

        public EditorType Type => EditorType.PropertyValue;

        public string Icon => DataEditorIcon;

        public string Group => PropertyEditors.Groups.Common;

        public bool IsDeprecated => false;

        public IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>();

        public IPropertyIndexValueFactory PropertyIndexValueFactory => new DefaultPropertyIndexValueFactory();

        public IConfigurationEditor GetConfigurationEditor()
        {
            var editor = new FallbackConfigurationEditor(propertyEditors, dataTypeService, localizedTextService, innerEditor);
            return editor;
        }

        public IDataValueEditor GetValueEditor()
        {
            return new DataValueEditor(
                localizedTextService,
                shortStringHelper,
                jsonSerializer)
            {
                ValueType = ValueTypes.Json,
                View = DefaultInnerViewPath,
            };
        }

        public IDataValueEditor GetValueEditor(object? configuration)
        {
            if (configuration is Dictionary<string, object> config &&
                config.TryGetValue(FallbackConfigurationEditor.DataTypeKey, out var obj1) == true &&
                obj1 is string str1)
            {
                var dataType = default(IDataType);

                if (int.TryParse(str1, out var id) == true)
                {
                    dataType = dataTypeService.GetDataType(id);
                }
                else if (UdiParser.TryParse<GuidUdi>(str1, out var udi) == true)
                {
                    dataType = dataTypeService.GetDataType(udi.Guid);
                }

                if (dataType != null && propertyEditors.TryGet(dataType.EditorAlias, out var dataEditor) == true)
                {
                    innerEditor = dataEditor.GetValueEditor(dataType.Configuration);
                    shadowedEditor = new ShadowEditor(innerEditor, dataType.Configuration);
                    return shadowedEditor;
                }
            }

            return GetValueEditor();
        }
    }

    public class ShadowEditor : IDataValueEditor
    {
        private IDataValueEditor innerEditor;

        public ShadowEditor(IDataValueEditor innerEditor, object? dataTypeConfiguration)
        {
            this.innerEditor = innerEditor;
        }

        public IEnumerable<ValidationResult> Validate(object? value, bool required, string? format) => innerEditor.Validate(value, required, format);

        public object? FromEditor(ContentPropertyData editorValue, object? currentValue) => innerEditor.FromEditor(editorValue, currentValue);

        public object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var editor = innerEditor.ToEditor(property, culture, segment);
            return editor;
        }

        public IEnumerable<XElement> ConvertDbToXml(IProperty property, bool published) => innerEditor.ConvertDbToXml(property, published);

        public XNode ConvertDbToXml(IPropertyType propertyType, object value) => innerEditor.ConvertDbToXml(propertyType, value);

        public string ConvertDbToString(IPropertyType propertyType, object? value) => innerEditor.ConvertDbToString(propertyType, value);

        public string? View => "/App_Plugins/Umbraco.Community.Fallback/fallback.html";

        public string ValueType
        {
            get => innerEditor.ValueType;
            set => innerEditor.ValueType = value;
        }

        public bool IsReadOnly => innerEditor.IsReadOnly;

        public bool HideLabel => innerEditor.HideLabel;

        public List<IValueValidator> Validators => innerEditor.Validators;
    }

    public class FallbackConfigurationEditor : ConfigurationEditor
    {
        private readonly PropertyEditorCollection propertyEditors;
        private readonly IDataTypeService dataTypeService;
        private readonly ILocalizedTextService localizedTextService;
        private readonly IDataValueEditor dataValueEditor;

        public const string DataTypeKey = "dataType";

        private const string LocalizationAreaKey = "fallbackProperty";
        private const string InnerViewKey = "fallback-inner-view";

        public FallbackConfigurationEditor(
            PropertyEditorCollection propertyEditors,
            IDataTypeService dataTypeService,
            ILocalizedTextService localizedTextService, 
            IDataValueEditor dataValueEditor
            )
        {
            this.propertyEditors = propertyEditors;
            this.dataTypeService = dataTypeService;
            this.localizedTextService = localizedTextService;
            this.dataValueEditor = dataValueEditor;

            Fields.Add(new ConfigurationField
            {
                Key = DataTypeKey,
                Name = localizedTextService.Localize(LocalizationAreaKey, "labelDataType"),
                Description = localizedTextService.Localize(LocalizationAreaKey, "descriptionDataType"),
                View = "treepicker",
                Config = new Dictionary<string, object>
                {
                    {"multiPicker", false},
                    {"entityType", nameof(DataType)},
                    {"type", Applications.Settings},
                    {"treeAlias", Trees.DataTypes},
                    {"idType", "id"}
                }
            });
        }

        public override IDictionary<string, object> ToValueEditor(object? configuration)
        {
            if (configuration is Dictionary<string, object> config &&
                config.TryGetValue(DataTypeKey, out var obj1) == true &&
                obj1 is string str1)
            {
                var dataType = default(IDataType);

                // NOTE: For backwards-compatibility, the value could either be an `int` or `Udi`.
                // However the `_dataTypeService.GetDataType` doesn't accept a `Udi`, so we'll use the `Guid`.
                if (int.TryParse(str1, out var id) == true)
                {
                    dataType = dataTypeService.GetDataType(id);
                }
                else if (UdiParser.TryParse<GuidUdi>(str1, out var udi) == true)
                {
                    dataType = dataTypeService.GetDataType(udi.Guid);
                }

                if (dataType != null && propertyEditors.TryGet(dataType.EditorAlias, out var dataEditor) == true)
                {
                    var cacheKey = $"__aopConfig";
                    var config2 = dataEditor.GetConfigurationEditor().ToValueEditor(dataType.Configuration);

                    if (config2?.ContainsKey(cacheKey) == false)
                    {
                        config2.Add(cacheKey, config);
                    }

                    if (config2 != null && !config2.ContainsKey(InnerViewKey))
                    {
                        config2.Add(InnerViewKey, dataValueEditor.View ?? FallbackEditor.DefaultInnerViewPath);
                    }

                    return config2!;
                }
            }

            return base.ToValueEditor(configuration);
        }

        public override object? FromConfigurationEditor(IDictionary<string, object?>? editorValues, object? configuration)
        {
            if (editorValues?.TryGetValue(DataTypeKey, out var value) == true && int.TryParse(value?.ToString(), out var id))
            {
                var dataType = dataTypeService.GetDataType(id);
                if (dataType != null)
                {
                    editorValues[DataTypeKey] = dataType.GetUdi().ToString();
                }
            }

            return base.FromConfigurationEditor(editorValues, configuration);
        }

        public override IDictionary<string, object> ToConfigurationEditor(object? configuration)
        {
            var editorValues = base.ToConfigurationEditor(configuration);

            if (editorValues.TryGetValue(DataTypeKey, out var value) == true && UdiParser.TryParse<GuidUdi>(value.ToString(), out var udi))
            {
                var dataType = dataTypeService.GetDataType(udi.Guid);
                if (dataType != null)
                {
                    editorValues[DataTypeKey] = dataType.Id.ToString();
                }
            }

            return editorValues;
        }
    }
}
