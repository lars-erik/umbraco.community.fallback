using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Community.Fallback;

public class FallbackEditor : IDataEditor
{
    public const string DataEditorAlias = "Umbraco.Community.FallbackProperty";
    public const string DataEditorName = "Fallback Property";
    public const string DefaultInnerViewPath = "readonlyvalue";
    public const string PreviewPath = "/App_Plugins/Umbraco.Community.Fallback/fallback-preview.html";
    const string DataEditorIcon = "icon-user";

    private readonly PropertyEditorCollection propertyEditors;
    private readonly ILocalizedTextService localizedTextService;
    private readonly IDataTypeService dataTypeService;
    private readonly IShortStringHelper shortStringHelper;
    private readonly IJsonSerializer jsonSerializer;
    private readonly IContentTypeService contentTypeService;
    private IDataValueEditor innerEditor;
    private ShadowEditor shadowedEditor;

    public FallbackEditor(
        PropertyEditorCollection propertyEditors,
        ILocalizedTextService localizedTextService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IDataValueEditorFactory dataValueEditorFactory, 
        IContentTypeService contentTypeService,
        EditorType type = EditorType.PropertyValue
    )
    {
        this.propertyEditors = propertyEditors;
        this.localizedTextService = localizedTextService;
        this.dataTypeService = dataTypeService;
        this.shortStringHelper = shortStringHelper;
        this.jsonSerializer = jsonSerializer;
        this.contentTypeService = contentTypeService;
    }

    public string Name => DataEditorName;

    public string Alias => DataEditorAlias;

    public EditorType Type => EditorType.PropertyValue;

    public string Icon => DataEditorIcon;

    public string Group => Constants.PropertyEditors.Groups.Common;

    public bool IsDeprecated => false;

    public IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>();

    public IPropertyIndexValueFactory PropertyIndexValueFactory => new DefaultPropertyIndexValueFactory();

    public IConfigurationEditor GetConfigurationEditor()
    {
        var editor = new FallbackConfigurationEditor(this, propertyEditors, dataTypeService, localizedTextService, innerEditor, contentTypeService);
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
            View = PreviewPath,
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