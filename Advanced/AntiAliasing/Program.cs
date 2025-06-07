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
    static Model planet, rock;
    static Shader shader, shaderInstanced, frameShader;
    const int amount = 100000;
    static Matrix4X4<float>[] matrices = new Matrix4X4<float>[amount];
    static int Rand()
    {
        return Random.Shared.Next();
    }
    static uint? msaaBuffer;
    static uint? screenTexture;
    static uint screenVAO;
    static uint intermediateFBO;
    static uint intermediateTex;
    private static unsafe void OnResize(WindowContext context, Vector2D<int> d)
    {
        uint width, height;
        width = (uint)d.X;
        height = (uint)d.Y;

        if (msaaBuffer != null)
        {
            gl.DeleteRenderbuffer(msaaBuffer.Value);
        }
        msaaBuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, msaaBuffer.Value);


        if (screenTexture != null)
        {
            gl.DeleteTexture(screenTexture.Value);
        }
        screenTexture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2DMultisample, screenTexture.Value);
        gl.TexImage2DMultisample(TextureTarget.Texture2DMultisample, 4, InternalFormat.Rgb, width, height, true);
        gl.TexParameterI(TextureTarget.Texture2DMultisample, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameterI(TextureTarget.Texture2DMultisample, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.BindTexture(TextureTarget.Texture2DMultisample, 0);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, screenTexture.Value, 0);

        uint rbo;
        rbo = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
        gl.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, 4, InternalFormat.Depth24Stencil8, width, height);
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

        intermediateFBO = CreateFBO(out intermediateTex);
    }

    private static unsafe uint CreateFBO(out uint fboTex)
    {
        uint width = (uint)context.window.Size.X;
        uint height = (uint)context.window.Size.Y;

        uint fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

        fboTex = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, fboTex);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.Int, null);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.BindTexture(TextureTarget.Texture2D, 0);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fboTex, 0);

        uint rbo;
        rbo = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, width, height);
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

        return fbo;
    }

    private static unsafe void OnLoad(WindowContext context)
    {
        OnResize(context, context.window.Size);
        PrepareRockAndPlanet();
        PrepareScreenVAO();
    }

    static unsafe void PrepareScreenVAO()
    {
        screenVAO = CreateScreenVAO();
        frameShader = new Shader(gl, "resources/FrameBufferShader.vs", "resources/FrameBufferShader.fs");

    }
    static unsafe void PrepareRockAndPlanet()
    {
        shader = new Shader(gl, "resources/shader.vs", "resources/shader_unlit.fs");
        shaderInstanced = new Shader(gl, "resources/shader_instanced.vs", "resources/shader_unlit.fs");
        planet = new Model(gl, @"resources\planet\planet.obj");
        rock = new Model(gl, @"resources\rock\rock.obj");
        float radius = 150;
        float offset = 100f;

        uint buffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);

        for (uint i = 0; i < amount; i++)
        {
            Matrix4X4<float> model = Matrix4X4<float>.Identity;
            float angle = (float)i / (float)amount * 2 * float.Pi;
            float displacement = (Rand() % (int)(2 * offset * 100)) / 100f - offset;
            float x = MathF.Sin(angle) * radius + displacement;
            displacement = (Rand() % (int)(2 * offset * 100)) / 100f - offset;
            float y = displacement * 0.4f;
            displacement = (Rand() % (int)(2 * offset * 100)) / 100f - offset;
            float z = MathF.Cos(angle) * radius + displacement;

            float scale = (Rand() % 20) / 100f + 0.05f;

            float rotAngle = float.DegreesToRadians(Rand() % 360);
            var axis = new Vector3(0.4f, 0.6f, 0.8f);
            axis = axis / axis.Length();
            model = model * Matrix4X4.CreateFromAxisAngle(new Vector3D<float>(axis.X, axis.Y, axis.Z), rotAngle);
            model = model * Matrix4X4.CreateScale(scale);
            model = model * Matrix4X4.CreateTranslation(x, y, z);

            matrices[i] = model;
        }
        gl.BufferData<Matrix4X4<float>>(BufferTargetARB.ArrayBuffer, matrices, BufferUsageARB.StaticDraw);

        for (int i = 0; i < rock.meshes.Count; i++)
        {
            uint vao = rock.meshes[i].VAO;
            gl.BindVertexArray(vao);

            var vec4Size = sizeof(Vector4D<float>);
            gl.EnableVertexAttribArray(3);
            gl.EnableVertexAttribArray(4);
            gl.EnableVertexAttribArray(5);
            gl.EnableVertexAttribArray(6);

            gl.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, (uint)(4 * vec4Size), 0);
            gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, (uint)(4 * vec4Size), 1 * vec4Size);
            gl.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, (uint)(4 * vec4Size), 2 * vec4Size);
            gl.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, (uint)(4 * vec4Size), 3 * vec4Size);

            gl.VertexAttribDivisor(3, 1);
            gl.VertexAttribDivisor(4, 1);
            gl.VertexAttribDivisor(5, 1);
            gl.VertexAttribDivisor(6, 1);

            gl.BindVertexArray(0);
        }

    }
    private static unsafe void OnRender(WindowContext context, double deltaTime)
    {
        var size = context.window.Size;
        int width = size.X;
        int height = size.Y;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, msaaBuffer.Value);
        gl.ClearColor(Color.Black);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.Clear(ClearBufferMask.DepthBufferBit);
        gl.Clear(ClearBufferMask.StencilBufferBit);
        DrawScene();

        gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, msaaBuffer.Value);
        gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, intermediateFBO);
        gl.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        gl.ClearColor(Color.Magenta);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.Clear(ClearBufferMask.DepthBufferBit);
        gl.Clear(ClearBufferMask.StencilBufferBit);
        DrawPostProcessingQuad();
    }

    static unsafe void DrawScene()
    {
        gl.Enable(EnableCap.DepthTest);
        SetShaderContext(shader, Matrix4X4.CreateScale<float>(4));
        planet.Draw(shader);

        shaderInstanced.Use();
        SetShaderContext(shaderInstanced, Matrix4X4<float>.Identity);
        for (int i = 0; i < rock.meshes.Count; i++)
        {
            var mesh = rock.meshes[i];
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, mesh.textures[0].id);
            gl.BindVertexArray(mesh.VAO);
            var indicies = mesh.indicies;
            gl.DrawElementsInstanced(GLEnum.Triangles,
             (uint)indicies.Count,
             DrawElementsType.UnsignedInt, null,
             amount);
        }
    }
    static uint CreateScreenVAO()
    {
        float[] ScreenVerts = {
        -1,-1,0,    0,0,
        1,-1,0,     1,0,
        1,1,0,      1,1,
        -1,1,0,     0,1
        };
        uint[] ScreenIndices ={
        0,1,2,
        0,2,3
        };
        uint screenVertBuffer = gl.GenBuffer();
        uint screenVAO = gl.GenVertexArray();
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
        return screenVAO;
    }

    static unsafe void DrawPostProcessingQuad()
    {
        frameShader.Use();
        gl.BindTexture(TextureTarget.Texture2D, intermediateTex);
        gl.BindVertexArray(screenVAO);
        gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
    }
    #region BASE
    static WindowContext context;
    static GL gl => context.gl;
    static IInputContext input => context.input;
    static Common.Camera camera = new Common.Camera()
    {
        position = new(0, 0, -3),
        farPlane = 1000f
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
        context = new WindowContext("Learn OpenGL", 800, 600, null);
        context.clearColor = Color.DarkSlateBlue;
        context.onLoad += OnLoad;
        context.onRender += OnRender;
        context.onUpdate += OnUpdate;
        context.onResize += OnResize;
        context.Run();
    }


    private static void OnUpdate(WindowContext context, double deltaTime)
    {
        UpdateCamera(deltaTime);
        if (input.Keyboards[0].IsKeyPressed(Key.F1))
        {
            Console.WriteLine($"{context.window.Size.X} {context.window.Size.Y}");
        }
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
