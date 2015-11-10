using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RenoRator
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "User", action = "Home", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ViewTaggedAds",
                url: "{controller}/{action}/{page}/{tag}",
                defaults: new { controller = "JobAds", action = "Ads", page = 0, tag = "" }
            );

            routes.MapRoute(
                name: "Contractors",
                url: "{controller}/{action}/{page}/{filters}",
                defaults: new { controller = "User", action = "Contractors", page = 0, filters = "" }
            );

            routes.MapRoute(
                name: "Profile",
                url: "{controller}/{action}",
                defaults: new { controller = "User", action = "Profile" }
            );

            routes.MapRoute(
                name: "ViewReceivedMessages",
                url: "{controller}/{action}/{page}",
                defaults: new { controller = "Message", action = "Received", page = 0 }
            );

            routes.MapRoute(
                name: "ViewSentMessages",
                url: "{controller}/{action}/{page}",
                defaults: new { controller = "Message", action = "Sent", page = 0 }
            );
        }
    }
}