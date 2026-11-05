using _1.Scenes;
using Gum.Forms;
using Gum.Forms.Controls;
using MonoGameGum;
using MonoGameLibrary;

namespace _1;

public class Game1 : Core
{
    public Game1() : base("OverWorked!", 1280, 720, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();

        InitializeGum();

        // El juego arranca en la pantalla de inicio
        ChangeScene(new TitleScene());
    }

    private void InitializeGum()
    {
        // Inicializa Gum con los visuales por defecto (V3)
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3);

        // Gum usa el ContentManager global de Core
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;

        // Teclado y gamepad también controlan la UI (el mouse funciona solo)
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);

        // La UI se diseña a 1/4 de la resolución y se amplía x4 (como en la guía)
        GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth / 4.0f;
        GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight / 4.0f;
        GumService.Default.Renderer.Camera.Zoom = 4.0f;
    }
}