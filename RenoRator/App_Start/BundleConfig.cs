using System.Web;
using System.Web.Optimization;
using System;

namespace RenoRator
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Add CSS bundle
            bundles.Add(new StyleBundle("~/bundles/css").IncludeDirectory("~/public/css/image-uploader/", "*.css"));

            // Add JS bundle
            bundles.Add(new ScriptBundle("~/bundles/js").Include("~/Scripts/image-uploader/vendor/jquery.ui.widget.js"
                                                        , "~/Scripts/image-uploader/tmpl.js"
                                                        , "~/Scripts/image-uploader/load-image.js"
                                                        , "~/Scripts/image-uploader/canvas-to-blob.js"
                                                        , "~/Scripts/image-uploader/bootstrap.js"
                                                        , "~/Scripts/image-uploader/bootstrap-image-gallery.js"
                                                        , "~/Scripts/image-uploader/jquery.iframe-transport.js"
                                                        , "~/Scripts/image-uploader/jquery.fileupload.js"
                                                        , "~/Scripts/image-uploader/jquery.fileupload-fp.js"
                                                        , "~/Scripts/image-uploader/jquery.fileupload-ui.js"
                                                        , "~/Scripts/image-uploader/locale.js"));
            bundles.Add(new ScriptBundle("~/bundles/main").IncludeDirectory("~/Scripts/image-uploader/main", "*.js"));
        }
    }
}