using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Community.Fallback
{
    public class FallbackController : UmbracoAuthorizedApiController
    {
        private readonly IDataTypeService dataTypeService;
        private readonly IShortStringHelper stringHelper;
        private readonly IUmbracoMapper mapper;

        public FallbackController(IDataTypeService dataTypeService, IShortStringHelper stringHelper, IUmbracoMapper mapper)
        {
            this.dataTypeService = dataTypeService;
            this.stringHelper = stringHelper;
            this.mapper = mapper;
        }

        public async Task<ActionResult> EditorModel(int dataTypeId)
        {
            await Task.CompletedTask;
            var dataType = dataTypeService.GetDataType(dataTypeId);
            if (dataType == null)
            {
                return NotFound();
            }
            var propType = new PropertyType(stringHelper, dataType);
            var property = new Property(propType);
            var model = mapper.Map<ContentPropertyDisplay>(property);
            // ContentPropertyDisplay
            return Ok(model);

        }

        public Task<ActionResult> Echo(string stuff)
        {
            return Task.FromResult((ActionResult)Ok("Stuff is " + stuff));
        }
    }
}
