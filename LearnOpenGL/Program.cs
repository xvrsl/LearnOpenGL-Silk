using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
public static class Program
{
    static uint DefaultWindowWidth => 800;
    static uint DefaultWindowHeight => 600;
    static IWindow window;
    private static IInputContext input;
    private static GL gl;

    public static void Main()
    {
        InitializeWindow();
        Console.Write("The end");
    }

    static void InitializeWindow()
    {
        WindowOptions winOptions = WindowOptions.Default;
        winOptions.Size = new Silk.NET.Maths.Vector2D<int>((int)DefaultWindowWidth, (int)DefaultWindowHeight);

        winOptions.Title = "Hello Silk";
        window = Window.Create(winOptions);
        window.Load += OnWindowLoad;
        window.Update += OnWindowUpdate;
        window.Render += OnWindowRender;
        window.Run();
    }

    private static void OnWindowLoad()
    {
        gl = window.CreateOpenGL();
        input = window.CreateInput();

        window.FramebufferResize += OnFramebufferResized;

        input.ConnectionChanged += OnInputConnectionChanged;

        foreach (var cur in input.Keyboards)
        {
            RegisterKeyboardEvents(cur);
        }
    }

    private static void OnInputConnectionChanged(IInputDevice device, bool connected)
    {
        if (device is IKeyboard keyboard)
        {
            if (connected) RegisterKeyboardEvents(keyboard);
            else UnregiseterKeyboardEvents(keyboard);
        }
    }

    private static void UnregiseterKeyboardEvents(IKeyboard keyboard)
    {
        keyboard.KeyDown -= OnInputKeyDown;
        keyboard.KeyChar -= OnInputKeyChar;
    }

    private static void OnInputKeyChar(IKeyboard keyboard, char c)
    {
    }

    private static void RegisterKeyboardEvents(IKeyboard keyboard)
    {
        keyboard.KeyChar += OnInputKeyChar;
        keyboard.KeyDown += OnInputKeyDown;
    }

    private static void OnInputKeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            window.Close();
        }
    }

    private static void OnFramebufferResized(Vector2D<int> d)
    {
        gl.Viewport(0, 0, (uint)window.Size.X, (uint)window.Size.Y);
    }

    private static void OnWindowUpdate(double obj)
    {
        
    }

    private static void OnWindowRender(double obj)
    {
        gl.ClearColor(0.2f,0.3f,0.3f,1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
    }

}