using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Community.Fallback;

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