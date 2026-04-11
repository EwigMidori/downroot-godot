namespace Downroot.Core.Ids;

public readonly record struct ContentId(string Value)
{
    public override string ToString() => Value;
}
