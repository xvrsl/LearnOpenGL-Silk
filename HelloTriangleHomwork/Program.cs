using System.Reflection.Metadata;
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
    static uint shaderProgramA,shaderProgramB;
    static uint vao_1, vao_2;
    private static unsafe void OnWindowRender(double obj)
    {


        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        gl.BindVertexArray(vao_1);
        gl.UseProgram(shaderProgramA);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        gl.BindVertexArray(vao_2);
        gl.UseProgram(shaderProgramB);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);

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


        const string vertShaderSource_A =
        @"
        #version 330 core
        layout (location = 0) in vec3 aPos;
        void main()
        {
            gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
        }
        ";
        const string fragShaderSource_A =
        @"
        #version 330 core
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
        } 
        ";


        const string fragShaderSource_B =
        @"
        #version 330 core
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(1.0f, 1.0f, 0.0f, 1.0f);
        } 
        ";
        shaderProgramA = CreateShaderProgram(vertShaderSource_A, fragShaderSource_A);
        shaderProgramB = CreateShaderProgram(vertShaderSource_A, fragShaderSource_B);

        
        float[] triangles_A ={
            -0.5f,-0.25f,0.0f,
            -0.25f,-0.25f, 0.0f,
            -0.375f, 0.25f, 0.0f,

        };
        float[] triangles_B ={
            0.25f,-0.25f,0.0f,
            0.5f,-0.25f, 0.0f,
            0.375f, 0.25f, 0.0f,
        };

        vao_1 = gl.GenVertexArray();
        gl.BindVertexArray(vao_1);
        uint vbo_1 = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo_1);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, triangles_A, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        gl.UseProgram(shaderProgramA);

        vao_2 = gl.GenVertexArray();
        gl.BindVertexArray(vao_2);
        uint vbo_2 = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo_2);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, triangles_B, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        gl.UseProgram(shaderProgramB);
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

    static uint CreateShaderProgram(string vert, string frag)
    {
        uint shaderVert = CreateShader(GLEnum.VertexShader, vert);
        uint shaderFrag = CreateShader(GLEnum.FragmentShader, frag);
        uint result = gl.CreateProgram();
        gl.AttachShader(result, shaderVert);
        gl.AttachShader(result, shaderFrag);
        gl.LinkProgram(result);
        gl.GetProgram(result, GLEnum.LinkStatus, out int linkInfo);

        if (linkInfo != 1)
        {
            var log = gl.GetProgramInfoLog(result);
            Console.WriteLine($"Shader Link Error:\n {log}");
        }
        else
        {
            Console.WriteLine("Link succeed!");
        }

        gl.DeleteShader(shaderVert);
        gl.DeleteShader(shaderFrag);
        return result;
    }
}