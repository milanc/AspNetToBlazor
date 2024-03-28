## Adding support for aspnet core authentication to WebForms app
This approach is based on [cookie sharing section in the docs](https://learn.microsoft.com/en-us/aspnet/core/security/cookie-sharing?#share-authentication-cookies-between-aspnet-4x-and-aspnet-core-apps) but it differs in the fact that the AspNet core app (proxy) is used to authenticate and pass the authentication cookie to the legacy app.

### Legacy App
WebForms app without OWIN and took following steps to configure it:

1. Enable OWIN hosting inside IIS by installing `Microsoft.Owin.Host.SystemWeb` package
1. Configure app with to use ASP.NET Microsoft.Owin Cookie Authentication Middleware (following steps form docs above)  
   **Note that we need to match the cookie name from the aspnet core app instead of the one from the legacy app** - `.AspNetCore.Identity.Application`
1. Encryption needs the application name, e.g. `blazor` and keys location to configure data protection
   In *Startup.Auth.cs* when configuring `TicketDataFormat` use the desired app name when and keys location when invoking
   ```
   DataProtectionProvider.Create(new DirectoryInfo(@"c:\blazor-dpk"),
                    builder => builder.SetApplicationName("blazor"))
   ```
1. Update web.config to require authorization
   ```
    <system.web>
      ***
      <authorization>
        <deny users="?" />
      </authorization>
      <authentication mode="None" />
      ***
   ```
1. Update web.config to remove forms authentication module
   ```
   <system.webServer>
    ***
    <modules>
      <remove name="FormsAuthentication" />
    </modules>
    ***
   ```

**Required Packages**
Microsoft.Owin.Host.SystemWeb
Microsoft.Owin.Security.Cookies
Microsoft.Owin.Security.Interop 

### Blazor App
This app also has to be configured to use same protection key location and application name
Add these lines to the *program.cs* to match values from above
```
builder.Services.AddDataProtection(c =>
{
    c.ApplicationDiscriminator = "blazor";
}).PersistKeysToFileSystem(new DirectoryInfo("c:\\blazor-dpk"))
```

Added api controller to render navigation ui to serve as a placeholder while the custom element is rendered on the page.
NavMenu control can't be rendered without Blazor context so replaced those with `a` tags.
Other issues can be solved with `IsLegacyPrerender` parameter check and conditional rendering.
TODO: Consider having two sections for placeholder and regular navigation so we can use NavItem control.
      