using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Wholething.FallbackTextProperty.Services.Impl;
using Wholething.FallbackTextProperty.Services;

namespace Umbraco.Community.Fallback.Wholething
{
    public class WholethingComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IFallbackService, FallbackService>();
            builder.Services.AddSingleton<IFallbackTextReferenceParser, FallbackTextReferenceParser>();
            builder.Services.AddSingleton<IFallbackTextLoggerService, FallbackTextLoggerService>();

            builder.Services.AddSingleton<IFallbackTextResolver, ParentFallbackTextResolver>();
            builder.Services.AddSingleton<IFallbackTextResolver, RootFallbackTextResolver>();
            builder.Services.AddSingleton<IFallbackTextResolver, AncestorFallbackTextResolver>();
            builder.Services.AddSingleton<IFallbackTextResolver, UrlFallbackTextResolver>();
        }
    }
}
