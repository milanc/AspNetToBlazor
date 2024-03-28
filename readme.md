# Migrating AspNet WebForms application to Blazor WebApp
This is a yet another migration project based on incremental migration guide using YARP.
Main goal was to modernize the authentication process and replace/share parts of the UI.

## Adding support for aspnet core authentication to WebForms app
This implementation is based on [cookie sharing section in the docs](https://learn.microsoft.com/en-us/aspnet/core/security/cookie-sharing?#share-authentication-cookies-between-aspnet-4x-and-aspnet-core-apps) but it differs in the fact that the AspNet core app (proxy) is used to authenticate and pass the authentication cookie to the legacy app.

### Legacy App
WebForms app from the default VS template with following tweaks:

1. Enable OWIN hosting inside IIS by installing `Microsoft.Owin.Host.SystemWeb` package
1. Configure app to use ASP.NET Microsoft.Owin Cookie Authentication Middleware (following steps form docs above)  
   **Note that we need to match the cookie name from the aspnet core app instead of the one from the legacy app** - `.AspNetCore.Identity.Application`
1. Encryption needs the application name, e.g. `blazor` and keys location to configure data protection
   In *Startup.Auth.cs* when configuring `TicketDataFormat` use the desired app name and keys location when invoking
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
   This could be tweaked to allow anonymous access to certain areas of the app. 
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

## Sharing Navigation Component
Navigation component is shared with WebForms by exposing it as custom element.  
Similar approach can be used for any other component so that web form pages can be partially upgraded and use modern ui components.
Following customizations are done to support sharing of the NavMenu component

### Blazor App

1. NavMenu component is moved to the client project
   1. Added `IsLegacy` and `IsLegacyPrerender` properties to manage the rendering and functionality outside of the Blazor context.
1. Relative links in the NavMenu replaced with the absolute to fix the navigation from the legacy app pages nested under sub paths
1. Tweaked the `blazor.webassembly.js` script to use global `blazorBase` variable instead of `document.baseURI`
   1. This was done to mitigate the issue with the fact that Blazor loads framework files relative to the `base` tag,  
      and adding the `base` tag to the legacy application, so that Blazor can load framework files for custom elements, messes the form posts and relative links in pages nested under sub paths
1. NavLink component can't be rendered without Blazor context so it is replaced with `a` tags to allow "prerendering"
   1. We also could have entirely new section used just for "prerendering", so we keep NavLink, but for larger menu it might be too much of markup duplication.  
1. Added legacy specific Logout link that redirects to Login page
   1. This was done to avoid complexity of supporting antiforgery tokens from the legacy app
   1. Login page on load executes script that posts to Logout page.  
1. In the server side project created `BlazorPrerender` controller with `menu` endpoint.
   1. This endpoint uses the .net8+ feature to [render blazor outside of asp.net core](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-components-outside-of-aspnetcore)  
   1. If menu can be collapsed consider exposing the another endpoint e.g `menu-collapsed` to prerender collapsed menu.
   1. These endpoints have cache attribute making them cacheable on the client to allow faster loading and reduce flickering when switching between wf and Blazor apps

### Legacy App
1. Added reference to blazor css and updated `blazor.webassembly-asp.js` file
   1. Set document.blazorBase in javascript that will be consumed by this js
1. Added NavMenu component.
   1. Set `username` attribute so the component can show it. 
   1. If IsLegacy property is used in the component set the `is-legacy` attribute
   1. Added placeholder div that will be replaced by statically rendered NavMenu component from the `menu` endpoint 
   1. This request will be cached client side to reduce menu flickering. 
   1. The url for `menu` endpoint can be extended with query string parameter for cache busting
      1. Reason for this would be if menu items depend on user profile - we could add a time stamp of the last modification
      1. `menu` endpoint does not need to process this parameter
      1. Once Blazor is initialized the actual component will replace this placeholder.


## More Shared UI
Using the same approach we did for navigation component, by exposing it as a custom element, we can expose any other component of interest.
This could be a any input element (dropdown, rich text box ... ) but intention for this feature is to expose more complex ui elements that could be used on both blazor and web forms pages.
Some examples are dialogs (alerts, confirmations...), notification messages, data entry windows ...

The Components folder in the client project contains subfolder named SharedComponents.
In this folder there is a `SharedComponentsContainer` which is exposed as a custom element and it is used to wrap all other shared components

### Share Forms With Popup
We can share entire form/page through the dialog interface. (this also covers other dialogs like confirmations and alerts)
For this implementation I've used [Radzen](https://blazor.radzen.com/get-started) library but it could be done with any other, with more or less similar functionality.
Good thing with Radzen is that, in their implementation, Dialog component is opened through a service call instead of having a component that depends on some sort of visibility property. 
The other important part for this feature is the [DynamicComponent](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/dynamiccomponent?view=aspnetcore-8.0) which allows invoking dialogs based on component types.
`DialogContent` component is a wrapper around this dynamic component and it is a target type for the `Dialog.OpenAsync` method.
In this folder we also have `DataEntry` component that is displayed inside the DialogContent component.

Using JavaScript Interop feature we can call show dialog by passing the component full type name (including namespace).
If we decide to put all components in the same folder then we could simplify calls so that we need only the component name.
```
async function showDialog() {
    var result = await BlazorShared.showDialog("BlazorWebApp.Client.Components.SharedComponents.DataEntry", "From About");
    alert("Result from radzen dialog - " + result);
}
```
