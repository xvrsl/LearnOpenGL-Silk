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

public static class Program
{

    static float[] cubeVerts =
        {
        // positions          
        -0.5f,  0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,
        0.5f, -0.5f, -0.5f,
        0.5f, -0.5f, -0.5f,
        0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,

        -0.5f, -0.5f,  0.5f,
        -0.5f, -0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f,  0.5f,
        -0.5f, -0.5f,  0.5f,

        0.5f, -0.5f, -0.5f,
        0.5f, -0.5f,  0.5f,
        0.5f,  0.5f,  0.5f,
        0.5f,  0.5f,  0.5f,
        0.5f,  0.5f, -0.5f,
        0.5f, -0.5f, -0.5f,

        -0.5f, -0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f,
        0.5f,  0.5f,  0.5f,
        0.5f,  0.5f,  0.5f,
        0.5f, -0.5f,  0.5f,
        -0.5f, -0.5f,  0.5f,

        -0.5f,  0.5f, -0.5f,
        0.5f,  0.5f, -0.5f,
        0.5f,  0.5f,  0.5f,
        0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f, -0.5f,

        -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f,  0.5f,
        0.5f, -0.5f, -0.5f,
        0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f,  0.5f,
        0.5f, -0.5f,  0.5f
    };
    static Common.Shader shaderRed, shaderGreen, shaderBlue, shaderYellow;
    static uint uboMatrices;
    static uint cubeVAO;
    private static unsafe void OnLoad(WindowContext context)
    {
        shaderRed = new Common.Shader(gl, @"resources\shader.vs", @"resources\shader.fs");
        shaderGreen = new Common.Shader(gl, @"resources\shader.vs", @"resources\shader.fs");
        shaderBlue = new Common.Shader(gl, @"resources\shader.vs", @"resources\shader.fs");
        shaderYellow = new Common.Shader(gl, @"resources\shader.vs", @"resources\shader.fs");

        shaderRed.SetVector4("color", new Vector4(1, 0, 0, 1));
        shaderGreen.SetVector4("color", new Vector4(0, 1, 0, 1));
        shaderBlue.SetVector4("color", new Vector4(0, 0, 1, 1));
        shaderYellow.SetVector4("color", new Vector4(1, 1, 0, 1));

        shaderRed.SetMatrix("model", Matrix4X4.CreateTranslation<float>(-0.75f, 0.75f, 0.0f));
        shaderGreen.SetMatrix("model", Matrix4X4.CreateTranslation<float>(0.75f, 0.75f, 0.0f));
        shaderBlue.SetMatrix("model", Matrix4X4.CreateTranslation<float>(0.75f, -0.75f, 0.0f));
        shaderYellow.SetMatrix("model", Matrix4X4.CreateTranslation<float>(-0.75f, -0.75f, 0.0f));

        uint uniformBlockIndexRed = gl.GetUniformBlockIndex(shaderRed.ID, "Matrices");
        uint uniformBlockIndexGreen = gl.GetUniformBlockIndex(shaderGreen.ID, "Matrices");
        uint uniformBlockIndexBlue = gl.GetUniformBlockIndex(shaderBlue.ID, "Matrices");
        uint uniformBlockIndexYellow = gl.GetUniformBlockIndex(shaderYellow.ID, "Matrices");

        gl.UniformBlockBinding(shaderRed.ID, uniformBlockIndexRed, 0);
        gl.UniformBlockBinding(shaderGreen.ID, uniformBlockIndexGreen, 0);
        gl.UniformBlockBinding(shaderBlue.ID, uniformBlockIndexBlue, 0);
        gl.UniformBlockBinding(shaderYellow.ID, uniformBlockIndexYellow, 0);

        uboMatrices = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.UniformBuffer, uboMatrices);
        gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(2 * sizeof(Matrix4X4<float>)), null, BufferUsageARB.StaticDraw);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

        gl.BindBufferRange(BufferTargetARB.UniformBuffer, 0, uboMatrices, 0, (nuint)(2 * sizeof(Matrix4X4<float>)));

        cubeVAO = gl.GenVertexArray();
        gl.BindVertexArray(cubeVAO);
        uint cubeArrayBuffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, cubeArrayBuffer);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, cubeVerts, BufferUsageARB.StaticDraw);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }
    private static unsafe void OnRender(WindowContext context, double deltaTime)
    {
        gl.Enable(EnableCap.DepthTest);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, uboMatrices);
        var prjRef = projection;
        var viewRef = view;
        gl.BufferSubData<Matrix4X4<float>>(BufferTargetARB.UniformBuffer, 0, (nuint)sizeof(Matrix4X4<float>), ref prjRef);
        gl.BufferSubData<Matrix4X4<float>>(BufferTargetARB.UniformBuffer, (nint)sizeof(Matrix4X4<float>), (nuint)sizeof(Matrix4X4<float>), ref viewRef);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);



        gl.BindVertexArray(cubeVAO);
        shaderRed.Use();
        gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        shaderGreen.Use();
        gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        shaderBlue.Use();
        gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        shaderYellow.Use();
        gl.DrawArrays(PrimitiveType.Triangles, 0, 36);

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
        //shader.SetMatrix("view", view);
        //shader.SetMatrix("projection", projection);

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
