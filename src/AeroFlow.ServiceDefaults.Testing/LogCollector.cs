using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AeroFlow.ServiceDefaults.Testing;

/// <summary>
/// In-memory <see cref="ILoggerProvider"/> for contract tests. Register with
/// <see cref="AddTo(ILoggingBuilder)"/> and assert through <see cref="Entries"/>.
/// </summary>
public sealed class LogCollector : ILoggerProvider
{
    private readonly ConcurrentQueue<CollectedLog> _entries = new();
    private readonly LoggerExternalScopeProvider _scopes = new();

    public IReadOnlyList<CollectedLog> Entries => _entries.ToArray();

    public void Clear()
    {
        while (_entries.TryDequeue(out _))
        {
        }
    }

    public ILogger CreateLogger(string categoryName) => new CollectorLogger(this, categoryName);

    public void Dispose()
    {
    }

    public ILoggingBuilder AddTo(ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider>(this));
        return builder;
    }

    internal void Add(CollectedLog entry) => _entries.Enqueue(entry);

    internal void ForEachScope(Action<object?, string?> callback, string? state) =>
        _scopes.ForEachScope(callback, state!);

    internal IDisposable PushScope<T>(T state) => _scopes.Push(state);

    private sealed class CollectorLogger(LogCollector collector, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            collector.PushScope(state);

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var scopes = new List<IReadOnlyDictionary<string, object?>>();
            collector._scopes.ForEachScope(
                (scope, bag) =>
                {
                    var mapped = ScopeToDictionary(scope);
                    if (mapped.Count > 0)
                    {
                        bag.Add(mapped);
                    }
                },
                scopes);

            collector.Add(new CollectedLog(
                category,
                logLevel,
                eventId,
                formatter(state, exception),
                exception,
                ScopeToDictionary(state),
                scopes));
        }
    }

    internal static IReadOnlyDictionary<string, object?> ScopeToDictionary(object? scope)
    {
        if (scope is IEnumerable<KeyValuePair<string, object>> typed)
        {
            return typed.ToDictionary(pair => pair.Key, pair => (object?)pair.Value, StringComparer.Ordinal);
        }

        if (scope is IEnumerable<KeyValuePair<string, object?>> nullable)
        {
            return nullable.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }

        return new Dictionary<string, object?>(StringComparer.Ordinal);
    }
}
