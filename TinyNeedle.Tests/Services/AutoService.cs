namespace TinyNeedle.Tests.Services;

[Dependency(Lifetime = Lifetime.Transient)]
public class AutoService
{
    [Inject] public CtorService CtorService { get; }

    public void Nice()
    {
        CtorService.DoSomeStuff();
    }
}