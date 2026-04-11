using System.IO;
using Downroot.Core.Definitions;
using Godot;

namespace Downroot.Game.Infrastructure;

public sealed class TextureContentLoader(PackPathResolver packPathResolver)
{
    public TextureLoadResult LoadTerrain(TerrainDef terrainDef)
    {
        var texture = LoadTexture(terrainDef.Id.Value, terrainDef.TexturePath);
        var atlas = new AtlasTexture
        {
            Atlas = texture.Texture,
            Region = new Rect2(
                terrainDef.AtlasColumn * terrainDef.TileWidth,
                terrainDef.AtlasRow * terrainDef.TileHeight,
                terrainDef.TileWidth,
                terrainDef.TileHeight)
        };

        return texture with { Texture = atlas };
    }

    public TextureLoadResult LoadItem(ItemDef itemDef) => LoadTexture(itemDef.Id.Value, itemDef.IconPath);

    public TextureLoadResult LoadPlaceable(PlaceableDef placeableDef)
    {
        var texture = LoadTexture(placeableDef.Id.Value, placeableDef.SpritePath);
        var atlas = new AtlasTexture
        {
            Atlas = texture.Texture,
            Region = new Rect2(0, 0, 32, 32)
        };

        return texture with { Texture = atlas };
    }

    public TextureLoadResult LoadTexture(string contentId, string packRelativePath)
    {
        var absolutePath = packPathResolver.ResolveAbsolutePath(packRelativePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"Missing content asset for '{contentId}'.", absolutePath);
        }

        var image = Image.LoadFromFile(absolutePath);
        if (image is null || image.IsEmpty())
        {
            throw new InvalidOperationException($"Failed to load image for '{contentId}' from '{absolutePath}'.");
        }

        var texture = ImageTexture.CreateFromImage(image);
        return new TextureLoadResult(contentId, absolutePath, texture);
    }
}
