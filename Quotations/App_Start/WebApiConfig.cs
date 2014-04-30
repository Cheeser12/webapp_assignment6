using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Quotations
{
    class WebApiConfig
    {
        public static void Register(HttpConfiguration configuration)
        {           
            configuration.Routes.MapHttpRoute("CustomActions", "api/{controller}/{action}");
            configuration.Routes.MapHttpRoute("API Default", "api/{controller}/{action}/{id}",
                new { id = RouteParameter.Optional });
            configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}