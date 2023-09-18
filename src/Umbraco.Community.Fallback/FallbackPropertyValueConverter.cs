using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Extensions;

namespace Umbraco.Community.Fallback
{
    public class FallbackPropertyValueConverter : PropertyValueConverterBase
    {
        private readonly IDataTypeService dataTypeService;
        private readonly IPublishedContentTypeFactory publishedContentTypeFactory;

        public FallbackPropertyValueConverter(
            IDataTypeService dataTypeService, 
            IPublishedContentTypeFactory publishedContentTypeFactory
            )
        {
            this.dataTypeService = dataTypeService;
            this.publishedContentTypeFactory = publishedContentTypeFactory;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.DataType.EditorAlias == FallbackEditor.DataEditorAlias;
        }

        public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        {
            var dataType = GetDataType(propertyType);
            var publishedPropertyType = GetInnerPropertyType(propertyType, dataType);
            var currentValue = publishedPropertyType.ConvertInterToObject(owner, referenceCacheLevel, inter, preview);

            // TODO: Remove cyclomatic complexity and move to intermediate
            if (IsBlank(currentValue) && propertyType.DataType.Configuration is Dictionary<string, object> config)
            {
                if (config?.TryGetValue(FallbackConfigurationEditor.FallbackChainKey, out var fallbackChainObj) == true && fallbackChainObj is JArray fallbackChain)
                {
                    foreach (var fallback in fallbackChain)
                    {
                        try
                        {
                            var target = fallback.Value<string>("value");
                            if (String.IsNullOrWhiteSpace(target)) continue;
                            var parts = target.Split('.');
                            if (parts.Length != 2) continue;
                            if (parts[0] == "content")
                            {
                                var ownerValue = GetOwnerValue(owner, parts);
                                if (IsBlank(ownerValue)) continue;
                                return ownerValue;
                            }
                            else
                            {
                                var prop = owner.GetProperty(parts[1]);
                                var value = prop?.GetValue();
                                if (IsBlank(value)) continue;
                                return value;
                            }
                        }
                        catch
                        {
                            // TODO: Log?
                        }
                    }
                }

                if (config?.TryGetValue(FallbackConfigurationEditor.FallbackKey, out var ultimateFallback) == true)
                {
                    return publishedPropertyType.ConvertInterToObject(owner, referenceCacheLevel, ultimateFallback, preview);
                }
            }

            return currentValue;
        }

        private object? GetOwnerValue(IPublishedElement owner, string[] parts)
        {
            return parts[1] switch
            {
                _ => owner.GetType().GetProperties()
                        .FirstOrDefault(x => x.Name.InvariantEquals(parts[1]))?
                        .GetValue(owner)
            };
        }

        private static bool IsBlank(object? value)
        {
            return value == null
                   || "".Equals(value)
                   || DateTime.MinValue.Equals(value)
                   || (value is IEnumerable enumerable && !enumerable.Cast<object>().Any());
        }

        public override object? ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
            => GetInnerPropertyType(propertyType, GetDataType(propertyType)).ConvertInterToXPath(owner, referenceCacheLevel, inter, preview);

        public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
            => GetInnerPropertyType(propertyType, GetDataType(propertyType)).ConvertSourceToInter(owner, source, preview);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => GetInnerPropertyType(propertyType, GetDataType(propertyType)).CacheLevel;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => GetInnerPropertyType(propertyType, GetDataType(propertyType)).ModelClrType;

        private IPublishedPropertyType GetInnerPropertyType(IPublishedPropertyType propertyType, IDataType? dataType)
        {
            if (dataType?.EditorAlias.InvariantEquals(FallbackEditor.DataEditorAlias) == false)
            {
                return publishedContentTypeFactory.CreatePropertyType(
                    propertyType.ContentType,
                    propertyType.Alias,
                    dataType.Id,
                    ContentVariation.Nothing);
            }

            throw new InvalidOperationException($"Data type not configured for the property: {propertyType.DataType.Id}");
        }

        private IDataType? GetDataType(IPublishedPropertyType propertyType)
        {
            var dataType = default(IDataType);
            if (propertyType.ContentType != null &&
                propertyType.DataType.Configuration is Dictionary<string, object> config &&
                config?.TryGetValue(FallbackConfigurationEditor.DataTypeKey, out var tmp1) == true &&
                tmp1 is string str1)
            {
                if (int.TryParse(str1, out var id) == true)
                {
                    dataType = dataTypeService.GetDataType(id);
                }
                else if (UdiParser.TryParse<GuidUdi>(str1, out var udi) == true)
                {
                    dataType = dataTypeService.GetDataType(udi.Guid);
                }
            }

            return dataType;
        }
    }
}
