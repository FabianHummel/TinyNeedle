namespace TinyNeedle.Tests.Services;

public class RootService
{
    [Inject] private ChildService Child { get; }

    public void Stuff()
    {
        Console.Out.WriteLine("Nice!");
    }

    public void ChildStuff()
    {
        Child.Bark();
    }
}