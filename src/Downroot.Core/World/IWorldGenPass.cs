namespace Downroot.Core.World;

public interface IWorldGenPass
{
    string Name { get; }
    void Execute(IWorldGenContext context);
}
