using System.Reflection.Metadata;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Common;

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
    }

    static void InitializeWindow()
    {
        WindowOptions winOptions = WindowOptions.Default;
        winOptions.Size = new Silk.NET.Maths.Vector2D<int>((int)DefaultWindowWidth, (int)DefaultWindowHeight);

        winOptions.Title = "Hello Silk";
        window = Window.Create(winOptions);
        startTime = DateTime.Now;
        window.Load += OnWindowLoad;
        window.Update += OnWindowUpdate;
        window.Render += OnWindowRender;

        window.Run();

    }
    static DateTime startTime;
    static TimeSpan time => DateTime.Now - startTime;

    private static unsafe void OnWindowRender(double obj)
    {
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        shaderProgram.Use();
        shaderProgram.SetVector2("offset",new System.Numerics.Vector2(0.5f,0.5f));
        gl.BindVertexArray(VAO);
        //gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        gl.DrawElements(PrimitiveType.TriangleStrip, 3, DrawElementsType.UnsignedInt, null);
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

        PrepareRenderingTriangle();
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



    static uint VBO, VAO, EBO;
    static Common.Shader shaderProgram;

    private static void PrepareRenderingTriangle()
    {
        float[] verticies ={
            0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // bottom left
            0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f
        };
        int[] indicies ={  // note that we start from 0!
            0, 1, 2,   // first triangle
            //1, 2, 3    // second triangle
        };
        VBO = gl.GenBuffer();
        VAO = gl.GenVertexArrays(1);
        EBO = gl.GenBuffers(1);

        gl.BindVertexArray(VAO);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, verticies, BufferUsageARB.StaticDraw);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
        gl.BufferData<int>(BufferTargetARB.ElementArrayBuffer, indicies, BufferUsageARB.StaticDraw);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);


        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindVertexArray(0);



        shaderProgram = new Common.Shader(gl, "shader.vs", "shader.fs");
        
    }

    static uint CreateShader(GLEnum shaderType, string shaderSource)
    {
        uint shaderID = gl.CreateShader(shaderType);

        gl.ShaderSource(shaderID, shaderSource);
        gl.CompileShader(shaderID);
        gl.GetShader(shaderID, GLEnum.CompileStatus, out int compileStatus);
        if (compileStatus != 1)
        {
            gl.GetShaderInfoLog(shaderID, out var shaderInfoLog);
            Console.WriteLine($"Shader Compile Error:\n{shaderInfoLog}");
        }
        else
        {
            Console.WriteLine("Shader compiled successfully!");
        }
        return shaderID;
    }
}