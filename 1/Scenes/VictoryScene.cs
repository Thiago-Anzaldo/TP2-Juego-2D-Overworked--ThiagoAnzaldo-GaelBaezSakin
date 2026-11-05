using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace _1.Scenes;

public class VictoryScene : Scene
{
    public override void Initialize()
    {
        base.Initialize();
        Core.ExitOnEscape = false;
        InitializeUI();
    }

    public override void LoadContent()
    {
        SoundEffect sonidoVictoria = Content.Load<SoundEffect>("audio/victoria");
        sonidoVictoria.Play(AudioSettings.EffectsVolume, 0f, 0f);
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        var panelVictoria = new Panel();
        panelVictoria.Dock(Gum.Wireframe.Dock.Fill);
        panelVictoria.AddToRoot();

        var etiquetaVictoria = new Label();
        etiquetaVictoria.Anchor(Gum.Wireframe.Anchor.Top);
        etiquetaVictoria.Y = 40;
        etiquetaVictoria.Width = 180;
        etiquetaVictoria.Text = "¡Ganaste!";
        panelVictoria.AddChild(etiquetaVictoria);

        var botonReintentar = new Button();
        botonReintentar.Anchor(Gum.Wireframe.Anchor.Bottom);
        botonReintentar.Y = -65;
        botonReintentar.Width = 130;
        botonReintentar.Text = "Volver a jugar";
        botonReintentar.Click += (_, _) => Core.ChangeScene(new GameScene());
        panelVictoria.AddChild(botonReintentar);

        var botonTitulo = new Button();
        botonTitulo.Anchor(Gum.Wireframe.Anchor.Bottom);
        botonTitulo.Y = -25;
        botonTitulo.Width = 140;
        botonTitulo.Text = "Volver al título";
        botonTitulo.Click += (_, _) => Core.ChangeScene(new TitleScene());
        panelVictoria.AddChild(botonTitulo);
    }

    public override void Update(GameTime gameTime)
    {
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Core.ChangeScene(new TitleScene());
            return;
        }

        GumService.Default.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.Black);
        GumService.Default.Draw();
    }
}
