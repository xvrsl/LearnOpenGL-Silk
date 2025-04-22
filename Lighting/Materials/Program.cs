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
    static Camera camera = new Camera()
    {
        position = new(0, 0, -3)
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
             -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
     0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
     0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
     0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
    -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

    -0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,
     0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,
     0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,
     0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,
    -0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,
    -0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,

    -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
    -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
    -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
    -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
    -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
    -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

     0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
     0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
     0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
     0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
     0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
     0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

    -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
     0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
     0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
     0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

    -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
     0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
     0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
     0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
    -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
    -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
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
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 6 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        objectVAO = gl.GenVertexArray();
        gl.BindVertexArray(objectVAO);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 6 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        lightShader = new Common.Shader(gl, @"..\..\..\shader.vs", @"..\..\..\shader_light.fs");
        lightShader.Use();
        lightShader.SetVector3("lightColor", 1.0f, 1.0f, 1.0f);

        objectShader = new Common.Shader(gl, @"..\..\..\shader.vs", @"..\..\..\shader_object.fs");
        objectShader.Use();
        objectShader.SetVector3("material.ambient", 1.0f, 0.5f, 0.31f);
        objectShader.SetVector3("material.diffuse", 1.0f, 0.5f, 0.31f);
        objectShader.SetVector3("material.specular", 0.5f, 0.5f, 0.5f);
        objectShader.SetFloat("material.shininess", 32f);
        objectShader.SetVector3("light.ambient", 0.2f, 0.2f, 0.2f);
        objectShader.SetVector3("light.diffuse", 0.5f, 0.5f, 0.5f);
        objectShader.SetVector3("light.specular", 1f, 1f, 1f);



        lastMousePos = input.Mice[0].Position;
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

            if (input.Keyboards[0].IsKeyPressed(Key.ControlLeft))
            {
                Vector3D<float> mouseViewPos = new(
                mouse.Position.X / context.window.Size.X * 2 - 1,
                -(mouse.Position.Y / context.window.Size.Y * 2 - 1),
                 0.5f);
                float depth = Vector3D.Dot(lightPos - camera.position, camera.Forward);
                float planeHeight = depth * MathF.Tan(float.DegreesToRadians(camera.fieldOfView));
                float planeWidth = planeHeight / context.window.Size.Y * context.window.Size.X;
                Vector3D<float> resultPos = camera.position + camera.Forward * depth
                    + camera.Up * planeHeight * mouseViewPos.Y / 2
                    + camera.Right * planeWidth * mouseViewPos.X / 2;

                lightPos = resultPos;
            }
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

    private static void OnRender(WindowContext context, double deltaTime)
    {
        Vector3D<float> lightColor = new
        (
            MathF.Sin((float)context.TimeSinceStart.TotalSeconds * 2f),
            MathF.Sin((float)context.TimeSinceStart.TotalSeconds * 0.7f),
            MathF.Sin((float)context.TimeSinceStart.TotalSeconds * 1.3f)
        );

        gl.BindVertexArray(lightVAO);
        lightShader.Use();
        lightShader.SetMatrix("model",
            Matrix4X4.CreateScale(0.2f)
            * Matrix4X4.CreateTranslation(lightPos)
            );
        lightShader.SetMatrix("view", view);
        lightShader.SetMatrix("projection", projection);
        lightShader.SetVector3("lightColor", lightColor);
        gl.Enable(EnableCap.DepthTest);
        gl.DrawArrays(GLEnum.Triangles, 0, (uint)verticies.Length);

        gl.BindVertexArray(objectVAO);
        objectShader.Use();
        objectShader.SetMatrix("model",
            Matrix4X4.CreateFromAxisAngle<float>(Vector3D.Normalize(Vector3D<float>.One), (float)context.TimeSinceStart.TotalSeconds * 1f)
        );
        objectShader.SetMatrix("view", view);
        objectShader.SetMatrix("projection", projection);
        objectShader.SetVector3("light.position", lightPos);
        objectShader.SetVector3("viewPos", camera.position);


        Vector3D<float> diffuseColor = lightColor * 0.5f;
        Vector3D<float> ambientColor = lightColor * 0.2f;
        objectShader.SetVector3("light.ambient", ambientColor);
        objectShader.SetVector3("light.diffuse", diffuseColor);


        gl.Enable(EnableCap.DepthTest);
        gl.DrawArrays(GLEnum.Triangles, 0, (uint)verticies.Length);
    }
}