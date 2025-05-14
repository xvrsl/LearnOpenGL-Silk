using System.Drawing;
using System.Numerics;
using Common;
using Common.Model;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

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

    static Model model;
    static Common.Shader objectShader;
    static Common.Shader screenShader;
    static uint objectVAO;
    static uint screenVAO;
    static float[] ScreenVerts = {
        -1,-1,0,    0,0,
        1,-1,0,     1,0,
        1,1,0,      1,1,
        -1,1,0,     0,1
    };
    static uint[] ScreenIndices ={
        0,1,2,
        0,2,3
    };

    static uint frameBuffer;
    static uint textureColorBuffer;
    private static unsafe void OnLoad(WindowContext context)
    {
        model = new Model(gl, @"..\..\..\kenney\air-hockey.obj");
        Console.WriteLine("Model loaded");
        //prepare shader
        objectShader = new Common.Shader(gl, @"..\..\..\shader.vs", @"..\..\..\shader_object.fs");
        screenShader = new Common.Shader(gl, @"..\..\..\FrameBufferShader.vs", @"..\..\..\FrameBufferShader.fs");


        objectShader.Use();
        objectShader.SetVector3("material.ambient", 1.0f, 0.5f, 0.31f);
        objectShader.SetVector3("material.specular", 0.5f, 0.5f, 0.5f);
        objectShader.SetFloat("material.shininess", 32f);

        gl.Uniform1(gl.GetUniformLocation(objectShader.ID, "material.emission"), 2);
        gl.ActiveTexture(TextureUnit.Texture2);
        gl.BindTexture(TextureTarget.Texture2D, 0);

        //frame buffer
        frameBuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);

        textureColorBuffer = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, textureColorBuffer);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, 800, 600, 0, PixelFormat.Rgb, PixelType.Int, null);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.BindTexture(TextureTarget.Texture2D, 0);


        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureColorBuffer, 0);

        if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            Console.WriteLine("ERROR: Frame Buffer is not complete (1)");
        }
        else
        {
            Console.WriteLine("Texture attached");
        }
        uint rbo;
        rbo = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, 800, 600);
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);

        if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            Console.WriteLine("ERROR: Frame Buffer is not complete (2)");
        }
        else
        {
            Console.WriteLine("Depth-Stencil Render Buffer Attached");
        }
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        //configure screen vao
        uint screenVertBuffer = gl.GenBuffer();
        screenVAO = gl.GenVertexArray();
        gl.BindVertexArray(screenVAO);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, screenVertBuffer);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, ScreenVerts, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 5 * sizeof(float), 0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        uint screenElementBuffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, screenElementBuffer);
        gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, ScreenIndices, BufferUsageARB.StaticDraw);
        gl.BindVertexArray(0);
        Console.WriteLine("Setup finished");
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
    private static unsafe void OnRender(WindowContext context, double deltaTime)
    {
        Console.WriteLine("R1");
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
        gl.Enable(EnableCap.DepthTest);
        gl.Enable(EnableCap.StencilTest);
        gl.ClearColor(Color.DarkSlateBlue);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.Clear(ClearBufferMask.DepthBufferBit);
        gl.Clear(ClearBufferMask.StencilBufferBit);
        SetupShaders(Matrix4X4<float>.Identity);
        model.Draw(objectShader);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        gl.ClearColor(1, 1, 1, 1);
        gl.Clear(ClearBufferMask.ColorBufferBit);


        screenShader.Use();
        gl.BindVertexArray(screenVAO);
        gl.Disable(EnableCap.DepthTest);
        gl.BindTexture(TextureTarget.Texture2D, textureColorBuffer);
        gl.DrawElements(PrimitiveType.Triangles, (uint)ScreenIndices.Length, DrawElementsType.UnsignedInt, null);
        //gl.DrawElements(PrimitiveType.Lines, (uint)ScreenIndices.Length, DrawElementsType.UnsignedInt, null);
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
    static void SetupShaders(Matrix4X4<float> modelMatrix)
    {
        SetShaderContext(objectShader, modelMatrix);
    }
}
