using Common;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

public static class Program
{
    static WindowContext context;
    static GL gl => context.gl;
    static Camera camera = new Camera()
    {
        camPos = new(0, 0, -3)
    };
    public static void Main()
    {
        context = new WindowContext("Learn OpenGL - Lighting - Colors", 800, 600);
        context.onLoad += OnLoad;
        context.onRender += OnRender;
        context.onUpdate += OnUpdate;
        context.Run();
    }

    static Common.Shader lightShader;
    static uint lightingVAO;

    static float[] verticies ={
             -0.5f, -0.5f, -0.5f,0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f, 0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        }
   ;
    private static unsafe void OnLoad(WindowContext context)
    {
        uint vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, verticies, BufferUsageARB.StaticDraw);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        //vao
        lightingVAO = gl.GenVertexArray();
        gl.BindVertexArray(lightingVAO);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 5 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 5 * sizeof(float), 3*sizeof(float));
        gl.EnableVertexAttribArray(1);

        lightShader = new Common.Shader(gl, @"..\..\..\shader_light.vs", @"..\..\..\shader_light.fs");
        lightShader.Use();
        lightShader.SetVector3("objectColor", 1.0f, 0.5f, 0.31f);
        lightShader.SetVector3("lightColor", 1.0f, 1.0f, 1.0f);
    }

    private static void OnUpdate(WindowContext context, double deltaTime)
    {
    }

    private static void OnRender(WindowContext context, double deltaTime)
    {
        gl.BindVertexArray(lightingVAO);
        lightShader.Use();
        lightShader.SetMatrix("model",Matrix4X4.CreateTranslation(-1f,0f,0f));
        lightShader.SetMatrix("view",camera.GetViewMatrix());
        lightShader.SetMatrix("projection",camera.GetProjectionMatrix(context.window.Size));
        gl.DrawArrays(GLEnum.TriangleFan, 0,(uint) verticies.Length);
    }
}