using System.Drawing;
using System.Numerics;
using Common;
using Common.Model;
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
        //prepare shader
        objectShader = new Common.Shader(gl, @"..\..\..\shader.vs", @"..\..\..\shader_object.fs");
        objectShader.Use();
        objectShader.SetVector3("material.ambient", 1.0f, 0.5f, 0.31f);
        objectShader.SetVector3("material.specular", 0.5f, 0.5f, 0.5f);
        objectShader.SetFloat("material.shininess", 32f);
        gl.Uniform1(gl.GetUniformLocation(objectShader.ID, "material.emission"), 2);
        gl.ActiveTexture(TextureUnit.Texture2);
        gl.BindTexture(TextureTarget.Texture2D, 0);
        
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
        SetupShaderContext(objectShader, Matrix4X4<float>.Identity);
        //draw
        if (model != null) model.Draw(objectShader);

    }

    static void SetupShaderContext(Common.Shader shader, Matrix4X4<float> modelMatrix)
    {
        //matrices
        objectShader.SetMatrix("view", view);
        objectShader.SetMatrix("projection", projection);

        //lights
        Vector3D<float> lightColor = Vector3D<float>.One;
        Vector3D<float> diffuseColor = lightColor * 0.5f;
        Vector3D<float> ambientColor = lightColor * 0.2f;
        //setup lights
        // dir light
        objectShader.SetVector3("dirLight.direction", new Vector3D<float>(1, -1, 0.5f));
        objectShader.SetVector3("dirLight.ambient", ambientColor);
        objectShader.SetVector3("dirLight.diffuse", diffuseColor);
        objectShader.SetVector3("dirLight.specular", lightColor);
        // spot light
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
        //camera
        objectShader.SetVector3("viewPos", camera.position);

        //enable depth test
        gl.Enable(EnableCap.DepthTest);

        objectShader.SetMatrix("model", modelMatrix);
    }
}
