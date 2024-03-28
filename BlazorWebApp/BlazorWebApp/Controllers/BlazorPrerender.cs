using BlazorWebApp.Client.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWebApp.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class BlazorPrerenderController : ControllerBase
    {
        private readonly HtmlRenderer renderer;

        public BlazorPrerenderController(IServiceProvider sp, ILoggerFactory lf)
        {
            this.renderer = new HtmlRenderer(sp, lf);
        }

        // enable caching in browser to reduce flicker
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 3600)]
        [HttpGet("menu")]
        public async Task<string> Menu()
        {
            // if needed accept action parameters and pass them to component
            var html = await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
                {
                    { "IsLegacyPrerender", true },
                    { "Username", User.Identity.Name }
                });
                var output = await renderer.RenderComponentAsync<NavMenu>(parameters);

                return output.ToHtmlString();
            });

            return html;
        }
    }
}
