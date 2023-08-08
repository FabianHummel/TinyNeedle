using System.Collections;
using BindingFlags = System.Reflection.BindingFlags;

namespace TinyNeedle;

public partial class ServiceContainer : IEnumerable
{
    private readonly ServiceContainer? _parent;
    private readonly Dictionary<Type, TypeDefinition> _registeredTypes = new();
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

    private TypeDefinition? GetTypeDefinition(Type concreteType)
    {
        if (!_registeredTypes.TryGetValue(concreteType, out var typeDefinition))
        {
            if (_parent is null)
            {
                return null;
            }
            
            return _parent.GetTypeDefinition(concreteType);
        }

        return typeDefinition;
    }

    private object? Resolve(Type type, object[]? @params = null)
    {
        var concreteType = GetConcreteType(type);
        var typeDefinition = GetTypeDefinition(type);
        
        switch (typeDefinition?._lifetime)
        {
            // If the target is a singleton, resolve the instance
            //  from the root container down to this one.
            case Lifetime.Singleton when _parent is null:
            {
                if (_instances.TryGetValue(concreteType, out var instance))
                {
                    return instance;
                }
                
                var obj = Instantiate(concreteType, @params ?? typeDefinition.Value.@params);
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
            
                var obj = Instantiate(concreteType, @params ?? typeDefinition.Value.@params);
                _instances.Add(concreteType, obj);
                return obj;
            }
            
            // If the target is transient, immediately return a new instance.
            case Lifetime.Transient:
                return Instantiate(concreteType, @params ?? typeDefinition.Value.@params);
            
            default:
                return null;
        }
    }
    
    private object Instantiate(Type concreteType, object[]? @params)
    {
        var instance = Activator.CreateInstance(concreteType, @params)!;
        var properties = concreteType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var property in properties.Where(p => Attribute.IsDefined(p, typeof(InjectAttribute))))
        {
            // https://stackoverflow.com/questions/3706389/changing-read-only-properties-with-reflection
            if (concreteType.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) is {} field)
            {
                field.SetValue(instance, Resolve(property.PropertyType));
            }
        }

        return instance;
    }
}