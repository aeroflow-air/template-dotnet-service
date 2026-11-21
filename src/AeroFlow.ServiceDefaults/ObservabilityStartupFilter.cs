using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace AeroFlow.ServiceDefaults;

internal sealed class ObservabilityStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            app.UseMiddleware<HttpRequestLoggingMiddleware>();
            next(app);
        };
}
