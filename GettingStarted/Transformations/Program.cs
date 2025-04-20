using System.Reflection.Metadata;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Common;
using StbImageSharp;
using System.Numerics;
public static class Program
{
    static uint DefaultWindowWidth => 800;
    static uint DefaultWindowHeight => 600;
    static IWindow window;
    private static IInputContext input;
    private static GL gl;

    static Matrix4X4<float> trans;
    static Matrix4X4<float> model, view, projection;
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
    private static unsafe void PrepareRenderingTriangle()
    {
        texture0 = CreateTexture(texture0Path, PixelFormat.Rgb, GLEnum.ClampToBorder, GLEnum.Linear);
        texture1 = CreateTexture(texture1Path, PixelFormat.Rgba, GLEnum.Repeat, GLEnum.Linear);

        float[] verticies ={
             -0.5f, -0.5f, -0.5f,0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f, 0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
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

        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        //gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        //gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(2, 2, GLEnum.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(2);



        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindVertexArray(0);



        shaderProgram = new Common.Shader(gl, vertShaderPath, fragShaderPath);

    }

    static DateTime startTime;
    static TimeSpan time => DateTime.Now - startTime;
    static float smileVisibility = 0.2f;

    static Vector3D<float>[] cubePositions = {
        new ( 0.0f,  0.0f,  0.0f),
        new ( 2.0f,  5.0f, -15.0f),
        new (-1.5f, -2.2f, -2.5f),
        new (-3.8f, -2.0f, -12.3f),
        new ( 2.4f, -0.4f, -3.5f),
        new (-1.7f,  3.0f, -7.5f),
        new ( 1.3f, -2.0f, -2.5f),
        new ( 1.5f,  2.0f, -2.5f),
        new ( 1.5f,  0.2f, -1.5f),
        new (-1.3f,  1.0f, -1.5f)
    };
    
    static float camSpeed = 10;
    static Camera camera = new Camera();

    private static unsafe void OnWindowRender(double obj)
    {
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.Clear(ClearBufferMask.DepthBufferBit);

        shaderProgram.Use();
        gl.Uniform1(gl.GetUniformLocation(shaderProgram.ID, "texture0"), 0);
        gl.Uniform1(gl.GetUniformLocation(shaderProgram.ID, "texture1"), 1);
        gl.Uniform1(gl.GetUniformLocation(shaderProgram.ID, "texture1Visibility"), smileVisibility);
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(GLEnum.Texture2D, texture0);
        gl.ActiveTexture(TextureUnit.Texture1);
        gl.BindTexture(GLEnum.Texture2D, texture1);

        gl.BindVertexArray(VAO);

        trans = Matrix4X4<float>.Identity;

        var axis = new Vector3D<float>(1.0f, 0.3f, 0.5f);
        axis = axis / axis.Length;

        //camPos = new Vector3D<float>(MathF.Sin((float)time.TotalSeconds), 0, MathF.Cos((float)time.TotalSeconds)) * 10;
        view = camera.GetViewMatrix();
        projection = camera.GetProjectionMatrix(window.Size.X,window.Size.Y);

        for (int i = 0; i < cubePositions.Count(); i++)
        {
            var cur = cubePositions[i];
            bool rotate = i % 3 == 0;
            float rotationDegree = 20f * i;
            if (rotate)
            {
                rotationDegree += (float)time.TotalSeconds * 10;
            }
            model = Matrix4X4<float>.Identity
                * Matrix4X4.CreateFromAxisAngle<float>(axis, float.DegreesToRadians(rotationDegree))
                * Matrix4X4.CreateTranslation(cur)
            ;
            trans = model * view * projection;
            float[] transList = new float[]
            {
                trans[0,0],trans[0,1],trans[0,2],trans[0,3],
                trans[1,0],trans[1,1],trans[1,2],trans[1,3],
                trans[2,0],trans[2,1],trans[2,2],trans[2,3],
                trans[3,0],trans[3,1],trans[3,2],trans[3,3],
            };
            gl.UniformMatrix4(gl.GetUniformLocation(shaderProgram.ID, "transform"), false,
            (ReadOnlySpan<float>)transList);
            //gl.DrawElements(PrimitiveType.TriangleStrip, 6, DrawElementsType.UnsignedInt, null);
            gl.Enable(EnableCap.DepthTest);
            gl.DrawArrays(GLEnum.Triangles, 0, 36);
        }

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
        foreach (var cur in input.Mice)
        {
            Console.WriteLine("Registering mouse");
            RegisterMouseEvents(cur);
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
        if (device is IMouse mouse)
        {
            if (connected) RegisterMouseEvents(mouse);
            else UnregisterMouseEvents(mouse);
        }
    }

    private static void UnregisterMouseEvents(IMouse mouse)
    {
        mouse.MouseMove += OnMouseMove;
    }

    private static void RegisterMouseEvents(IMouse mouse)
    {
        mouse.MouseMove -= OnMouseMove;
    }

    static Vector2 mouseDelta;
    private static void OnMouseMove(IMouse mouse, Vector2 vector)
    {
        Console.WriteLine($"Mouse move: {vector}");
        mouseDelta += vector;
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

    static float lastMouseX, lastMouseY;
    private static void OnWindowUpdate(double deltaTime)
    {

        if (input.Keyboards[0].IsKeyPressed(Key.W))
        {
            camera.camPos += camera.Forward * (float)deltaTime * camSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.S))
        {
            camera.camPos += camera.Backward * (float)deltaTime * camSpeed;
        }

        if (input.Keyboards[0].IsKeyPressed(Key.A))
        {
            camera.camPos += camera.Left * (float)deltaTime * camSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.D))
        {
            camera.camPos += camera.Right * (float)deltaTime * camSpeed;
        }

        if (input.Mice.Count != 0)
        {
            var mouse = input.Mice[0];

            mouseDelta.X = mouse.Position.X - lastMouseX;
            mouseDelta.Y = mouse.Position.Y - lastMouseY;

            lastMouseX = mouse.Position.X;
            lastMouseY = mouse.Position.Y;
        }
        if (mouseDelta.LengthSquared() > 0)
        {
            camera.yaw = camera.yaw - mouseDelta.X;
            camera.pitch = Math.Clamp(camera.pitch + mouseDelta.Y, -80, 80);
        }

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

}