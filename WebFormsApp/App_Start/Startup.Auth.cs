using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;

using Owin;

using System.IO;

namespace WebFormsApp
{
    public partial class Startup
    {

        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301883
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/Account/Login"),
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = async (c) =>
                    {
                        // TODO: At this point we can validate identity received in the cookie 
                        var identity = c.Identity;
                    }
                },

                // Settings to configure shared cookie with ASP.NET Core app
                CookieName = ".AspNetCore.Identity.Application",
                AuthenticationType = "Identity.Application",
                TicketDataFormat = new AspNetTicketDataFormat(
                new DataProtectorShim(
                    DataProtectionProvider.Create(new DirectoryInfo(@"c:\blazor-dpk"),
                    builder => builder.SetApplicationName("blazor"))
                    .CreateProtector(
                        "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                        // Must match the Scheme name used in the ASP.NET Core app, i.e. IdentityConstants.ApplicationScheme
                        "Identity.Application",
                        "v2"))),
                CookieManager = new ChunkingCookieManager()
            });
        }
    }
}
