using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Common;

public class WindowContext
{
    public GL gl { get; private set; }
    public IWindow window { get; private set; }
    public IInputContext input { get; private set; }
    public DateTime startTime { get; private set; }
    public TimeSpan TimeSinceStart{
        get{
            return DateTime.Now - startTime;
        }
    }
    public event Action<IInputDevice, bool> onInputConnectionChanged;
    public event Action<WindowContext> onLoad;
    public event Action<WindowContext, double> onUpdate;
    public event Action<WindowContext, double> onRender;

    public bool clearOnRender = true;
    public Color clearColor = Color.Black;

    public WindowContext(string title, int width, int height)
    {
        WindowOptions winOptions = WindowOptions.Default;
        winOptions.Size = new Silk.NET.Maths.Vector2D<int>(width, height);
        winOptions.Title = title;
        window = Window.Create(winOptions);
        startTime = DateTime.Now;
        window.Load += OnWindowLoad;
        window.Update += OnWindowUpdate;
        window.Render += OnWindowRender;
    }

    public void Run()
    {
        window.Run();
    }

    private void OnWindowLoad()
    {
        gl = window.CreateOpenGL();
        input = window.CreateInput();
        window.FramebufferResize += OnFramebufferResized;
        input.ConnectionChanged += OnInputConnectionChanged;
        onLoad?.Invoke(this);
    }

    private void OnInputConnectionChanged(IInputDevice device, bool connected)
    {
        onInputConnectionChanged?.Invoke(device, connected);
    }

    private void OnFramebufferResized(Vector2D<int> d)
    {
        gl.Viewport(0, 0, (uint)window.Size.X, (uint)window.Size.Y);
    }

    private void OnWindowUpdate(double obj)
    {
        onUpdate?.Invoke(this, obj);
    }

    private void OnWindowRender(double obj)
    {
        if (clearOnRender)
        {
            gl.ClearColor(clearColor);
            gl.Clear(ClearBufferMask.ColorBufferBit);
            gl.Clear(ClearBufferMask.DepthBufferBit);
        }
        onRender?.Invoke(this, obj);
    }


}