using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary;

public class Core : Game
{
    internal static Core s_instance;
    public static Core Instance => s_instance;

    private static Scene s_activeScene;
    private static Scene s_nextScene;

    public static GraphicsDeviceManager Graphics { get; private set; }
    public static new GraphicsDevice GraphicsDevice { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static new ContentManager Content { get; private set; }
    public static InputManager Input { get; private set; }

    /// <summary>Si es true, Escape cierra el juego.</summary>
    public static bool ExitOnEscape { get; set; }

    public Core(string title, int width, int height, bool fullScreen)
    {
        if (s_instance != null)
            throw new InvalidOperationException("Solo puede existir una instancia de Core");
        s_instance = this;

        Graphics = new GraphicsDeviceManager(this);
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;
        Graphics.ApplyChanges();

        Window.Title = title;

        Content = base.Content;
        Content.RootDirectory = "Content";

        IsMouseVisible = true;
        ExitOnEscape = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        GraphicsDevice = base.GraphicsDevice;
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        Input = new InputManager();
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update(gameTime);

        if (ExitOnEscape && Input.Keyboard.WasKeyJustPressed(Keys.Escape))
            Exit();

        // Si hay una escena pendiente, hacemos el cambio antes de actualizar
        if (s_nextScene != null)
            TransitionScene();

        s_activeScene?.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        s_activeScene?.Draw(gameTime);

        base.Draw(gameTime);
    }

    public static void ChangeScene(Scene next)
    {
        if (s_activeScene != next)
            s_nextScene = next;
    }

    private static void TransitionScene()
    {
        s_activeScene?.Dispose();

        GC.Collect();

        s_activeScene = s_nextScene;
        s_nextScene = null;

        // Initialize también llama a LoadContent de la escena
        s_activeScene?.Initialize();
    }
}