namespace Web.Infrastructure;

public static class ModuleRegistry
{
    private static readonly List<ModuleDefinition> _modules = new();

    public static IReadOnlyList<ModuleDefinition> Modules => _modules.AsReadOnly();

    public static void Register<TService>(string name, string routePrefix, bool hasComplexFeatures)
        where TService : class
    {
        _modules.Add(new ModuleDefinition(
            Name: name,
            RoutePrefix: routePrefix,
            HasComplexFeatures: hasComplexFeatures,
            ServiceType: typeof(TService)));
    }

    public static void RegisterServices(IServiceCollection services)
    {
        foreach (var module in _modules)
        {
            if (module.ServiceType is not null)
            {
                services.AddScoped(module.ServiceType);
            }
        }
    }
}

public record ModuleDefinition(
    string Name,
    string RoutePrefix,
    bool HasComplexFeatures,
    Type? ServiceType);
