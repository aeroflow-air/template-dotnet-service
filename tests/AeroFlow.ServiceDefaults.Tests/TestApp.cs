using AeroFlow.ServiceDefaults.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroFlow.ServiceDefaults.Tests;

internal static class TestApp
{
    public static async Task<Hosted> StartAsync(
        string environment = "Development",
        IReadOnlyDictionary<string, string?>? configuration = null,
        Action<WebApplication>? configure = null,
        Action<WebApplication>? map = null)
    {
        var collector = new LogCollector();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = environment,
            ApplicationName = "test-service",
            Args = [],
        });

        var settings = new Dictionary<string, string?>
        {
            ["Observability:ServiceName"] = "test-service",
            ["Observability:ServiceVersion"] = "1.2.3",
            ["Observability:Environment"] = "local",
            ["Observability:Squad"] = "platform",
        };

        if (configuration is not null)
        {
            foreach (var pair in configuration)
            {
                settings[pair.Key] = pair.Value;
            }
        }

        builder.Configuration.AddInMemoryCollection(settings);
        builder.Logging.ClearProviders();
        collector.AddTo(builder.Logging);
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.AddServiceDefaults();

        var app = builder.Build();
        configure?.Invoke(app);
        app.MapDefaultEndpoints();
        map?.Invoke(app);

        await app.StartAsync();
        return new Hosted(app, collector, app.Services.GetRequiredService<ObservabilityResource>());
    }

    internal sealed class Hosted(WebApplication app, LogCollector logs, ObservabilityResource resource) : IAsyncDisposable
    {
        public WebApplication App { get; } = app;

        public LogCollector Logs { get; } = logs;

        public ObservabilityResource Resource { get; } = resource;

        private HttpClient? _client;

        public HttpClient Client => _client ??= new HttpClient { BaseAddress = new Uri(App.Urls.First()) };

        public IObservabilityLog ObservabilityLog => App.Services.GetRequiredService<IObservabilityLog>();

        public async ValueTask DisposeAsync()
        {
            _client?.Dispose();
            await App.StopAsync();
            await App.DisposeAsync();
        }
    }
}
