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
    static Camera camera = new Camera()
    {
        camPos = new(0, 0, 3)
    };
    static float cameraSpeed = 1f;
    public static void Main()
    {
        context = new WindowContext("Learn OpenGL - Lighting - Colors", 800, 600);
        context.onLoad += OnLoad;
        context.onRender += OnRender;
        context.onUpdate += OnUpdate;
        context.Run();
    }

    static Common.Shader lightShader, objectShader;
    static Vector3D<float> lightPos = new(1.2f, 1.0f, 2.0f);
    static uint objectVAO, lightVAO;

    static float[] verticies ={
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
    private static unsafe void OnLoad(WindowContext context)
    {

        uint vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, verticies, BufferUsageARB.StaticDraw);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        //vao
        lightVAO = gl.GenVertexArray();
        gl.BindVertexArray(lightVAO);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 5 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        objectVAO = gl.GenVertexArray();
        gl.BindVertexArray(objectVAO);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 5 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        lightShader = new Common.Shader(gl, @"..\..\..\shader.vs", @"..\..\..\shader_light.fs");
        lightShader.Use();
        lightShader.SetVector3("lightColor", 1.0f, 1.0f, 1.0f);

        objectShader = new Common.Shader(gl, @"..\..\..\shader.vs", @"..\..\..\shader_object.fs");
        objectShader.Use();
        objectShader.SetVector3("objectColor", 1.0f, 0.5f, 0.31f);
        objectShader.SetVector3("lightColor", 1.0f, 1.0f, 1.0f);
    }

    private static void OnUpdate(WindowContext context, double deltaTime)
    {
        UpdateCamera(deltaTime);
    }
    static Vector2 lastMousePos, mouseDelta;
    static float mouseSensitivity =0.5f;
    private static void UpdateCamera(double deltaTime)
    {
        if (input.Keyboards[0].IsKeyPressed(Key.Escape))
        {
            context.window.Close();
        }

        if (input.Keyboards[0].IsKeyPressed(Key.W))
        {
            camera.camPos += camera.Forward * (float)deltaTime * cameraSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.S))
        {
            camera.camPos += camera.Backward * (float)deltaTime * cameraSpeed;
        }

        if (input.Keyboards[0].IsKeyPressed(Key.A))
        {
            camera.camPos += camera.Left * (float)deltaTime * cameraSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.D))
        {
            camera.camPos += camera.Right * (float)deltaTime * cameraSpeed;
        }

        if (input.Mice.Count != 0)
        {
            var mouse = input.Mice[0];

            mouseDelta.X = mouse.Position.X - lastMousePos.X;
            mouseDelta.Y = mouse.Position.Y - lastMousePos.Y;

            lastMousePos = mouse.Position;
        }
        if (mouseDelta.LengthSquared() > 0)
        {
            camera.yaw = camera.yaw - mouseDelta.X*mouseSensitivity;
            camera.pitch = Math.Clamp(camera.pitch + mouseDelta.Y*mouseSensitivity, -80, 80);
        }

    }

    static Matrix4X4<float> view => camera.GetViewMatrix();
    static Matrix4X4<float> projection => camera.GetProjectionMatrix(context.window.Size);

    private static void OnRender(WindowContext context, double deltaTime)
    {
        gl.BindVertexArray(lightVAO);
        lightShader.Use();
        lightShader.SetMatrix("model",
            Matrix4X4.CreateScale(0.2f)
            * Matrix4X4.CreateTranslation(lightPos)
            );
        lightShader.SetMatrix("view", view);
        lightShader.SetMatrix("projection", projection);
        gl.DrawArrays(GLEnum.Triangles, 0, (uint)verticies.Length);

        gl.BindVertexArray(objectVAO);
        objectShader.Use();
        objectShader.SetMatrix("model",
            Matrix4X4<float>.Identity
        );
        objectShader.SetMatrix("view", view);
        objectShader.SetMatrix("projection", projection);
        gl.DrawArrays(GLEnum.Triangles, 0, (uint)verticies.Length);
    }
}