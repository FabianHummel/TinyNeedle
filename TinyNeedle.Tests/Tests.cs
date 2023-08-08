using TinyNeedle.Tests.Services;

namespace TinyNeedle.Tests;

public class Tests
{
    private ServiceContainer _rootDI = null!;
    
    [OneTimeSetUp]
    public void Setup()
    {
        _rootDI = new ServiceContainer()
            .Register<RootService>(Lifetime.Singleton)
            .Register<ChildService>(Lifetime.Transient);
    }
    
    [Test]
    public void TestRootDI()
    {
        var root = _rootDI.ResolveRequired<RootService>();
        root.Stuff();
    }

    [Test]
    public void TestScopedDI()
    {
        var scope = _rootDI.Scope();
        var root = scope.ResolveRequired<RootService>();
        root.Stuff();
    }
    
    [Test]
    public void TestInjectDI()
    {
        var root = _rootDI.ResolveRequired<RootService>();
        root.ChildStuff();
    }
}