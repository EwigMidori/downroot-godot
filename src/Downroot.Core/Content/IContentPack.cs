namespace Downroot.Core.Content;

public interface IContentPack
{
    ContentPackManifest Manifest { get; }
    string PackId => Manifest.PackId;
    void Register(IContentRegistrar registrar);
}
