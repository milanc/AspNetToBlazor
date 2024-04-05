#define NEED_HOT_RELOAD
using BlazorWebApp.Components;
using BlazorWebApp.Components.Account;
using BlazorWebApp.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

#if !NEED_HOT_RELOAD
// support yarp and api >>>>>>>>>
builder.Services.AddHttpForwarder();
builder.Services.AddControllers();
// <<<<<<<<< support yarp and api
#endif

builder.Services.AddRadzenComponents();

// support sharing authentication cookie (and any other encrypted data) >>>>>>>>>>>>>>>
builder.Services.AddDataProtection(c =>
{
    c.ApplicationDiscriminator = "blazor";

}).PersistKeysToFileSystem(new DirectoryInfo("c:\\blazor-dpk"));
// <<<<<<<<<<<<<<< support sharing authentication cookie (and any other encrypted data)

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
#if !NEED_HOT_RELOAD
// support yarp and api >>>>>>>>>
// seems that routing breaks hot reload
app.UseRouting();
// <<<<<<<<< support yarp and api
#endif
app.UseAntiforgery();

// support yarp and api >>>>>>>>>
app.UseAuthorization();
#if !NEED_HOT_RELOAD
app.MapDefaultControllerRoute();
#endif
// <<<<<<<<< support yarp and api

app.MapGet("/api/min-values", [Authorize] () =>
{
    return new List<string>() { "mv1", "mv2" };
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebApp.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
#if !NEED_HOT_RELOAD
// support yarp and api >>>>>>>>>
app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);
// <<<<<<<<< support yarp and api
#endif
app.Run();
