using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SecurityTest.Security;

namespace SecurityTest
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //添加MessageHandler
            config.MessageHandlers.Add(new BasicAuthenticationMessageHandler());
            config.MessageHandlers.Add(new JwtAuthenticationMessageHandler());
            config.MessageHandlers.Add(new JwtAuthorizationMessageHandler());
            config.MessageHandlers.Add(new ChangeRoleMessageHandler());
        }
    }
}
