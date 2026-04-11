using System.IO;
using Downroot.Core.Definitions;
using Godot;

namespace Downroot.Game.Infrastructure;

public sealed class PlayerAnimationFactory(PackPathResolver packPathResolver)
{
    private static readonly (string Name, int Row)[] Directions =
    [
        ("down", 0),
        ("left", 1),
        ("right", 2),
        ("up", 3)
    ];

    public SpriteFrames Create(CreatureDef creatureDef)
    {
        var idleImage = LoadImage(creatureDef.IdleSpriteSheetPath);
        var runImage = LoadImage(creatureDef.RunSpriteSheetPath);
        var frames = new SpriteFrames();

        foreach (var (name, row) in Directions)
        {
            var idleAnimation = $"idle_{name}";
            frames.AddAnimation(idleAnimation);
            frames.SetAnimationLoop(idleAnimation, true);
            frames.SetAnimationSpeed(idleAnimation, 2);
            frames.AddFrame(idleAnimation, ExtractFrame(idleImage, 0, row, 64, 64));

            var runAnimation = $"run_{name}";
            frames.AddAnimation(runAnimation);
            frames.SetAnimationLoop(runAnimation, true);
            frames.SetAnimationSpeed(runAnimation, 10);

            for (var column = 0; column < 8; column++)
            {
                frames.AddFrame(runAnimation, ExtractFrame(runImage, column, row, 64, 64));
            }
        }

        return frames;
    }

    private Image LoadImage(string packRelativePath)
    {
        var absolutePath = packPathResolver.ResolveAbsolutePath(packRelativePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Missing creature spritesheet.", absolutePath);
        }

        var image = Image.LoadFromFile(absolutePath);
        if (image is null || image.IsEmpty())
        {
            throw new InvalidOperationException($"Failed to load spritesheet '{absolutePath}'.");
        }

        return image;
    }

    private static Texture2D ExtractFrame(Image source, int column, int row, int frameWidth, int frameHeight)
    {
        var frame = Image.CreateEmpty(frameWidth, frameHeight, false, source.GetFormat());
        frame.BlitRect(source, new Rect2I(column * frameWidth, row * frameHeight, frameWidth, frameHeight), Vector2I.Zero);
        return ImageTexture.CreateFromImage(frame);
    }
}
