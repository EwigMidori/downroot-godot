namespace Downroot.Core.Content;

public interface IContentPack
{
    string PackId { get; }
    void Register(IContentRegistrar registrar);
}
