using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.Implement;
using static Umbraco.Cms.Core.Constants.Conventions;

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
                Stylesheets = new []
                {
                    "/App_Plugins/Umbraco.Community.Fallback/data-list.editor.css",
                },
                Scripts = new []
                {
                    "/App_Plugins/Umbraco.Community.Fallback/fallback.js",
                    "/App_Plugins/Umbraco.Community.Fallback/contentment.js",
                }
            });
        }
    }

    //[DataEditor("Umbraco.Community.Fallback", EditorType.PropertyValue, "Fallback", "~/App_Plugins/Umbraco.Community.Fallback/fallback.html")]
}
