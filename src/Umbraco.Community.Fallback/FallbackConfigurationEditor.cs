using Microsoft.AspNetCore.Http;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Community.Fallback;

public class FallbackConfigurationEditor : ConfigurationEditor
{
    private readonly FallbackEditor fallbackEditor;
    private readonly PropertyEditorCollection propertyEditors;
    private readonly IDataTypeService dataTypeService;
    private readonly ILocalizedTextService localizedTextService;
    private readonly IDataValueEditor dataValueEditor;
    private readonly IContentTypeService contentTypeService;

    public const string DataTypeKey = "dataType";
    public const string FallbackKey = "fallbackTemplate";

    private const string LocalizationAreaKey = "fallbackProperty";
    private const string InnerViewKey = "fallback-inner-view";
    public const string FallbackChainKey = "fallbackChain";

    public FallbackConfigurationEditor(
        FallbackEditor fallbackEditor,
        PropertyEditorCollection propertyEditors,
        IDataTypeService dataTypeService,
        ILocalizedTextService localizedTextService,
        IDataValueEditor dataValueEditor,
        IContentTypeService contentTypeService
    )
    {
        this.fallbackEditor = fallbackEditor;
        this.propertyEditors = propertyEditors;
        this.dataTypeService = dataTypeService;
        this.localizedTextService = localizedTextService;
        this.dataValueEditor = dataValueEditor;
        this.contentTypeService = contentTypeService;

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
                {"type", Constants.Applications.Settings},
                {"treeAlias", Constants.Trees.DataTypes},
                {"idType", "id"}
            }
        });

        var allProperties = new object[]
        {
            new
            {
                Name = "Content",
                Alias = "content",
                Properties = new[]
                {
                    new { Alias = "name", Name = "Name" },
                    new { Alias = "createdBy", Name = "Created By" },
                    new { Alias = "createdDate", Name = "Created Date" },
                    new { Alias = "publishDate", Name = "Publish Date" },
                }
            }
        }.Union(
            contentTypeService.GetAll()
            .Where(x => !x.IsElement)
            .Select(x => 
                new
                {
                    x.Alias,
                    x.Name,
                    Properties = x.PropertyTypes.Select(x => new { x.Alias, x.Name })
                }
            )
        );

        Fields.Add(new ConfigurationField
        {
            Key = FallbackChainKey,
            Name = localizedTextService.Localize(LocalizationAreaKey, "labelFallbackChain"),
            Description = localizedTextService.Localize(LocalizationAreaKey, "descriptionFallbackChain"),
            //View = "/umbraco/views/propertyeditors/blocklist/blocklist.html",
            View = "/App_Plugins/Umbraco.Community.Fallback/fallback-chain.html",
            Config = new Dictionary<string, object>
            {
                { "properties", allProperties }
            }
        });

        Fields.Add(new ConfigurationField
        {
            Key = FallbackKey,
            Name = localizedTextService.Localize(LocalizationAreaKey, "labelFallback"),
            Description = localizedTextService.Localize(LocalizationAreaKey, "descriptionFallback"),
            View = "/App_Plugins/Umbraco.Community.Fallback/fallback-config.html"
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

                if (config2 != null && config.ContainsKey(FallbackKey) && !config2.ContainsKey(FallbackKey))
                {
                    config2?.Add(FallbackKey, config[FallbackKey]);
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

                if (propertyEditors.TryGet(dataType.EditorAlias, out var dataEditor) == true)
                {
                    var shadowed = dataEditor.GetValueEditor(dataType.Configuration);
                    editorValues["fallback-inner-view"] = shadowed?.View ?? FallbackEditor.DefaultInnerViewPath;
                }
            }
        }

        return editorValues;
    }
}