using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Common;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

struct Vertex
{
    public Vector3D<float> position;
    public Vector3D<float> normal;
    public Vector2D<float> TexCoords;
    public byte dummy;
}

public struct Texture
{
    public uint id;
    public string type;
    public string path;
}

class Mesh
{
    GL gl;
    public List<Vertex> verticies;
    public List<uint> indicies;
    public List<Texture> textures;
    public Mesh(GL gl, List<Vertex> verticies, List<uint> indicies, List<Texture> textures)
    {
        this.gl = gl;
        this.verticies = new List<Vertex>(verticies);
        this.indicies = new List<uint>(indicies);
        this.textures = new List<Texture>(textures);
        SetupMesh();
    }
    public unsafe void Draw( Common.Shader shader)
    {
        uint diffuseNr = 1;
        uint specularNr = 1;
        for (int i = 0; i < textures.Count; i++)
        {
            var texture = textures[i];
            gl.ActiveTexture(GLEnum.Texture0 + i);
            string num = "";
            if(texture.type == "texture_diffuse")
                num = $"{diffuseNr++}";
            if(texture.type == "texture_specular")
                num = $"{specularNr++}";
                
            shader.SetInt("material." + texture.type + num, i);
            gl.BindTexture(GLEnum.Texture2D, texture.id);
        }

        gl.ActiveTexture(GLEnum.Texture0);

        gl.BindVertexArray(VAO);
        shader.Use();
        gl.DrawElements(GLEnum.Triangles, (uint)indicies.Count, DrawElementsType.UnsignedInt, null);
        gl.BindVertexArray(0);
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
        int vertexSize = Marshal.SizeOf<Vertex>();
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)vertexSize, Marshal.OffsetOf<Vertex>("position"));
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, (uint)vertexSize, Marshal.OffsetOf<Vertex>("normal"));
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 2, GLEnum.Float, false, (uint)vertexSize, Marshal.OffsetOf<Vertex>("TexCoords"));

        gl.BindVertexArray(0);
    }
}
