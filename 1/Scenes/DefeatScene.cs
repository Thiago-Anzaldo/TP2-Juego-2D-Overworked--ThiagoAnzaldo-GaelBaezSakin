using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace _1.Scenes;

public class DefeatScene : Scene
{
    public override void Initialize()
    {
        base.Initialize();
        Core.ExitOnEscape = false;
        InitializeUI();
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        var panelDerrota = new Panel();
        panelDerrota.Dock(Gum.Wireframe.Dock.Fill);
        panelDerrota.AddToRoot();

        var etiquetaDerrota = new Label();
        etiquetaDerrota.Anchor(Gum.Wireframe.Anchor.Top);
        etiquetaDerrota.Y = 40;
        etiquetaDerrota.Width = 180;
        etiquetaDerrota.Text = "Perdiste";
        panelDerrota.AddChild(etiquetaDerrota);

        var botonReintentar = new Button();
        botonReintentar.Anchor(Gum.Wireframe.Anchor.Bottom);
        botonReintentar.Y = -65;
        botonReintentar.Width = 100;
        botonReintentar.Text = "Reintentar";
        botonReintentar.Click += (_, _) => Core.ChangeScene(new GameScene());
        panelDerrota.AddChild(botonReintentar);

        var botonTitulo = new Button();
        botonTitulo.Anchor(Gum.Wireframe.Anchor.Bottom);
        botonTitulo.Y = -25;
        botonTitulo.Width = 140;
        botonTitulo.Text = "Volver al título";
        botonTitulo.Click += (_, _) => Core.ChangeScene(new TitleScene());
        panelDerrota.AddChild(botonTitulo);
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
