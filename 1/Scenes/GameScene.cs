using System;
using System.Collections.Generic;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
    private const float ESCALA_IMPRESORA = 1.8f;
    private const float ESCALA_MANCHA_CAFE = 2f;
    private const float ESCALA_TAREA_COMPLETADA = 1.5f;
    private const float ESCALA_PROGRESO_TAREA = 1.5f;
    private const int ANCHO_CUADRO_PROGRESO = 32;
    private const int CANTIDAD_CUADROS_PROGRESO = 10;
    private const float ANCHO_HITBOX = 35f;
    private const float ALTO_HITBOX = 35f;
    private const float DESPLAZAMIENTO_CENTRO_CABEZA_Y = -18f;
    private const float RADIO_IMPRESORA = 90f;
    private const float RADIO_MANCHA_CAFE = 45f;
    private const float PASO_BUSQUEDA_MANCHA = 19f;
    private const float DISTANCIA_MINIMA_MANCHA_INICIO = 200f;
    private const float DISTANCIA_MAXIMA_MANCHA_INICIO = 300f;
    private const float DISTANCIA_MINIMA_MANCHA_IMPRESORA = 140f;
    private static readonly TimeSpan TIEMPO_TAREA = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan BONIFICACION_TAREA = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DURACION_MENSAJE_TAREA = TimeSpan.FromSeconds(2.5);
    private static readonly TimeSpan DURACION_EFECTO_LIMPIEZA = TimeSpan.FromSeconds(2.5);
    private static readonly Keys[] TeclasDireccion = { Keys.D, Keys.A, Keys.S, Keys.W };
    private static readonly Vector2 PUNTO_IMPRESORA_MAPA = new(133f, 360f);

    private Sprite _mapa;
    private Sprite _impresora;
    private Sprite _manchaCafe;
    private Sprite _tareaLimpiarCompletada;
    private Texture2D _texturaMapa;
    private Texture2D _texturaImpresora;
    private Texture2D _texturaManchaCafe;
    private Texture2D _texturaTareaLimpiarCompletada;
    private Texture2D _texturaProgresoTarea;
    private Texture2D _texturaPixel;
    private SoundEffect _sonidoImpresora;
    private SoundEffect _sonidoLimpieza;
    private SoundEffectInstance _instanciaAmbiente;
    private Color[] _pixelesMapa;
    private AnimatedSprite _personaje;
    private Animation _animCaminar;
    private Animation _animQuieto;
    private Label _timerLabel;
    private Label _mensajeTareaLabel;

    private Vector2 _posicion;
    private Vector2 _posicionImpresora;
    private Vector2 _posicionManchaCafe;
    private Matrix _cameraTransform = Matrix.Identity;
    private TimeSpan _tiempoRestante = TimeSpan.FromSeconds(120);
    private TimeSpan _tiempoQuietoEnImpresora;
    private TimeSpan _tiempoQuietoEnManchaCafe;
    private TimeSpan _tiempoMensajeTarea;
    private TimeSpan _tiempoEfectoLimpieza;
    private bool _tareaImpresoraCompletada;
    private bool _manchaCafeCompletada;
    private EstadoMenuPausa _estadoMenuPausa;
    private readonly List<Keys> _teclasDireccionPresionadas = new();

    private enum EstadoMenuPausa
    {
        Cerrado,
        Pausa,
        Ajustes
    }

    public override void Initialize()
    {
        base.Initialize();

        // Escape pausa o reanuda la partida.
        Core.ExitOnEscape = false;

        InicializarUIJuego();
    }

    private void InicializarUIJuego()
    {
        GumService.Default.Root.Children.Clear();

        _timerLabel = new Label();
        _timerLabel.Anchor(Gum.Wireframe.Anchor.TopRight);
        _timerLabel.X = -5;
        _timerLabel.Y = 5;
        _timerLabel.Width = 55;
        _timerLabel.AddToRoot();

        _mensajeTareaLabel = new Label();
        _mensajeTareaLabel.Anchor(Gum.Wireframe.Anchor.Bottom);
        _mensajeTareaLabel.Y = -10;
        _mensajeTareaLabel.Width = 160;
        _mensajeTareaLabel.Text = " ";
        _mensajeTareaLabel.AddToRoot();
    }

    public override void LoadContent()
    {
        // Atlas del personaje (se comparte, por eso el Content global)
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        _animCaminar = atlas.GetAnimation("caminar-animation");
        _animQuieto = atlas.GetAnimation("quieto-animation");

        _personaje = atlas.CreateAnimatedSprite("quieto-animation");
        _personaje.CenterOrigin();
        _personaje.Origin = new Vector2(
            _personaje.Origin.X,
            _personaje.Origin.Y + DESPLAZAMIENTO_CENTRO_CABEZA_Y);
        _personaje.Scale = new Vector2(0.8f, 0.8f);

        // Los recursos propios de esta escena se liberan al cambiar de escena.
        _texturaMapa = Content.Load<Texture2D>("images/mapa1-oficina");
        _texturaImpresora = Content.Load<Texture2D>("images/Impresora");
        _texturaProgresoTarea = Content.Load<Texture2D>("images/Progreso Tarea");
        _texturaManchaCafe = Content.Load<Texture2D>("images/ManchaCafe");
        _texturaTareaLimpiarCompletada = Content.Load<Texture2D>("images/TareaLimpiarCompletada");
        _sonidoImpresora = Content.Load<SoundEffect>("audio/impresora");
        _sonidoLimpieza = Content.Load<SoundEffect>("audio/limpieza");
        SoundEffect sonidoAmbiente = Content.Load<SoundEffect>("audio/ambiente");
        _pixelesMapa = new Color[_texturaMapa.Width * _texturaMapa.Height];
        _texturaMapa.GetData(_pixelesMapa);
        _texturaPixel = new Texture2D(Core.GraphicsDevice, 1, 1);
        _texturaPixel.SetData(new[] { Color.White });

        _mapa = new Sprite(new TextureRegion(_texturaMapa, 0, 0, _texturaMapa.Width, _texturaMapa.Height));
        _mapa.CenterOrigin();
        _mapa.Scale = new Vector2(ESCALA_MAPA, ESCALA_MAPA);
        _impresora = new Sprite(new TextureRegion(
            _texturaImpresora,
            0,
            0,
            _texturaImpresora.Width,
            _texturaImpresora.Height));
        _impresora.CenterOrigin();
        _impresora.Scale = new Vector2(ESCALA_IMPRESORA, ESCALA_IMPRESORA);
        _impresora.Rotation = -MathHelper.PiOver2;

        Vector2 puntoInicioMapa = new(309f, 300f);
        _posicion = (puntoInicioMapa - new Vector2(_texturaMapa.Width / 2f, _texturaMapa.Height / 2f)) * ESCALA_MAPA;
        _posicionImpresora = (PUNTO_IMPRESORA_MAPA - new Vector2(_texturaMapa.Width / 2f, _texturaMapa.Height / 2f)) * ESCALA_MAPA;
        _posicionManchaCafe = ElegirPosicionManchaCafeAccesible();

        _manchaCafe = new Sprite(new TextureRegion(
            _texturaManchaCafe,
            0,
            0,
            _texturaManchaCafe.Width,
            _texturaManchaCafe.Height));
        _manchaCafe.CenterOrigin();
        _manchaCafe.Scale = new Vector2(ESCALA_MANCHA_CAFE, ESCALA_MANCHA_CAFE);

        _tareaLimpiarCompletada = new Sprite(new TextureRegion(
            _texturaTareaLimpiarCompletada,
            0,
            0,
            _texturaTareaLimpiarCompletada.Width,
            _texturaTareaLimpiarCompletada.Height));
        _tareaLimpiarCompletada.CenterOrigin();
        _tareaLimpiarCompletada.Scale = new Vector2(ESCALA_TAREA_COMPLETADA, ESCALA_TAREA_COMPLETADA);

        _instanciaAmbiente = sonidoAmbiente.CreateInstance();
        _instanciaAmbiente.IsLooped = true;
        _instanciaAmbiente.Volume = AudioSettings.MusicVolume;
        _instanciaAmbiente.Play();
    }

    public override void Update(GameTime gameTime)
    {
        KeyboardInfo teclado = Core.Input.Keyboard;

        if (teclado.WasKeyJustPressed(Keys.Escape))
        {
            switch (_estadoMenuPausa)
            {
                case EstadoMenuPausa.Cerrado:
                    MostrarMenuPausa();
                    break;
                case EstadoMenuPausa.Pausa:
                    Reanudar();
                    break;
                case EstadoMenuPausa.Ajustes:
                    MostrarMenuPausa();
                    break;
            }

            return;
        }

        if (_estadoMenuPausa != EstadoMenuPausa.Cerrado)
        {
            GumService.Default.Update(gameTime);
            return;
        }

        ActualizarOrientacion(teclado);

        _tiempoRestante -= gameTime.ElapsedGameTime;
        if (_tiempoRestante <= TimeSpan.Zero)
        {
            _tiempoRestante = TimeSpan.Zero;
            Core.ChangeScene(new DefeatScene());
            return;
        }

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

        ActualizarTareaImpresora(gameTime.ElapsedGameTime, estaCaminando);
        ActualizarTareaManchaCafe(gameTime.ElapsedGameTime, estaCaminando);
        ActualizarMensajeTarea(gameTime.ElapsedGameTime);
        ActualizarEfectoLimpieza(gameTime.ElapsedGameTime);
        _timerLabel.Text = $"{(int)_tiempoRestante.TotalMinutes:00}:{_tiempoRestante.Seconds:00}";

        if (_tareaImpresoraCompletada && _manchaCafeCompletada)
        {
            Core.ChangeScene(new VictoryScene());
            return;
        }

        _personaje.Update(gameTime);

        float cameraX = -_posicion.X + (Core.GraphicsDevice.Viewport.Width / 2f);
        float cameraY = -_posicion.Y + (Core.GraphicsDevice.Viewport.Height / 2f);
        _cameraTransform = Matrix.CreateTranslation(cameraX, cameraY, 0);
        GumService.Default.Update(gameTime);
    }

    private void ActualizarTareaImpresora(TimeSpan tiempoTranscurrido, bool estaCaminando)
    {
        if (!ActualizarProgresoTarea(
                _tareaImpresoraCompletada,
                ref _tiempoQuietoEnImpresora,
                _posicionImpresora,
                RADIO_IMPRESORA,
                tiempoTranscurrido,
                estaCaminando))
        {
            return;
        }

        _tareaImpresoraCompletada = true;
        CompletarTarea();
        _sonidoImpresora.Play(AudioSettings.EffectsVolume, 0f, 0f);
    }

    private void ActualizarTareaManchaCafe(TimeSpan tiempoTranscurrido, bool estaCaminando)
    {
        if (!ActualizarProgresoTarea(
                _manchaCafeCompletada,
                ref _tiempoQuietoEnManchaCafe,
                _posicionManchaCafe,
                RADIO_MANCHA_CAFE,
                tiempoTranscurrido,
                estaCaminando))
        {
            return;
        }

        _manchaCafeCompletada = true;
        _tiempoEfectoLimpieza = DURACION_EFECTO_LIMPIEZA;
        CompletarTarea();
        _sonidoLimpieza.Play(AudioSettings.EffectsVolume, 0f, 0f);
    }

    private bool ActualizarProgresoTarea(
        bool completada,
        ref TimeSpan progreso,
        Vector2 posicionTarea,
        float radio,
        TimeSpan tiempoTranscurrido,
        bool estaCaminando)
    {
        if (completada)
            return false;

        bool dentroDelRadio = Vector2.DistanceSquared(_posicion, posicionTarea) <= radio * radio;
        if (estaCaminando || !dentroDelRadio)
        {
            progreso = TimeSpan.Zero;
            return false;
        }

        progreso += tiempoTranscurrido;
        if (progreso < TIEMPO_TAREA)
            return false;

        progreso = TimeSpan.Zero;
        return true;
    }

    private void CompletarTarea()
    {
        _tiempoRestante += BONIFICACION_TAREA;
        _tiempoMensajeTarea = DURACION_MENSAJE_TAREA;
        _mensajeTareaLabel.Text = "Tarea hecha: +10 seg";
    }

    private void MostrarMenuPausa()
    {
        _estadoMenuPausa = EstadoMenuPausa.Pausa;
        GumService.Default.Root.Children.Clear();

        var panel = new Panel();
        panel.Dock(Gum.Wireframe.Dock.Fill);
        panel.AddToRoot();

        float separacion = CalcularSeparacionMenu(5, 38f, 48f);
        CrearEtiquetaCentrada(panel, "Pausa", ObtenerPosicionMenu(0, 5, separacion), 180);
        CrearBotonCentrado(panel, "Reanudar", ObtenerPosicionMenu(1, 5, separacion), Reanudar);
        CrearBotonCentrado(panel, "Reintentar", ObtenerPosicionMenu(2, 5, separacion), () => Core.ChangeScene(new GameScene()));
        CrearBotonCentrado(panel, "Ajustes", ObtenerPosicionMenu(3, 5, separacion), MostrarMenuAjustes);
        CrearBotonCentrado(panel, "Volver al título", ObtenerPosicionMenu(4, 5, separacion), () => Core.ChangeScene(new TitleScene()));
    }

    private void MostrarMenuAjustes()
    {
        _estadoMenuPausa = EstadoMenuPausa.Ajustes;
        GumService.Default.Root.Children.Clear();

        var panel = new Panel();
        panel.Dock(Gum.Wireframe.Dock.Fill);
        panel.AddToRoot();

        float separacion = CalcularSeparacionMenu(4, 38f, 54f);
        CrearEtiquetaCentrada(panel, "Ajustes", ObtenerPosicionMenu(0, 4, separacion), 180);
        CrearControlVolumen(panel, "Ambiente", ObtenerPosicionMenu(1, 4, separacion), true);
        CrearControlVolumen(panel, "Efectos", ObtenerPosicionMenu(2, 4, separacion), false);
        CrearBotonCentrado(panel, "Volver", ObtenerPosicionMenu(3, 4, separacion), MostrarMenuPausa);
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

    private void AjustarVolumen(bool esMusica, float cambio, string nombre, Label etiqueta)
    {
        if (esMusica)
        {
            AudioSettings.AdjustMusicVolume(cambio);
            _instanciaAmbiente.Volume = AudioSettings.MusicVolume;
        }
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

    private void Reanudar()
    {
        _estadoMenuPausa = EstadoMenuPausa.Cerrado;
        InicializarUIJuego();
    }

    private void ActualizarMensajeTarea(TimeSpan tiempoTranscurrido)
    {
        if (_tiempoMensajeTarea <= TimeSpan.Zero)
            return;

        _tiempoMensajeTarea -= tiempoTranscurrido;
        if (_tiempoMensajeTarea <= TimeSpan.Zero)
        {
            _tiempoMensajeTarea = TimeSpan.Zero;
            _mensajeTareaLabel.Text = " ";
        }
    }

    private void ActualizarEfectoLimpieza(TimeSpan tiempoTranscurrido)
    {
        if (_tiempoEfectoLimpieza <= TimeSpan.Zero)
            return;

        _tiempoEfectoLimpieza -= tiempoTranscurrido;
        if (_tiempoEfectoLimpieza < TimeSpan.Zero)
            _tiempoEfectoLimpieza = TimeSpan.Zero;
    }

    private void ActualizarOrientacion(KeyboardInfo teclado)
    {
        foreach (Keys tecla in TeclasDireccion)
        {
            if (teclado.WasKeyJustReleased(tecla))
                _teclasDireccionPresionadas.Remove(tecla);
        }

        foreach (Keys tecla in TeclasDireccion)
        {
            if (teclado.IsKeyDown(tecla) && !_teclasDireccionPresionadas.Contains(tecla))
                _teclasDireccionPresionadas.Add(tecla);
        }

        for (int i = _teclasDireccionPresionadas.Count - 1; i >= 0; i--)
        {
            Keys tecla = _teclasDireccionPresionadas[i];
            if (!teclado.IsKeyDown(tecla))
                continue;

            _personaje.Rotation = tecla switch
            {
                Keys.D => MathHelper.PiOver2,
                Keys.A => -MathHelper.PiOver2,
                Keys.S => MathHelper.Pi,
                Keys.W => 0f,
                _ => _personaje.Rotation
            };
            break;
        }
    }

    private bool PuedeMoverA(Vector2 posicion)
    {
        Rectangle hitbox = CrearHitbox(posicion);
        if (hitbox.Intersects(CrearHitboxImpresora()))
            return false;

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

    private Vector2 ElegirPosicionManchaCafeAccesible()
    {
        int limitePasos = (int)Math.Ceiling(
            Math.Max(_texturaMapa.Width, _texturaMapa.Height) * ESCALA_MAPA / PASO_BUSQUEDA_MANCHA);
        int centro = limitePasos;
        bool[,] visitados = new bool[limitePasos * 2 + 1, limitePasos * 2 + 1];
        Queue<Point> pendientes = new();
        List<Vector2> posicionesAccesibles = new();

        visitados[centro, centro] = true;
        pendientes.Enqueue(new Point(0, 0));

        // Busca desde el inicio para descartar ubicaciones bloqueadas por el mapa.
        while (pendientes.Count > 0)
        {
            Point paso = pendientes.Dequeue();
            Vector2 posicion = _posicion + new Vector2(
                paso.X * PASO_BUSQUEDA_MANCHA,
                paso.Y * PASO_BUSQUEDA_MANCHA);

            float distanciaInicioCuadrada = Vector2.DistanceSquared(posicion, _posicion);
            if (distanciaInicioCuadrada >=
                    DISTANCIA_MINIMA_MANCHA_INICIO * DISTANCIA_MINIMA_MANCHA_INICIO &&
                distanciaInicioCuadrada <=
                    DISTANCIA_MAXIMA_MANCHA_INICIO * DISTANCIA_MAXIMA_MANCHA_INICIO &&
                Vector2.DistanceSquared(posicion, _posicionImpresora) >=
                    DISTANCIA_MINIMA_MANCHA_IMPRESORA * DISTANCIA_MINIMA_MANCHA_IMPRESORA)
            {
                posicionesAccesibles.Add(posicion);
            }

            Explorar(paso.X - 1, paso.Y);
            Explorar(paso.X + 1, paso.Y);
            Explorar(paso.X, paso.Y - 1);
            Explorar(paso.X, paso.Y + 1);
        }

        if (posicionesAccesibles.Count == 0)
            throw new InvalidOperationException("No se encontró una ubicación accesible para la mancha de café.");

        return posicionesAccesibles[Random.Shared.Next(posicionesAccesibles.Count)];

        void Explorar(int pasoX, int pasoY)
        {
            int indiceX = pasoX + centro;
            int indiceY = pasoY + centro;
            if (indiceX < 0 || indiceX >= visitados.GetLength(0) ||
                indiceY < 0 || indiceY >= visitados.GetLength(1) ||
                visitados[indiceX, indiceY])
            {
                return;
            }

            visitados[indiceX, indiceY] = true;
            Vector2 siguiente = _posicion + new Vector2(
                pasoX * PASO_BUSQUEDA_MANCHA,
                pasoY * PASO_BUSQUEDA_MANCHA);

            if (PuedeMoverA(siguiente))
                pendientes.Enqueue(new Point(pasoX, pasoY));
        }
    }

    private Rectangle CrearHitboxImpresora()
    {
        int ancho = (int)Math.Ceiling(_texturaImpresora.Width * ESCALA_IMPRESORA);
        int alto = (int)Math.Ceiling(_texturaImpresora.Height * ESCALA_IMPRESORA);

        return new Rectangle(
            (int)Math.Floor(_posicionImpresora.X - ancho / 2f),
            (int)Math.Floor(_posicionImpresora.Y - alto / 2f),
            ancho,
            alto);
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
        if (!_manchaCafeCompletada)
            _manchaCafe.Draw(Core.SpriteBatch, _posicionManchaCafe);
        else if (_tiempoEfectoLimpieza > TimeSpan.Zero)
            _tareaLimpiarCompletada.Draw(Core.SpriteBatch, _posicionManchaCafe);
        _impresora.Draw(Core.SpriteBatch, _posicionImpresora);
        _personaje.Draw(
            Core.SpriteBatch,
            _posicion + new Vector2(0, DESPLAZAMIENTO_CENTRO_CABEZA_Y));

        if (!_tareaImpresoraCompletada)
            DibujarProgresoTarea(_tiempoQuietoEnImpresora, _posicionImpresora);
        if (!_manchaCafeCompletada)
            DibujarProgresoTarea(_tiempoQuietoEnManchaCafe, _posicionManchaCafe);

        Core.SpriteBatch.End();
        if (_estadoMenuPausa != EstadoMenuPausa.Cerrado)
        {
            Core.SpriteBatch.Begin(blendState: BlendState.AlphaBlend);
            Core.SpriteBatch.Draw(
                _texturaPixel,
                new Rectangle(0, 0, Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height),
                new Color(0, 0, 0, 160));
            Core.SpriteBatch.End();
        }

        GumService.Default.Draw();
    }

    private void DibujarProgresoTarea(TimeSpan tiempoProgreso, Vector2 posicionTarea)
    {
        if (tiempoProgreso <= TimeSpan.Zero)
            return;

        float progreso = (float)(tiempoProgreso.TotalSeconds / TIEMPO_TAREA.TotalSeconds);
        int cuadro = Math.Min((int)(progreso * CANTIDAD_CUADROS_PROGRESO), CANTIDAD_CUADROS_PROGRESO - 1);
        Rectangle origen = new(
            cuadro % 3 * ANCHO_CUADRO_PROGRESO,
            cuadro / 3 * ANCHO_CUADRO_PROGRESO,
            ANCHO_CUADRO_PROGRESO,
            ANCHO_CUADRO_PROGRESO);
        Vector2 posicionProgreso = posicionTarea + new Vector2(0, -60);
        Core.SpriteBatch.Draw(
            _texturaProgresoTarea,
            posicionProgreso,
            origen,
            Color.White,
            0f,
            new Vector2(ANCHO_CUADRO_PROGRESO / 2f, ANCHO_CUADRO_PROGRESO / 2f),
            ESCALA_PROGRESO_TAREA,
            SpriteEffects.None,
            0f);
    }

    public override void UnloadContent()
    {
        _instanciaAmbiente?.Stop();
        _instanciaAmbiente?.Dispose();
        _instanciaAmbiente = null;
        _texturaPixel?.Dispose();
        base.UnloadContent();
    }
}