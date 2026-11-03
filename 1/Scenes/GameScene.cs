using System;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace _1.Scenes;

public class GameScene : Scene
{
    private const float VELOCIDAD = 3.5f;
    private const float ESCALA_MAPA = 3.8f;
    private const float ANCHO_HITBOX = 35f;
    private const float ALTO_HITBOX = 35f;
    private const float DESPLAZAMIENTO_CENTRO_CABEZA_Y = -18f;

    private Sprite _mapa;
    private Texture2D _texturaMapa;
    private Texture2D _texturaPixel;
    private Color[] _pixelesMapa;
    private AnimatedSprite _personaje;
    private Animation _animCaminar;
    private Animation _animQuieto;
    private Label _timerLabel;

    private Vector2 _posicion;
    private Matrix _cameraTransform = Matrix.Identity;
    private TimeSpan _tiempoRestante = TimeSpan.FromSeconds(10);

    public override void Initialize()
    {
        base.Initialize();

        // En el juego, Escape vuelve al título en vez de cerrar
        // (si preferís que cierre el juego, poné true)
        Core.ExitOnEscape = false;

        // Sacamos el botón "Jugar" que quedó de la escena anterior
        GumService.Default.Root.Children.Clear();

        _timerLabel = new Label();
        _timerLabel.Anchor(Gum.Wireframe.Anchor.TopRight);
        _timerLabel.X = -5;
        _timerLabel.Y = 5;
        _timerLabel.Width = 55;
        _timerLabel.AddToRoot();
    }

    public override void LoadContent()
    {
        // Atlas del personaje (se comparte, por eso el Content global)
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        _animCaminar = atlas.GetAnimation("caminar-animation");
        _animQuieto = atlas.GetAnimation("quieto-animation");

        _personaje = atlas.CreateAnimatedSprite("quieto-animation");
        _personaje.CenterOrigin();
        _personaje.Scale = new Vector2(0.8f, 0.8f);

        // El mapa es solo de esta escena: usa el Content de la escena
        _texturaMapa = Content.Load<Texture2D>("images/mapa1-oficina");
        _pixelesMapa = new Color[_texturaMapa.Width * _texturaMapa.Height];
        _texturaMapa.GetData(_pixelesMapa);
        _texturaPixel = new Texture2D(Core.GraphicsDevice, 1, 1);
        _texturaPixel.SetData(new[] { Color.White });

        _mapa = new Sprite(new TextureRegion(_texturaMapa, 0, 0, _texturaMapa.Width, _texturaMapa.Height));
        _mapa.CenterOrigin();
        _mapa.Scale = new Vector2(ESCALA_MAPA, ESCALA_MAPA);

        Vector2 puntoInicioMapa = new(309f, 300f);
        _posicion = (puntoInicioMapa - new Vector2(_texturaMapa.Width / 2f, _texturaMapa.Height / 2f)) * ESCALA_MAPA;
    }

    public override void Update(GameTime gameTime)
    {
        KeyboardInfo teclado = Core.Input.Keyboard;

        if (teclado.WasKeyJustPressed(Keys.Escape))
        {
            Core.ChangeScene(new TitleScene());
            return;
        }

        _tiempoRestante -= gameTime.ElapsedGameTime;
        if (_tiempoRestante <= TimeSpan.Zero)
        {
            _tiempoRestante = TimeSpan.Zero;
            Core.ChangeScene(new DefeatScene());
            return;
        }

        _timerLabel.Text = $"{(int)_tiempoRestante.TotalMinutes:00}:{_tiempoRestante.Seconds:00}";

        Vector2 movimiento = Vector2.Zero;

        if (teclado.IsKeyDown(Keys.S)) movimiento.Y += VELOCIDAD;
        if (teclado.IsKeyDown(Keys.W)) movimiento.Y -= VELOCIDAD;
        if (teclado.IsKeyDown(Keys.A)) movimiento.X -= VELOCIDAD;
        if (teclado.IsKeyDown(Keys.D)) movimiento.X += VELOCIDAD;

        bool estaCaminando = false;
        Vector2 movimientoHorizontal = new(movimiento.X, 0);
        if (movimientoHorizontal.X != 0 && PuedeMoverA(_posicion + movimientoHorizontal))
        {
            _posicion.X += movimientoHorizontal.X;
            estaCaminando = true;
        }

        Vector2 movimientoVertical = new(0, movimiento.Y);
        if (movimientoVertical.Y != 0 && PuedeMoverA(_posicion + movimientoVertical))
        {
            _posicion.Y += movimientoVertical.Y;
            estaCaminando = true;
        }

        // Solo cambiamos de animación si es distinta (al asignarla se reinicia)
        Animation deseada = estaCaminando ? _animCaminar : _animQuieto;
        if (_personaje.Animation != deseada)
            _personaje.Animation = deseada;

        _personaje.Update(gameTime);

        // Cámara centrada en el personaje
        float cameraX = -_posicion.X + (Core.GraphicsDevice.Viewport.Width / 2f);
        float cameraY = -_posicion.Y + (Core.GraphicsDevice.Viewport.Height / 2f);
        _cameraTransform = Matrix.CreateTranslation((int)cameraX, (int)cameraY, 0);
        GumService.Default.Update(gameTime);
    }

    private bool PuedeMoverA(Vector2 posicion)
    {
        Rectangle hitbox = CrearHitbox(posicion);
        float izquierda = hitbox.Left;
        float derecha = hitbox.Right - 1;
        float arriba = hitbox.Top;
        float abajo = hitbox.Bottom - 1;

        float origenMapaX = -_texturaMapa.Width * ESCALA_MAPA / 2f;
        float origenMapaY = -_texturaMapa.Height * ESCALA_MAPA / 2f;
        float limiteMapaDerecho = origenMapaX + _texturaMapa.Width * ESCALA_MAPA;
        float limiteMapaInferior = origenMapaY + _texturaMapa.Height * ESCALA_MAPA;

        if (izquierda < origenMapaX || arriba < origenMapaY ||
            derecha > limiteMapaDerecho || abajo > limiteMapaInferior)
        {
            return false;
        }

        int inicioX = Math.Max(0, (int)Math.Floor((izquierda - origenMapaX) / ESCALA_MAPA));
        int finX = Math.Min(_texturaMapa.Width - 1, (int)Math.Floor((derecha - origenMapaX) / ESCALA_MAPA));
        int inicioY = Math.Max(0, (int)Math.Floor((arriba - origenMapaY) / ESCALA_MAPA));
        int finY = Math.Min(_texturaMapa.Height - 1, (int)Math.Floor((abajo - origenMapaY) / ESCALA_MAPA));

        for (int y = inicioY; y <= finY; y++)
        {
            for (int x = inicioX; x <= finX; x++)
            {
                if (_pixelesMapa[y * _texturaMapa.Width + x].A > 0)
                    return false;
            }
        }

        return true;
    }

    private static Rectangle CrearHitbox(Vector2 posicion)
    {
        float centroX = posicion.X;
        float centroY = posicion.Y + DESPLAZAMIENTO_CENTRO_CABEZA_Y;

        return new Rectangle(
            (int)centroX - (int)(ANCHO_HITBOX / 2f),
            (int)centroY - (int)(ALTO_HITBOX / 2f),
            (int)ANCHO_HITBOX,
            (int)ALTO_HITBOX);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.Gray);

        Core.SpriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null,
            _cameraTransform
        );

        _mapa.Draw(Core.SpriteBatch, Vector2.Zero);
        _personaje.Draw(Core.SpriteBatch, new Vector2((int)_posicion.X, (int)_posicion.Y));

        Rectangle hitbox = CrearHitbox(_posicion);
        const int grosor = 1;
        Core.SpriteBatch.Draw(_texturaPixel, new Rectangle(hitbox.X, hitbox.Y, hitbox.Width, grosor), Color.Red);
        Core.SpriteBatch.Draw(_texturaPixel, new Rectangle(hitbox.X, hitbox.Bottom - grosor, hitbox.Width, grosor), Color.Red);
        Core.SpriteBatch.Draw(_texturaPixel, new Rectangle(hitbox.X, hitbox.Y, grosor, hitbox.Height), Color.Red);
        Core.SpriteBatch.Draw(_texturaPixel, new Rectangle(hitbox.Right - grosor, hitbox.Y, grosor, hitbox.Height), Color.Red);

        Core.SpriteBatch.End();
        GumService.Default.Draw();
    }

    public override void UnloadContent()
    {
        _texturaPixel?.Dispose();
        base.UnloadContent();
    }
}