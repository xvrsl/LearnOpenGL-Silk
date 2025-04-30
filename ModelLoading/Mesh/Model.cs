using System.Numerics;
using System.Runtime.InteropServices;
using Common;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

public class Model
{
    GL gl;
    Assimp assimp;
    List<Mesh> meshes;
    string directory;

    public void Draw(Common.Shader shader)
    {
        for (int i = 0; i < meshes.Count; i++)
        {
            meshes[i].Draw(shader);
        }
    }

    public Model(GL gl, string path)
    {
        this.gl = gl;
        assimp = Assimp.GetApi();
        LoadModel(path);
    }

    private unsafe void LoadModel(string path)
    {
        Scene* scene = assimp.ImportFile(path, (uint)(Silk.NET.Assimp.PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs));
        if (scene == null || (scene->MFlags & (uint)Silk.NET.Assimp.SceneFlags.Incomplete) != 0 || scene->MRootNode == null)
        {
            byte* errStrPtr = assimp.GetErrorString();
            var str = Marshal.PtrToStringUTF8((IntPtr)errStrPtr);
            Console.WriteLine($"ERROR: ASSIMP: {str}");
        }
        Console.WriteLine(path);
        directory = path.Substring(0, path.LastIndexOf('\\'));
        Console.WriteLine("Begin processing nodes");
        ProcessNode(scene->MRootNode, in scene);
    }
    private unsafe void ProcessNode(Node* node, ref readonly Scene* scene)
    {
        Console.WriteLine($"Processing node: {node->MName}");
        for (uint i = 0; i < node->MNumMeshes; i++)
        {
            Console.WriteLine($"Processing mesh[{i}]");
            Silk.NET.Assimp.Mesh* mesh = scene->MMeshes[node->MMeshes[i]];
            Console.WriteLine($"Processing mesh begin[{i}]");
            meshes.Add(ProcessMesh(mesh, in scene));
            Console.WriteLine($"Processing mesh[{i}] done");
        }

        Console.WriteLine($"Processing child: {node->MName}");
        for (uint i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], in scene);
        }
        Console.WriteLine($"Node processed: {node->MName}");
    }

    private unsafe Mesh ProcessMesh(Silk.NET.Assimp.Mesh* mesh, ref readonly Scene* scene)
    {
        List<Vertex> verticies = new List<Vertex>();
        List<uint> indicies = new List<uint>();
        List<Texture> textures = new List<Texture>();
        Console.WriteLine("Processing verts");
        for (uint i = 0; i < mesh->MNumVertices; i++)
        {
            Vertex v = new Vertex();
            //process verts
            var rawPos = mesh->MVertices[i];
            var rawNormal = mesh->MNormals[i];
            v.position = new(rawPos.X, rawPos.Y, rawPos.Z);
            v.normal = new(rawNormal.X, rawNormal.Y, rawNormal.Z);
            if (mesh->MTextureCoords[0] != null)
            {
                var rawUV = mesh->MTextureCoords[0][i];
                v.TexCoords = new(rawUV.X, rawUV.Y);
            }
            else v.TexCoords = new(0, 0);

            verticies.Add(v);
        }

        Console.WriteLine("Processing indicies");
        //process indicies
        for (uint i = 0; i < mesh->MNumFaces; i++)
        {
            Face face = mesh->MFaces[i];
            for (uint j = 0; j < face.MNumIndices; j++)
            {
                indicies.Add(face.MIndices[j]);
            }
        }

        //process material
        Console.WriteLine("Processing materials");
        if (mesh->MMaterialIndex >= 0)
        {
            Silk.NET.Assimp.Material* material = scene->MMaterials[mesh->MMaterialIndex];
            List<Texture> diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
            List<Texture> specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_diffuse");
            textures.AddRange(diffuseMaps);
            textures.AddRange(specularMaps);
        }
        Console.WriteLine("Mesh Processing Done");
        return new Mesh(gl, verticies, indicies, textures);
    }

    public static List<Texture> loadedTextures = new List<Texture>();
    unsafe List<Texture> LoadMaterialTextures(Silk.NET.Assimp.Material* mat, Silk.NET.Assimp.TextureType type, string typeName)
    {
        var count = assimp.GetMaterialTextureCount(mat, type);
        List<Texture> textures = new List<Texture>();
        for (uint i = 0; i < count; i++)
        {
            AssimpString str = default;
            TextureMapping mapping = default;
            uint uvIndex = default;
            float blend = default;
            TextureOp op = default;
            TextureMapMode mapMode = default;
            uint flags = default;
            assimp.GetMaterialTexture(mat, type, i, &str, &mapping, &uvIndex, &blend, &op, &mapMode, &flags);

            string path = str.AsString;

            var found = loadedTextures.Find(e => e.path == path);
            if (found.path == str.AsString)
            {
                textures.Add(found);
                continue;
            }

            Texture tex = new Texture();
            tex.id = Common.Texture.TextureFromFile(gl, directory + "\\" + str.AsString, PixelFormat.Rgba, GLEnum.Repeat, GLEnum.Linear);
            tex.type = typeName;
            tex.path = str;
            textures.Add(tex);
        }
        return textures;
    }


}