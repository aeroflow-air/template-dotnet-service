using Microsoft.AspNetCore.Http;

namespace AeroFlow.ServiceDefaults;

internal static class HealthPaths
{
    public const string Health = "/health";
    public const string Alive = "/alive";

    public static bool IsHealth(PathString path) =>
        path.StartsWithSegments(Health) || path.StartsWithSegments(Alive);
}
