using System;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;

namespace _1.Scenes;

public class TitleScene : Scene
{
    // El título es un sprite (tu imagen), no texto
    private Sprite _titulo;
    private Vector2 _tituloPos;
    private Sprite _fondo;
    private Panel _panelTitulo;

    public override void Initialize()
    {
        // base.Initialize() llama a LoadContent
        base.Initialize();

        // En el título, Escape cierra el juego
        Core.ExitOnEscape = true;

        // Arriba y centrado
        int anchoPantalla = Core.GraphicsDevice.PresentationParameters.BackBufferWidth;
        _tituloPos = new Vector2(anchoPantalla / 2f, 140);

        InitializeUI();
    }

    public override void LoadContent()
    {
        Texture2D textura = Content.Load<Texture2D>("images/TextoLogo");

        _titulo = new Sprite(new TextureRegion(textura, 0, 0, textura.Width, textura.Height));
        _titulo.CenterOrigin();
        // _titulo.Scale = new Vector2(2f, 2f);  

        Texture2D texturaFondo = Content.Load<Texture2D>("images/FondoTitulo");
        _fondo = new Sprite(new TextureRegion(texturaFondo, 0, 0, texturaFondo.Width, texturaFondo.Height));
    }

    private void InitializeUI()
    {
        // Limpiamos cualquier UI que haya quedado de otra escena
        GumService.Default.Root.Children.Clear();

        _panelTitulo = new Panel();
        _panelTitulo.Dock(Gum.Wireframe.Dock.Fill);
        _panelTitulo.AddToRoot();

        var botonJugar = new Button();
        botonJugar.Anchor(Gum.Wireframe.Anchor.Bottom);
        botonJugar.Y = -30;
        botonJugar.Width = 70;
        botonJugar.Text = "Jugar";
        botonJugar.Click += HandleJugarClicked;
        _panelTitulo.AddChild(botonJugar);
    }

    private void HandleJugarClicked(object sender, EventArgs e)
    {
        Core.ChangeScene(new GameScene());
    }

    public override void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.White);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _fondo.Draw(Core.SpriteBatch, Vector2.Zero);
        _titulo.Draw(Core.SpriteBatch, _tituloPos);
        Core.SpriteBatch.End();

        // La UI de Gum se dibuja después, arriba de todo
        GumService.Default.Draw();
    }
}