using BlazorWebApp.Components;
using BlazorWebApp.Components.Account;
using BlazorWebApp.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
// support yarp and api >>>>>>>>>
builder.Services.AddHttpForwarder();
builder.Services.AddControllers();
// <<<<<<<<< support yarp and api
// Add services to the container.

builder.Services.AddRadzenComponents();

// support OTEL >>>>>>>>>>>>>>>
var otel = builder.Services.AddOpenTelemetry();
var tracingOtlpEndpoint = "http://localhost:4317/";
//const string serviceName = "yarpProxy";
string serviceName = builder.Environment.ApplicationName;
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName)).AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                });
});

// Configure OpenTelemetry Resources with the application name
otel.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

otel.ConfigureResource(resource => resource
    .AddService(serviceName));
// Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus

otel.WithMetrics(metrics => metrics
    // Metrics provider from OpenTelemetry
    .AddAspNetCoreInstrumentation()
    //.AddMeter(greeterMeter.Name)
    // Metrics provides by ASP.NET Core in .NET 8
    .AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                       0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            })
    .AddPrometheusExporter(c =>
    {
        c.ScrapeEndpointPath = "/mymet";
    }));



otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddSource("Yarp.ReverseProxy");
    //tracing.AddSource(greeterActivitySource.Name);
    if (tracingOtlpEndpoint != null)
    {
        tracing.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
        });
    }
    else
    {
        tracing.AddConsoleExporter();
    }
});
// <<<<<<<<<< support OTEL


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


// support OTEL >>>>>>>>>>>>>>>
app.MapPrometheusScrapingEndpoint();
// <<<<<<<<<<<<< support OTEL


app.UseStaticFiles();
// support yarp and api >>>>>>>>>
// seems that routing breaks hot reload
app.UseRouting();
// <<<<<<<<< support yarp and api
app.UseAntiforgery();

// support yarp and api >>>>>>>>>
app.UseAuthorization();
app.MapDefaultControllerRoute();
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

// support yarp and api >>>>>>>>>
app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);
// <<<<<<<<< support yarp and api

app.Run();
