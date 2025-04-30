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

    public static void Main()
    {
        context = new WindowContext("Learn OpenGL - Lighting - Colors", 800, 600);
        context.clearColor = Color.Aqua;
        context.onLoad += OnLoad;
        context.onRender += OnRender;
        context.onUpdate += OnUpdate;

        context.Run();
    }

    static Model model;
    static Common.Shader objectShader;

    private static unsafe void OnLoad(WindowContext context)
    {
        model = new Model(gl, @"..\..\..\backpack\backpack.obj");

        objectShader = new Common.Shader(gl, @"..\..\..\shader.vs", @"..\..\..\shader_object.fs");
        objectShader.Use();
        objectShader.SetVector3("material.ambient", 1.0f, 0.5f, 0.31f);
        objectShader.SetVector3("material.specular", 0.5f, 0.5f, 0.5f);
        objectShader.SetFloat("material.shininess", 32f);

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
        objectShader.SetMatrix("view", view);
        objectShader.SetMatrix("projection", projection);

        objectShader.SetVector3("spotLight.position", camera.position);
        objectShader.SetVector3("spotLight.direction", camera.Forward);
        objectShader.SetFloat("spotLight.cutOff", float.Cos(float.DegreesToRadians(spotLightAngle)));
        objectShader.SetFloat("spotLight.outerCutOff", float.Cos(float.DegreesToRadians(spotLightAngle + 5)));
        objectShader.SetFloat("spotLight.constant", 1.0f);
        objectShader.SetFloat("spotLight.linear", 0.09f);
        objectShader.SetFloat("spotLight.quadratic", 0.032f);

        objectShader.SetVector3("viewPos", camera.position);

        
        gl.Enable(EnableCap.DepthTest);
        model.Draw(objectShader);
    }
}
