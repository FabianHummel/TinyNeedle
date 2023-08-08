namespace TinyNeedle;

public partial class ServiceContainer
{
    /// <summary>
    /// Registers a new type for a given interface.
    /// </summary>
    /// <param name="lifetime">The lifetime in which the type instance will live</param>
    /// <typeparam name="TInterface">The interface that is implemented by type T</typeparam>
    /// <typeparam name="T">The actual implementation class for an interface TInterface</typeparam>
    /// <param name="params">Object parameters that are passed to the constructor</param>
    /// <returns>This service container instance for chaining</returns>
    public ServiceContainer Register<TInterface, T>(Lifetime lifetime = Lifetime.Transient, object[]? @params = null) where T : TInterface
    {
        return Register(typeof(TInterface), typeof(T), lifetime, @params);
    }
    
    /// <summary>
    /// Registers a new type for a given interface.
    /// </summary>
    /// <param name="lifetime">The lifetime in which the type instance will live</param>
    /// <param name="interface">The interface that is implemented by the given concrete class</param>
    /// <param name="type">The actual implementation class for the given interface</param>
    /// <param name="params">Object parameters that are passed to the constructor</param>
    /// <returns>This service container instance for chaining</returns>
    public ServiceContainer Register(Type @interface, Type type, Lifetime lifetime = Lifetime.Transient, object[]? @params = null)
    {
        _ = _registeredInterfaces.TryAdd(@interface, type) && 
            _registeredTypes.TryAdd(type, new TypeDefinition(lifetime, @params));
        return this;
    }

    /// <summary>
    /// Registers a new type without an interface.
    /// </summary>
    /// <typeparam name="T">The class type to register</typeparam>
    /// <param name="lifetime">The lifetime in which the type instance will live</param>
    /// <param name="params">Object parameters that are passed to the constructor</param>
    /// <returns>This service container instance for chaining</returns>
    public ServiceContainer Register<T>(Lifetime lifetime = Lifetime.Transient, object[]? @params = null)
    {
        return Register(typeof(T), lifetime, @params);
    }
    
    /// <summary>
    /// Registers a new type without an interface.
    /// </summary>
    /// <param name="type">The class type to register</param>
    /// <param name="lifetime">The lifetime in which the type instance will live</param>
    /// <param name="params">Object parameters that are passed to the constructor</param>
    /// <returns>This service container instance for chaining</returns>
    public ServiceContainer Register(Type type, Lifetime lifetime = Lifetime.Transient, object[]? @params = null)
    {
        _registeredTypes.TryAdd(type, new TypeDefinition(lifetime, @params));
        return this;
    }
}