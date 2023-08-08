namespace TinyNeedle.Tests.Services;

public class CtorService
{
    public CtorService(string niceProperty)
    {
        NiceProperty = niceProperty;
    }


    public void DoSomeStuff()
    {
        Console.WriteLine(NiceProperty);
    }

    private string NiceProperty { get; }
}