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
    static float CameraSpeed
    {
        get
        {
            if (context.input.Keyboards[0].IsKeyPressed(Key.ShiftLeft)) return 10f;
            return 1f;
        }
    }


    public static void Main()
    {
        context = new WindowContext("Learn OpenGL", 800, 600);
        context.clearColor = Color.DarkGray;
        context.onLoad += OnLoad;
        context.onRender += OnRender;
        context.onUpdate += OnUpdate;

        context.Run();
    }
    static float[] quadVerts =
    {
        -0.5f,-0.5f,0.0f,   0,0,1,    0f,0f,
        0.5f,-0.5f,0.0f,    0,0,1,    1f,0f,
        0.5f,0.5f,0.0f,     0,0,1,    1f,1f,
        -0.5f,0.5f,0.0f,    0,0,1,    0f,1f,

    };
    static int[] quadIndicies =
    {
        0,1,2,
        0,2,3
    };
    static uint vegitationVAO;
    static List<Vector3> vegetation = new()
    {
        new Vector3(-1.5f,  0.0f, -0.48f),
        new Vector3(1.5f,  0.0f,  0.51f),
        new Vector3(0.0f,  0.0f,  0.7f),
        new Vector3(-0.3f,  0.0f, -2.3f),
        new Vector3(0.5f,  0.0f, -0.6f),
    };
    static uint grassTexture;
    private static unsafe void OnLoad(WindowContext context)
    {
        uint quadVBO = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, quadVBO);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, quadVerts, BufferUsageARB.StaticDraw);
        uint quadEBO = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, quadEBO);
        gl.BufferData<int>(BufferTargetARB.ElementArrayBuffer, quadIndicies, BufferUsageARB.StaticDraw);

        grassTexture = Common.Texture.TextureFromFile(gl, "../../../grass.png", GLEnum.Repeat, GLEnum.Linear);
        vegitationVAO = gl.GenVertexArray();
        gl.BindVertexArray(vegitationVAO);
        gl.BindTexture(GLEnum.Texture2D, grassTexture);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, quadVBO);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, quadEBO);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)8 * sizeof(float), 0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, (uint)8 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, GLEnum.Float, false, (uint)8 * sizeof(float), 6 * sizeof(float));
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
            camera.position += camera.Forward * (float)deltaTime * CameraSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.S))
        {
            camera.position += camera.Backward * (float)deltaTime * CameraSpeed;
        }

        if (input.Keyboards[0].IsKeyPressed(Key.A))
        {
            camera.position += camera.Left * (float)deltaTime * CameraSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.D))
        {
            camera.position += camera.Right * (float)deltaTime * CameraSpeed;
        }

        if (input.Keyboards[0].IsKeyPressed(Key.E))
        {
            camera.position += camera.Up * (float)deltaTime * CameraSpeed;
        }
        else if (input.Keyboards[0].IsKeyPressed(Key.Q))
        {
            camera.position += -camera.Up * (float)deltaTime * CameraSpeed;
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


    }
    static void SetShaderContext(Common.Shader shader, Matrix4X4<float> modelMatrix)
    {
        shader.Use();
        //matrices
        shader.SetMatrix("view", view);
        shader.SetMatrix("projection", projection);

        //lights
        Vector3D<float> lightColor = Vector3D<float>.One;
        Vector3D<float> diffuseColor = lightColor * 0.5f;
        Vector3D<float> ambientColor = lightColor * 0.2f;
        //setup lights
        // dir light
        shader.SetVector3("dirLight.direction", new Vector3D<float>(1, -1, 0.5f));
        shader.SetVector3("dirLight.ambient", ambientColor);
        shader.SetVector3("dirLight.diffuse", diffuseColor);
        shader.SetVector3("dirLight.specular", lightColor);
        // spot light
        shader.SetVector3("spotLight.position", camera.position);
        shader.SetVector3("spotLight.direction", camera.Forward);
        shader.SetFloat("spotLight.cutOff", float.Cos(float.DegreesToRadians(spotLightAngle)));
        shader.SetFloat("spotLight.outerCutOff", float.Cos(float.DegreesToRadians(spotLightAngle + 5)));
        shader.SetFloat("spotLight.constant", 1.0f);
        shader.SetFloat("spotLight.linear", 0.09f);
        shader.SetFloat("spotLight.quadratic", 0.032f);
        shader.SetVector3("spotLight.ambient", ambientColor);
        shader.SetVector3("spotLight.diffuse", diffuseColor);
        shader.SetVector3("spotLight.specular", lightColor);
        //camera
        shader.SetVector3("viewPos", camera.position);
        shader.SetFloat("near", camera.nearPlane);
        shader.SetFloat("far", camera.farPlane);
        shader.SetMatrix("model", modelMatrix);
    }
}