using System;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;

namespace _1.Scenes;

public class DefeatScene : Scene
{
    private const float ESCALA_MAPA = 3.8f;
    private Texture2D _texturaMapa;
    private Texture2D _texturaPixel;
    private Sprite _mapa;
    private bool _mostrarAjustes;

    public override void Initialize()
    {
        base.Initialize();
        Core.ExitOnEscape = false;
        InitializeUI();
    }

    public override void LoadContent()
    {
        SoundEffect sonidoDerrota = Content.Load<SoundEffect>("audio/derrota");
        sonidoDerrota.Play(AudioSettings.EffectsVolume, 0f, 0f);

        _texturaMapa = Content.Load<Texture2D>("images/mapa1-oficina");
        _texturaPixel = new Texture2D(Core.GraphicsDevice, 1, 1);
        _texturaPixel.SetData(new[] { Color.White });
        _mapa = new Sprite(new TextureRegion(_texturaMapa, 0, 0, _texturaMapa.Width, _texturaMapa.Height));
        _mapa.CenterOrigin();
        _mapa.Scale = new Vector2(ESCALA_MAPA, ESCALA_MAPA);
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        var panel = new Panel();
        panel.Dock(Gum.Wireframe.Dock.Fill);
        panel.AddToRoot();

        float separacion = CalcularSeparacionMenu(4, 38f, 48f);
        CrearEtiquetaCentrada(panel, "Perdiste", ObtenerPosicionMenu(0, 4, separacion), 180);
        CrearBotonCentrado(panel, "Reintentar", ObtenerPosicionMenu(1, 4, separacion), () =>
            Core.ChangeScene(new GameScene()));
        CrearBotonCentrado(panel, "Ajustes", ObtenerPosicionMenu(2, 4, separacion), MostrarMenuAjustes);
        CrearBotonCentrado(panel, "Volver al título", ObtenerPosicionMenu(3, 4, separacion), () =>
            Core.ChangeScene(new TitleScene()));
    }

    private void MostrarMenuAjustes()
    {
        _mostrarAjustes = true;
        GumService.Default.Root.Children.Clear();

        var panel = new Panel();
        panel.Dock(Gum.Wireframe.Dock.Fill);
        panel.AddToRoot();

        float separacion = CalcularSeparacionMenu(4, 38f, 54f);
        CrearEtiquetaCentrada(panel, "Ajustes", ObtenerPosicionMenu(0, 4, separacion), 180);
        CrearControlVolumen(panel, "Ambiente", ObtenerPosicionMenu(1, 4, separacion), true);
        CrearControlVolumen(panel, "Efectos", ObtenerPosicionMenu(2, 4, separacion), false);
        CrearBotonCentrado(panel, "Volver", ObtenerPosicionMenu(3, 4, separacion), VolverAlMenuDerrota);
    }

    private static float CalcularSeparacionMenu(int cantidadElementos, float altoElemento, float separacionMaxima)
    {
        float altoDisponible = Math.Max(0f, (float)GumService.Default.CanvasHeight - 20f);
        return Math.Min(separacionMaxima, (altoDisponible - altoElemento) / (cantidadElementos - 1));
    }

    private static float ObtenerPosicionMenu(int indice, int cantidadElementos, float separacion) =>
        (indice - (cantidadElementos - 1) / 2f) * separacion;

    private void CrearControlVolumen(Panel panel, string nombre, float posicionY, bool esMusica)
    {
        var etiqueta = CrearEtiquetaCentrada(panel, $"{nombre}: {ObtenerPorcentaje(esMusica)}%", posicionY, 145);
        var botonBajar = CrearBotonCentrado(panel, "-", posicionY, () =>
            AjustarVolumen(esMusica, -0.1f, nombre, etiqueta));
        botonBajar.X = -105;
        botonBajar.Width = 40;

        var botonSubir = CrearBotonCentrado(panel, "+", posicionY, () =>
            AjustarVolumen(esMusica, 0.1f, nombre, etiqueta));
        botonSubir.X = 105;
        botonSubir.Width = 40;
    }

    private static void AjustarVolumen(bool esMusica, float cambio, string nombre, Label etiqueta)
    {
        if (esMusica)
            AudioSettings.AdjustMusicVolume(cambio);
        else
            AudioSettings.AdjustEffectsVolume(cambio);

        etiqueta.Text = $"{nombre}: {ObtenerPorcentaje(esMusica)}%";
    }

    private static int ObtenerPorcentaje(bool esMusica) =>
        (int)Math.Round((esMusica ? AudioSettings.MusicVolume : AudioSettings.EffectsVolume) * 100f);

    private static Label CrearEtiquetaCentrada(Panel panel, string texto, float posicionY, int ancho)
    {
        var etiqueta = new Label();
        etiqueta.Anchor(Gum.Wireframe.Anchor.Center);
        etiqueta.Y = posicionY;
        etiqueta.Width = ancho;
        etiqueta.Text = texto;
        panel.AddChild(etiqueta);
        return etiqueta;
    }

    private static Button CrearBotonCentrado(Panel panel, string texto, float posicionY, Action accion)
    {
        var boton = new Button();
        boton.Anchor(Gum.Wireframe.Anchor.Center);
        boton.Y = posicionY;
        boton.Width = 150;
        boton.Text = texto;
        boton.Click += (_, _) => accion();
        panel.AddChild(boton);
        return boton;
    }

    private void VolverAlMenuDerrota()
    {
        _mostrarAjustes = false;
        InitializeUI();
    }

    public override void Update(GameTime gameTime)
    {
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            if (_mostrarAjustes)
                VolverAlMenuDerrota();
            else
                Core.ChangeScene(new TitleScene());
            return;
        }

        GumService.Default.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.Gray);

        Core.SpriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp);
        _mapa.Draw(
            Core.SpriteBatch,
            new Vector2(Core.GraphicsDevice.Viewport.Width / 2f, Core.GraphicsDevice.Viewport.Height / 2f));
        Core.SpriteBatch.Draw(
            _texturaPixel,
            new Rectangle(0, 0, Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height),
            new Color(0, 0, 0, 160));
        Core.SpriteBatch.End();

        GumService.Default.Draw();
    }

    public override void UnloadContent()
    {
        _texturaPixel?.Dispose();
        base.UnloadContent();
    }
}
