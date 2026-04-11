using Godot;

namespace Downroot.Game.Infrastructure;

public sealed record TextureLoadResult(string ContentId, string AbsolutePath, Texture2D Texture);
