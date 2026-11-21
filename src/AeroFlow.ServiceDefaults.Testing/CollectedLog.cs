using Microsoft.Extensions.Logging;

namespace AeroFlow.ServiceDefaults.Testing;

/// <summary>One captured log record, with state and scopes flattened into <see cref="Fields"/>.</summary>
public sealed record CollectedLog(
    string Category,
    LogLevel Level,
    EventId EventId,
    string Message,
    Exception? Exception,
    IReadOnlyDictionary<string, object?> State,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Scopes)
{
    /// <summary>Scope values (outer to inner) then state; later keys win.</summary>
    public IReadOnlyDictionary<string, object?> Fields
    {
        get
        {
            var fields = new Dictionary<string, object?>();
            foreach (var scope in Scopes)
            {
                foreach (var pair in scope)
                {
                    fields[pair.Key] = pair.Value;
                }
            }

            foreach (var pair in State)
            {
                fields[pair.Key] = pair.Value;
            }

            return fields;
        }
    }

    public bool TryGetField(string name, out object? value) => Fields.TryGetValue(name, out value);
}
