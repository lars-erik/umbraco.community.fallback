using System;
using System.ComponentModel.DataAnnotations;
using Wholething.FallbackTextProperty.Services;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Wholething.FallbackTextProperty.Extensions;
using Umbraco.Cms.Core.Services;
#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
#else
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
#endif

namespace Umbraco.Community.Fallback.Wholething.FallbackTextProperty.Controllers
{
    [PluginController("Fallback")]
    public class TemplateDataController : UmbracoAuthorizedApiController
    {
        private readonly IFallbackService _fallbackTextService;
        private readonly IPublishedSnapshotAccessor publisheSnapshotAccessor;
        private readonly IDataTypeService dataTypeService;

        public TemplateDataController(IFallbackService fallbackTextService, IPublishedSnapshotAccessor publisheSnapshotAccessor, IDataTypeService dataTypeService)
        {
            _fallbackTextService = fallbackTextService;
            this.publisheSnapshotAccessor = publisheSnapshotAccessor;
            this.dataTypeService = dataTypeService;
        }

#if NET5_0_OR_GREATER
        [HttpGet]
        public IActionResult Get([Required] Guid nodeId, [Required] Guid dataTypeKey, string culture = null, Guid? blockId = null)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }


            var result = _fallbackTextService.BuildDictionary(nodeId, blockId, dataTypeKey, culture);
            return new OkObjectResult(result);
        }
#else
        [HttpGet]
        public IHttpActionResult Get([Required] Guid nodeId, [Required] Guid dataTypeKey, string culture = null, Guid? blockId = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(_fallbackTextService.BuildDictionary(nodeId, blockId, dataTypeKey, culture));
        }
#endif

    }
}