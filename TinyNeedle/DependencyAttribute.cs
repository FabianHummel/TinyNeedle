namespace TinyNeedle;

public class DependencyAttribute : Attribute
{
    public required Lifetime Lifetime { get; init; }
}