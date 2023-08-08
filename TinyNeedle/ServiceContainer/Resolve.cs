namespace TinyNeedle;

public partial class ServiceContainer
{
    /// <summary>
    /// Resolves a type T by searching all parents recursively for that type.
    /// T may be an interface or a concrete class type.
    /// </summary>
    /// <typeparam name="T">The type to resolve to an injected instance</typeparam>
    /// <returns>The created instance of that type</returns>
    public T? Resolve<T>(object[]? @params = null)
    {
        return (T?) Resolve(typeof(T), @params);
    }

    /// <summary>
    /// Resolves a type T by searching all parents recursively for that type.
    /// T may be an interface or a concrete class type.
    /// </summary>
    /// <typeparam name="T">The type to resolve to an injected instance</typeparam>
    /// <returns>The created instance of that type</returns>
    /// <exception cref="NullReferenceException">When the resolved instance could not be found or created</exception>
    public T ResolveRequired<T>(object[]? @params = null)
    {
        return Resolve<T>(@params) ?? throw new NullReferenceException();
    }
}