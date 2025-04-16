using System.Reflection.Metadata;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Common;
using StbImageSharp;

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
    static float smileVisibility = 0;
    private static unsafe void OnWindowRender(double obj)
    {
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        shaderProgram.Use();
        gl.Uniform1(gl.GetUniformLocation(shaderProgram.ID, "texture0"), 0);
        gl.Uniform1(gl.GetUniformLocation(shaderProgram.ID, "texture1"), 1);
        gl.Uniform1(gl.GetUniformLocation(shaderProgram.ID, "texture1Visibility"), smileVisibility);

        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(GLEnum.Texture2D, texture0);
        gl.ActiveTexture(TextureUnit.Texture1);
        gl.BindTexture(GLEnum.Texture2D, texture1);

        gl.BindVertexArray(VAO);
        gl.DrawElements(PrimitiveType.TriangleStrip, 6, DrawElementsType.UnsignedInt, null);
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
        if (key == Key.Up)
        {
            smileVisibility = MathF.Min(smileVisibility + 0.1f, 1.0f);
        }
        if (key == Key.Down)
        {
            smileVisibility = MathF.Max(smileVisibility - 0.1f, 0.0f);
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
    static string texture0Path = "../../../container.jpg";
    static string texture1Path = "../../../awesomeface.png";
    static string vertShaderPath = "../../../shader.vs";
    static string fragShaderPath = "../../../shader.fs";
    static uint texture0, texture1;
    static unsafe uint CreateTexture(string path, PixelFormat format, GLEnum wrapMode, GLEnum magFilter)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path));
        int width = result.Width;
        int height = result.Height;
        uint texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)wrapMode);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)wrapMode);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)magFilter);

        fixed (byte* ptr = result.Data) gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)result.Width, (uint)result.Height, 0, format, PixelType.UnsignedByte, ptr);
        //gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)result.Width, (uint)result.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (ReadOnlySpan<byte>)result.Data.AsSpan());
        gl.GenerateMipmap(TextureTarget.Texture2D);
        return texture;
    }
    private static unsafe void PrepareRenderingTriangle()
    {
        texture0 = CreateTexture(texture0Path, PixelFormat.Rgb, GLEnum.ClampToBorder, GLEnum.Linear);
        texture1 = CreateTexture(texture1Path, PixelFormat.Rgba, GLEnum.Repeat, GLEnum.Linear);

        float[] verticies ={
            // positions          // colors           // texture coords
            0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f,   // top right
            0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   0.0f, 0.0f,   // bottom left
            -0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f,   0.0f, 1.0f    // top left 
        }
        ;
        int[] indicies ={  // note that we start from 0!
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };

        VBO = gl.GenBuffer();
        VAO = gl.GenVertexArrays(1);
        EBO = gl.GenBuffers(1);

        gl.BindVertexArray(VAO);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, verticies, BufferUsageARB.StaticDraw);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
        gl.BufferData<int>(BufferTargetARB.ElementArrayBuffer, indicies, BufferUsageARB.StaticDraw);

        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(2, 2, GLEnum.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        gl.EnableVertexAttribArray(2);



        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindVertexArray(0);



        shaderProgram = new Common.Shader(gl, vertShaderPath, fragShaderPath);

    }

}