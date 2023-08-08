using System.Collections;
using System.ComponentModel;
using BindingFlags = System.Reflection.BindingFlags;

namespace TinyNeedle;

public class ServiceContainer : IEnumerable
{
    private readonly ServiceContainer? _parent;
    private readonly Dictionary<Type, Lifetime> _registeredTypes = new();
    private readonly Dictionary<Type, Type> _registeredInterfaces = new();
    private readonly Dictionary<Type, object> _instances = new();
    
    /// <summary>
    /// Creates a new ServiceContainer with another container as its parent.
    /// The newly created service container is considered scoped from the parent.
    /// </summary>
    /// <param name="serviceContainer">The parent service container.</param>
    public ServiceContainer(ServiceContainer serviceContainer)
    {
        _parent = serviceContainer;
    }

    /// <summary>
    /// Creates a new root ServiceContainer.
    /// </summary>
    public ServiceContainer()
    {
        _parent = null;
    }
    
    public IEnumerator GetEnumerator()
    {
        return _registeredTypes.GetEnumerator();
    }

    /// <summary>
    /// Automatically registers assembly-wide services.
    /// </summary>
    public ServiceContainer AutoRegister()
    {
        return this;
    }

    /// <summary>
    /// Registers a new type for a given interface.
    /// </summary>
    /// <param name="lifetime">The lifetime in which the type instance will live</param>
    /// <typeparam name="TInterface">The interface that is implemented by type T</typeparam>
    /// <typeparam name="T">The actual implementation class for an interface TInterface</typeparam>
    /// <returns></returns>
    public ServiceContainer Register<TInterface, T>(Lifetime lifetime) where T : TInterface
    {
        _ = _registeredInterfaces.TryAdd(typeof(TInterface), typeof(T)) && 
            _registeredTypes.TryAdd(typeof(T), lifetime);
        return this;
    }
    
    /// <summary>
    /// Registers a new type without an interface.
    /// </summary>
    /// <param name="lifetime">The lifetime in which the type instance will live</param>
    /// <typeparam name="T">The class type to register</typeparam>
    /// <returns></returns>
    public ServiceContainer Register<T>(Lifetime lifetime)
    {
        _registeredTypes.TryAdd(typeof(T), lifetime);
        return this;
    }

    /// <summary>
    /// Resolves a type T by searching all parents recursively for that type.
    /// T may be an interface or a concrete class type.
    /// </summary>
    /// <typeparam name="T">The type to resolve to an injected instance</typeparam>
    /// <returns>The created instance of that type</returns>
    public T? Resolve<T>()
    {
        return (T?) Resolve(typeof(T));
    }

    /// <summary>
    /// Resolves a type T by searching all parents recursively for that type.
    /// T may be an interface or a concrete class type.
    /// </summary>
    /// <typeparam name="T">The type to resolve to an injected instance</typeparam>
    /// <returns>The created instance of that type</returns>
    /// <exception cref="NullReferenceException">When the resolved instance could not be found or created</exception>
    public T ResolveRequired<T>()
    {
        return Resolve<T>() ?? throw new NullReferenceException();
    }

    /// <summary>
    /// Creates a new scope based off this service container.
    /// All transient types will be an isolated singleton instance within this scope.
    /// </summary>
    /// <returns>The new service container</returns>
    public ServiceContainer Scope()
    {
        return new ServiceContainer(this);
    }

    private Type GetConcreteType(Type type)
    {
        return type.IsInterface 
            ? _registeredInterfaces[type] 
            : type;
    }

    private Lifetime? GetLifetime(Type concreteType)
    {
        if (!_registeredTypes.TryGetValue(concreteType, out var lifetime))
        {
            if (_parent is null)
            {
                return null;
            }
            
            return _parent.GetLifetime(concreteType);
        }

        return lifetime;
    }

    private object? Resolve(Type type)
    {
        var concreteType = GetConcreteType(type);
        var lifetime = GetLifetime(type);
        
        switch (lifetime)
        {
            // If the target is a singleton, resolve the instance
            //  from the root container down to this one.
            case Lifetime.Singleton when _parent is null:
            {
                if (_instances.TryGetValue(concreteType, out var instance))
                {
                    return instance;
                }
                
                var obj = Instantiate(concreteType);
                _instances.Add(concreteType, obj);
                return obj;
            }
            
            case Lifetime.Singleton:
                return _parent.Resolve(type);
            
            // If the target is scoped, resolve the instance
            //  from the this container up to the root container.
            case Lifetime.Scoped:
            {
                var instance = _instances.GetValueOrDefault(concreteType);
                if (instance is not null)
                {
                    return instance;
                }

                if (_parent is null)
                {
                    return null;
                }
            
                var result = _parent.Resolve(type);
                if (result is not null)
                {
                    return result;
                }
            
                var obj = Instantiate(concreteType);
                _instances.Add(concreteType, obj);
                return obj;
            }
            
            // If the target is transient, immediately return a new instance.
            case Lifetime.Transient:
                return Instantiate(concreteType);
            
            default:
                return null;
        }
    }
    
    private object Instantiate(Type concreteType)
    {
        var instance = Activator.CreateInstance(concreteType)!;
        var injectableProperties = concreteType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => Attribute.IsDefined(p, typeof(InjectAttribute)));
        foreach (var property in injectableProperties)
        {
            // https://stackoverflow.com/questions/3706389/changing-read-only-properties-with-reflection
            property.SetValue(instance, Resolve(property.PropertyType));
        }

        return instance;
    }
}