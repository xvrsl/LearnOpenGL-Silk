using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Common;
using Common.Model;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using StbImageSharp;
using Shader = Common.Shader;
public static class Program
{
    static float[] quadVertices = {
         // positions     // colors
        -0.05f,  0.05f,  1.0f, 0.0f, 0.0f,
        0.05f, -0.05f,  0.0f, 1.0f, 0.0f,
        -0.05f, -0.05f,  0.0f, 0.0f, 1.0f,

        -0.05f,  0.05f,  1.0f, 0.0f, 0.0f,
        0.05f, -0.05f,  0.0f, 1.0f, 0.0f,
        0.05f,  0.05f,  0.0f, 1.0f, 1.0f
    };
    static Shader quadShader;
    static Model planet, rock;
    static uint vao;
    static uint instanceVBO;
    private static unsafe void OnLoad(WindowContext context)
    {
        planet = new Model(gl, @"resources\planet\planet.obj");
        rock = new Model(gl, @"resources\rock\rock.obj");

        Vector2[] offsets = new Vector2[100];
        int index = 0;
        float offset = 0.1f;
        for (int y = -10; y < 10; y += 2)
        {
            for (int x = -10; x < 10; x += 2)
            {
                offsets[index++] = new Vector2(x / 10f + offset, y / 10f + offset);
            }
        }
        instanceVBO = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, instanceVBO);
        gl.BufferData<Vector2>(BufferTargetARB.ArrayBuffer, offsets, BufferUsageARB.StaticDraw);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        uint vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, quadVertices, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));
        gl.EnableVertexAttribArray(2);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, instanceVBO);
        gl.VertexAttribPointer(2, 2, GLEnum.Float, false, 2 * sizeof(float), 0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.VertexAttribDivisor(2, 1);

        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        quadShader = new Shader(gl, "resources/quadShader_vert.glsl", "resources/quadShader_frag.glsl");

    

    }
    private static unsafe void OnRender(WindowContext context, double deltaTime)
    {
        quadShader.Use();
        gl.BindVertexArray(vao);
        gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, 100);
    }

    #region BASE
    static WindowContext context;
    static GL gl => context.gl;
    static IInputContext input => context.input;
    static Common.Camera camera = new Common.Camera()
    {
        position = new(0, 0, -3)
    };
    static Matrix4X4<float> view => camera.GetViewMatrix();
    static Matrix4X4<float> projection => camera.GetProjectionMatrix(context.window.Size);
    static float spotLightAngle = 10f;
    static float CameraSpeed
    {
        get
        {
            if (context.input.Keyboards[0].IsKeyPressed(Key.ShiftLeft)) return 10f;
            return 1f;
        }
    }
    static Vector2 lastMousePos, mouseDelta;
    static float mouseSensitivity = 0.5f;
    public static void Main()
    {
        context = new WindowContext("Learn OpenGL", 800, 600);
        context.clearColor = Color.DarkSlateBlue;
        context.onLoad += OnLoad;
        context.onRender += OnRender;
        context.onUpdate += OnUpdate;

        context.Run();
    }
    private static void OnUpdate(WindowContext context, double deltaTime)
    {
        UpdateCamera(deltaTime);
    }

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
    #endregion
}
