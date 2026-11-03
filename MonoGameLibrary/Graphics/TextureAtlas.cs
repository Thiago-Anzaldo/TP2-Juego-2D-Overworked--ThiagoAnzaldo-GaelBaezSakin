using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class TextureAtlas
{
    private Dictionary<string, TextureRegion> _regions;
    private Dictionary<string, Animation> _animations;

    public Texture2D Texture { get; set; }

    public TextureAtlas()
    {
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    public TextureAtlas(Texture2D texture) : this()
    {
        Texture = texture;
    }

    public void AddRegion(string name, int x, int y, int width, int height)
        => _regions.Add(name, new TextureRegion(Texture, x, y, width, height));

    public TextureRegion GetRegion(string name) => _regions[name];
    public bool RemoveRegion(string name) => _regions.Remove(name);
    public void Clear() => _regions.Clear();

    public void AddAnimation(string name, Animation animation) => _animations.Add(name, animation);
    public Animation GetAnimation(string name) => _animations[name];
    public bool RemoveAnimation(string name) => _animations.Remove(name);

    public Sprite CreateSprite(string regionName) => new Sprite(GetRegion(regionName));

    public AnimatedSprite CreateAnimatedSprite(string animationName)
        => new AnimatedSprite(GetAnimation(animationName));

    public static TextureAtlas FromFile(ContentManager content, string fileName)
    {
        TextureAtlas atlas = new TextureAtlas();

        string filePath = Path.Combine(content.RootDirectory, fileName);

        using Stream stream = TitleContainer.OpenStream(filePath);
        using XmlReader reader = XmlReader.Create(stream);
        XElement root = XDocument.Load(reader).Root;

        atlas.Texture = content.Load<Texture2D>(root.Element("Texture").Value);

        var regions = root.Element("Regions")?.Elements("Region");
        if (regions != null)
        {
            foreach (var region in regions)
            {
                string name = region.Attribute("name")?.Value;
                int x = int.Parse(region.Attribute("x")?.Value ?? "0");
                int y = int.Parse(region.Attribute("y")?.Value ?? "0");
                int width = int.Parse(region.Attribute("width")?.Value ?? "0");
                int height = int.Parse(region.Attribute("height")?.Value ?? "0");

                if (!string.IsNullOrEmpty(name))
                    atlas.AddRegion(name, x, y, width, height);
            }
        }

        var animations = root.Element("Animations")?.Elements("Animation");
        if (animations != null)
        {
            foreach (var animElement in animations)
            {
                string name = animElement.Attribute("name")?.Value;
                double delayMs = double.Parse(animElement.Attribute("delay")?.Value ?? "0");

                var frames = new List<TextureRegion>();
                foreach (var frame in animElement.Elements("Frame"))
                    frames.Add(atlas.GetRegion(frame.Attribute("region").Value));

                atlas.AddAnimation(name, new Animation(frames, TimeSpan.FromMilliseconds(delayMs)));
            }
        }

        return atlas;
    }
}