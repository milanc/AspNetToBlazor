using Microsoft.Owin;
using Owin;

using System.Web;

[assembly: OwinStartupAttribute(typeof(WebFormsApp.Startup))]
namespace WebFormsApp
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            app.Use(async (context, next) =>
            {
                var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                var forwardedHost = httpContext.Request.Headers["X-Forwarded-Host"];
                var serverVars = httpContext.Request.ServerVariables;
                serverVars["HTTP_HOST"] = forwardedHost;
                context.Request.Host = new HostString(forwardedHost);

                await next();
            });

            ConfigureAuth(app);
        }
    }
}
