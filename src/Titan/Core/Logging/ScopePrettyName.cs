namespace Titan.Core.Logging;

internal static class ScopePrettyName
{
    private static readonly Dictionary<Type, string> _scopeCache = new();
    public static string PrettyName(Type? scope)
    {
        if (scope == null)
        {
            return string.Empty;
        }

        if (!scope.IsGenericType)
        {
            return scope.Name;
        }

        if (_scopeCache.TryGetValue(scope, out var value))
        {
            return value;

        }
        var genericArguments = string.Join(", ", scope.GenericTypeArguments.Select(static t => PrettyName(t)));
        var name = scope.Name;

        return _scopeCache[scope] = $"{name[..name.IndexOf('`')]}<{genericArguments}>";
    }
}
