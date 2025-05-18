using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
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

    static uint skyboxTex;

    static uint skyboxVAO;
    static float[] skyboxVertices =
    {
        // positions          
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        -1.0f,  1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f,  1.0f
    };
    static Common.Shader skyboxShader;
    static Common.Shader reflectiveShader;
    static Common.Model.Model model;
    private static unsafe void OnLoad(WindowContext context)
    {
        skyboxShader = new Common.Shader(gl, "resources/shaders/cubemap.vs", "resources/shaders/cubemap.fs");
        reflectiveShader = new Common.Shader(gl, "resources/shaders/basic.vs", "resources/shaders/reflective.fs");
        model = new Model(gl, @"resources\kenney\air-hockey.obj", false);

        string[] faces = {
            "right.jpg",
            "left.jpg",
            "top.jpg",
            "bottom.jpg",
            "front.jpg",
            "back.jpg",
        };
        skyboxTex = LoadCubemap(gl, "resources/skybox", faces);

        skyboxVAO = gl.GenVertexArray();
        gl.BindVertexArray(skyboxVAO);
        uint vertBuffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertBuffer);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, skyboxVertices, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), 0);
        gl.BindVertexArray(0);

    }
    private static unsafe void OnRender(WindowContext context, double deltaTime)
    {
        gl.DepthMask(false);
        skyboxShader.Use();
        skyboxShader.SetMatrix("projection", projection);
        skyboxShader.SetMatrix("view", camera.GetViewMatrixWithoutPosition());
        gl.BindVertexArray(skyboxVAO);
        gl.BindTexture(TextureTarget.TextureCubeMap, skyboxTex);
        gl.DrawArrays(GLEnum.Triangles, 0, 36);
        gl.DepthMask(true);

        gl.Enable(EnableCap.DepthTest);
        reflectiveShader.Use();
        SetShaderContext(reflectiveShader, Matrix4X4<float>.Identity);
        model.Draw(reflectiveShader);
    }

    static unsafe uint LoadCubemap(GL gl, string folder, string[] texture_faces)
    {
        uint textureID = gl.GenTexture();
        gl.BindTexture(TextureTarget.TextureCubeMap, textureID);

        for (int i = 0; i < texture_faces.Length; i++)
        {
            string path = folder + "/" + texture_faces[i];
            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path));
            int width = result.Width;
            int height = result.Height;
            PixelFormat format = default;
            switch (result.SourceComp)
            {
                case ColorComponents.Default:
                    format = PixelFormat.Rgb;
                    break;
                case ColorComponents.Grey:
                    format = PixelFormat.Red;
                    break;
                case ColorComponents.GreyAlpha:
                    format = PixelFormat.RG;
                    break;
                case ColorComponents.RedGreenBlue:
                    format = PixelFormat.Rgb;
                    break;
                case ColorComponents.RedGreenBlueAlpha:
                    format = PixelFormat.Rgba;
                    break;
            }
            fixed (byte* ptr = result.Data) gl.TexImage2D(
                (TextureTarget)(TextureTarget.TextureCubeMapPositiveX + i), 0,
                InternalFormat.Rgb,
                (uint)width, (uint)height, 0, format,
                PixelType.UnsignedByte,
                ptr);
        }

        gl.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapR, (int)GLEnum.ClampToEdge);
        return textureID;
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
        context.clearColor = Color.Magenta;
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
