namespace TinyNeedle;

public struct TypeDefinition
{
    public readonly Lifetime _lifetime;
    public readonly object[]? @params;

    public TypeDefinition(Lifetime lifetime, object[]? @params)
    {
        _lifetime = lifetime;
        this.@params = @params;
    }
}