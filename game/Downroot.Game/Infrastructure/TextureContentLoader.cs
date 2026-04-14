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

    public TextureLoadResult LoadItem(ItemDef itemDef)
    {
        var texture = LoadTexture(itemDef.Id.Value, itemDef.IconPath);
        return texture with
        {
            Texture = ToAtlas(texture.Texture, itemDef.IconWidth, itemDef.IconHeight, 0, 0)
        };
    }

    public TextureLoadResult LoadPlaceable(PlaceableDef placeableDef, bool isOpen = false)
    {
        var texture = LoadTexture(placeableDef.Id.Value, placeableDef.SpritePath);
        var atlasColumn = isOpen && placeableDef.HasOpenVariant ? placeableDef.OpenAtlasColumn : placeableDef.AtlasColumn;
        var atlasRow = isOpen && placeableDef.HasOpenVariant ? placeableDef.OpenAtlasRow : placeableDef.AtlasRow;

        return texture with
        {
            Texture = ToAtlas(texture.Texture, placeableDef.SpriteWidth, placeableDef.SpriteHeight, atlasColumn, atlasRow)
        };
    }

    public TextureLoadResult LoadResourceNode(ResourceNodeDef resourceNodeDef)
    {
        var texture = LoadTexture(resourceNodeDef.Id.Value, resourceNodeDef.SpritePath);
        return texture with
        {
            Texture = ToAtlas(texture.Texture, resourceNodeDef.SpriteWidth, resourceNodeDef.SpriteHeight, resourceNodeDef.AtlasColumn, resourceNodeDef.AtlasRow)
        };
    }

    public TextureLoadResult LoadRaisedFeature(RaisedFeatureDef raisedFeatureDef, byte variantIndex)
    {
        if (variantIndex >= raisedFeatureDef.AutoTileColumnCount)
        {
            throw new ArgumentOutOfRangeException(nameof(variantIndex), $"Raised feature variant {variantIndex} exceeds atlas column count {raisedFeatureDef.AutoTileColumnCount}.");
        }

        var texture = LoadTexture(raisedFeatureDef.Id.Value, raisedFeatureDef.TexturePath);
        return texture with
        {
            Texture = ToAtlas(texture.Texture, raisedFeatureDef.TileWidth, raisedFeatureDef.TileHeight, variantIndex, 0)
        };
    }

    public TextureLoadResult LoadCreature(CreatureDef creatureDef)
    {
        var path = creatureDef.WorldSpritePath ?? creatureDef.IdleSpriteSheetPath;
        var texture = LoadTexture(creatureDef.Id.Value, path);
        return texture with
        {
            Texture = ToAtlas(texture.Texture, creatureDef.SpriteWidth, creatureDef.SpriteHeight, 0, 0)
        };
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

    private static AtlasTexture ToAtlas(Texture2D texture, int width, int height, int column, int row)
    {
        return new AtlasTexture
        {
            Atlas = texture,
            Region = new Rect2(column * width, row * height, width, height)
        };
    }
}
