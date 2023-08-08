using System.Reflection;

namespace TinyNeedle;

public partial class ServiceContainer
{
    /// <summary>
    /// Automatically registers assembly-wide services.
    /// </summary>
    /// <returns>This service container instance for chaining</returns>
    public ServiceContainer AutoRegister()
    {
        var types = Assembly.GetCallingAssembly().GetTypes();
        foreach (var type in types)
        {
            if (type.GetCustomAttribute<DependencyAttribute>() is { } dependency)
            {
                Register(type, dependency.Lifetime);
            }
        }

        return this;
    }
}