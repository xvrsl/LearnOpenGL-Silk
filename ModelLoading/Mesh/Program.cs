using System.Data;
using System.Drawing;
using System.Numerics;
using Common;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Assimp;
using System.Runtime.InteropServices;

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

    struct Vertex
    {
        public Vector3D<float> position;
        public Vector3D<float> normal;
        public Vector2D<float> TexCoords;
        public byte dummy;
    }

    struct Texture
    {
        public uint id;
        public string type;

    }

    class Mesh
    {
        public List<Vertex> verticies;
        public List<uint> indicies;
        public List<Texture> textures;
        public Mesh(List<Vertex> verticies, List<uint> indicies, List<Texture> textures)
        {
            this.verticies = new List<Vertex>(verticies);
            this.indicies = new List<uint>(indicies);
            this.textures = new List<Texture>(textures);
            SetupMesh();
        }
        public void Draw(Common.Shader shader)
        {

        }
        uint VAO, VBO, EBO;
        unsafe void SetupMesh()
        {
            VAO = gl.GenVertexArray();
            VBO = gl.GenBuffer();
            EBO = gl.GenBuffer();

            gl.BindVertexArray(VAO);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);

            var vertSpan = (ReadOnlySpan<Vertex>)verticies.ToArray().AsSpan();
            gl.BufferData<Vertex>(BufferTargetARB.ArrayBuffer, verticies.ToArray(), BufferUsageARB.StaticDraw);

            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
            gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, indicies.ToArray(), BufferUsageARB.StaticDraw);
            int vertexSize= Marshal.SizeOf<Vertex>();
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)vertexSize, Marshal.OffsetOf<Vertex>("position"));
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, GLEnum.Float, false, (uint)vertexSize, Marshal.OffsetOf<Vertex>("normal"));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, GLEnum.Float, false, (uint)vertexSize, Marshal.OffsetOf<Vertex>("TexCoords"));

            gl.BindVertexArray(0);
        }
    }

    public static void Main()
    {
        context = new WindowContext("Learn OpenGL - Lighting - Colors", 800, 600);
        context.clearColor = Color.Aqua;
        context.onLoad += OnLoad;
        context.onRender += OnRender;
        context.onUpdate += OnUpdate;
        var posOffset = Marshal.OffsetOf<Vertex>("position");
        var normOffset = Marshal.OffsetOf<Vertex>("normal");
        var uvOffset = Marshal.OffsetOf<Vertex>("TexCoords");
        var dummyOffset = Marshal.OffsetOf<Vertex>("dummy");
        int vertexSize= Marshal.SizeOf<Vertex>();
        Console.WriteLine($"TEST {vertexSize} {posOffset} {normOffset} {uvOffset} {dummyOffset}");

        context.Run();
    }


    private static unsafe void OnLoad(WindowContext context)
    {
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

    private static void OnRender(WindowContext context, double deltaTime)
    {
    }
}
