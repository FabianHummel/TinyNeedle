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
            .Register<ChildService>()
            .Register<CtorService>(@params: new object[] { "Very cool!" });
    }
    
    [Test]
    public void AutoRegister()
    {
        {
            var rootDI = new ServiceContainer();
            rootDI.Register<CtorService>();
            rootDI.AutoRegister();

            Assert.Throws<MissingMethodException>(() => rootDI.ResolveRequired<AutoService>());
        }
        
        {
            var rootDI = new ServiceContainer();
            rootDI.Register<CtorService>(@params: new object[] { "That's fancy!" });
            rootDI.AutoRegister();

            var auto = rootDI.ResolveRequired<AutoService>();
            auto.Nice();
        }
    }
    
    [Test]
    public void TestRootDI()
    {
        var root = _rootDI.ResolveRequired<RootService>();
        root.Stuff();
    }
    
    [Test]
    public void TestNotRegistered()
    {
        Assert.Throws<NullReferenceException>(() => _rootDI.ResolveRequired<Tests>());
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
    
    [Test]
    public void TestConstructorWithParams()
    {
        var ctor = _rootDI.ResolveRequired<CtorService>(new object[]{ "Custom parameter!" });
        ctor.DoSomeStuff();
    }
    
    [Test]
    public void TestConstructor()
    {
        var ctor = _rootDI.ResolveRequired<CtorService>();
        ctor.DoSomeStuff();
    }
}