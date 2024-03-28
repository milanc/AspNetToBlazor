using BlazorWebApp.Client;
using BlazorWebApp.Client.Components;
using BlazorWebApp.Client.Components.SharedComponents;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.RegisterCustomElement<NavMenu>("nav-menu");
builder.RootComponents.RegisterCustomElement<SharedComponentsContainer>("shared-comps");

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
builder.Services.AddRadzenComponents();
await builder.Build().RunAsync();
