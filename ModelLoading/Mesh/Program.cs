using System.Drawing;
using System.Numerics;
using Common;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

public static class Program
{
    static WindowContext context;
    static GL gl => context.gl;
    static IInputContext input => context.input;
    static Common.Camera camera = new Common.Camera()
    {
        position = new(0, 0, -3)
    };
    static float cameraSpeed = 1f;
    static float[] verticies ={
        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f,
        0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 0.0f,
        0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f,
        0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   0.0f, 0.0f,
        0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   1.0f, 0.0f,
        0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   1.0f, 1.0f,
        0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   1.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   0.0f, 0.0f,

        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f,
        -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 1.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 0.0f,
        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f,

        0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,
        0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f,
        0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
        0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
        0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 0.0f,
        0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f,
        0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 1.0f,
        0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f,
        0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f,
        0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 1.0f,
        0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f,
        0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f
        }
       ;
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
    public static void Main()
    {
        context = new WindowContext("Learn OpenGL - Lighting - Colors", 800, 600);
        context.clearColor = Color.SlateBlue;
        context.onLoad += OnLoad;
        context.onRender += OnRender;
        context.onUpdate += OnUpdate;

        context.Run();
    }

    static Model model;
    static Common.Shader objectShader;
    static uint objectVAO;


    private static unsafe void OnLoad(WindowContext context)
    {
        model = new Model(gl, @"..\..\..\backpack\backpack.obj");
        Console.WriteLine("Model loaded");
        objectShader = new Common.Shader(gl, @"..\..\..\shader.vs", @"..\..\..\shader_object.fs");
        objectShader.Use();
        objectShader.SetVector3("material.ambient", 1.0f, 0.5f, 0.31f);
        objectShader.SetVector3("material.specular", 0.5f, 0.5f, 0.5f);
        objectShader.SetFloat("material.shininess", 32f);

        Common.Texture texAlbedo = new Common.Texture(gl, @"..\..\..\container2.png", PixelFormat.Rgba, GLEnum.Repeat, GLEnum.Linear);
        Common.Texture texSpecular = new Common.Texture(gl, @"..\..\..\container2_specular.png", PixelFormat.Rgba, GLEnum.Repeat, GLEnum.Linear);
        //Common.Texture texEmission = new Common.Texture(gl, @"..\..\..\matrix.jpg", PixelFormat.Rgb, GLEnum.Repeat, GLEnum.Linear);

        texAlbedo.Bind(objectShader.ID, "material.diffuse", TextureUnit.Texture0);
        texSpecular.Bind(objectShader.ID, "material.specular", TextureUnit.Texture1);
        //texEmission.Bind(objectShader.ID, "material.emission", TextureUnit.Texture2);
        gl.Uniform1(gl.GetUniformLocation(objectShader.ID, "material.emission"), 2);
        gl.ActiveTexture(TextureUnit.Texture2);
        gl.BindTexture(TextureTarget.Texture2D, 0);
        MakeCube();
    }

    static void MakeCube()
    {
        uint vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, verticies, BufferUsageARB.StaticDraw);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        objectVAO = gl.GenVertexArray();
        gl.BindVertexArray(objectVAO);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 8 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(2, 2, GLEnum.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        gl.EnableVertexAttribArray(2);
    }

    private static void OnUpdate(WindowContext context, double deltaTime)
    {
        UpdateCamera(deltaTime);
    }
    static Vector2 lastMousePos, mouseDelta;
    static float mouseSensitivity = 0.5f;
    private static void UpdateCamera(double deltaTime)
    {
        if (input.Keyboards[0].IsKeyPressed(Key.Escape))
        {
            context.window.Close();
        }

        if (input.Keyboards[0].IsKeyPressed(Key.W))
        {
            camera.position += camera.Forward * (float)deltaTime * cameraSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.S))
        {
            camera.position += camera.Backward * (float)deltaTime * cameraSpeed;
        }

        if (input.Keyboards[0].IsKeyPressed(Key.A))
        {
            camera.position += camera.Left * (float)deltaTime * cameraSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.D))
        {
            camera.position += camera.Right * (float)deltaTime * cameraSpeed;
        }

        if (input.Keyboards[0].IsKeyPressed(Key.E))
        {
            camera.position += camera.Up * (float)deltaTime * cameraSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.Q))
        {
            camera.position += -camera.Up * (float)deltaTime * cameraSpeed;
        }

        if (input.Mice.Count != 0)
        {
            var mouse = input.Mice[0];

            mouseDelta = mouse.Position - lastMousePos;
            lastMousePos = mouse.Position;

            if (mouseDelta.LengthSquared() > 0 && mouse.IsButtonPressed(MouseButton.Right))
            {
                camera.yaw = camera.yaw - mouseDelta.X * mouseSensitivity;
                camera.pitch = Math.Clamp(camera.pitch + mouseDelta.Y * mouseSensitivity, -80, 80);
            }
        }

    }

    static Matrix4X4<float> view => camera.GetViewMatrix();
    static Matrix4X4<float> projection => camera.GetProjectionMatrix(context.window.Size);
    static float spotLightAngle = 10f;
    private static void OnRender(WindowContext context, double deltaTime)
    {
        Vector3D<float> lightColor = Vector3D<float>.One;
        Vector3D<float> diffuseColor = lightColor * 0.5f;
        Vector3D<float> ambientColor = lightColor * 0.2f;
        objectShader.SetMatrix("view", view);
        objectShader.SetMatrix("projection", projection);
        objectShader.SetVector3("dirLight.direction", new Vector3D<float>(1, -1, 0.5f));
        objectShader.SetVector3("dirLight.ambient", ambientColor);
        objectShader.SetVector3("dirLight.diffuse", diffuseColor);
        objectShader.SetVector3("dirLight.specular", lightColor);

        objectShader.SetVector3("spotLight.position", camera.position);
        objectShader.SetVector3("spotLight.direction", camera.Forward);
        objectShader.SetFloat("spotLight.cutOff", float.Cos(float.DegreesToRadians(spotLightAngle)));
        objectShader.SetFloat("spotLight.outerCutOff", float.Cos(float.DegreesToRadians(spotLightAngle + 5)));
        objectShader.SetFloat("spotLight.constant", 1.0f);
        objectShader.SetFloat("spotLight.linear", 0.09f);
        objectShader.SetFloat("spotLight.quadratic", 0.032f);

        objectShader.SetVector3("spotLight.ambient", ambientColor);
        objectShader.SetVector3("spotLight.diffuse", diffuseColor);
        objectShader.SetVector3("spotLight.specular", lightColor);

        objectShader.SetVector3("viewPos", camera.position);
        gl.Enable(EnableCap.DepthTest);
        objectShader.SetMatrix("model", Matrix4X4<float>.Identity);
        if (model != null) model.Draw(objectShader);


    }
}
