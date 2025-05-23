using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Common;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Common.Model;
public class Model
{
    GL gl;
    Assimp assimp;
    List<Mesh> meshes = new List<Mesh>();
    string directory;

    public void Draw(Common.Shader shader)
    {
        for (int i = 0; i < meshes.Count; i++)
        {
            meshes[i].Draw(shader);
        }
    }

    public Model(GL gl, string path, bool flipImageVertical = false)
    {
        this.gl = gl;
        assimp = Assimp.GetApi();
        LoadModel(path, flipImageVertical);
    }


    private unsafe void LoadModel(string path, bool flipImageVertical = false)
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
        ProcessNode(scene->MRootNode, in scene, flipImageVertical);
    }
    private unsafe void ProcessNode(Node* node, ref readonly Scene* scene, bool flipImageVertical = false)
    {
        Console.WriteLine($"Processing node: {node->MName}");
        for (uint i = 0; i < node->MNumMeshes; i++)
        {
            Silk.NET.Assimp.Mesh* mesh = scene->MMeshes[node->MMeshes[i]];
            meshes.Add(ProcessMesh(mesh, in scene, flipImageVertical));
        }

        Console.WriteLine($"Processing child: {node->MName}");
        for (uint i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], in scene, flipImageVertical);
        }
    }

    private unsafe Mesh ProcessMesh(Silk.NET.Assimp.Mesh* mesh, ref readonly Scene* scene, bool flipImageVertical = false)
    {
        List<Vertex> verticies = new List<Vertex>();
        List<uint> indicies = new List<uint>();
        List<Texture> textures = new List<Texture>();
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

        if (mesh->MMaterialIndex >= 0)
        {
            Silk.NET.Assimp.Material* material = scene->MMaterials[mesh->MMaterialIndex];
            List<Texture> diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse",flipImageVertical);
            List<Texture> specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_diffuse",flipImageVertical);
            textures.AddRange(diffuseMaps);
            textures.AddRange(specularMaps);
        }

        return new Mesh(gl, verticies, indicies, textures);
    }

    public static List<Texture> loadedTextures = new List<Texture>();
    unsafe List<Texture> LoadMaterialTextures(Silk.NET.Assimp.Material* mat, Silk.NET.Assimp.TextureType type, string typeName,bool flipImageVertical= false)
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
            string p = directory + "\\" + str.AsString;
            tex.id = Common.Texture.TextureFromFile(gl, p, GLEnum.Repeat, GLEnum.Linear,flipImageVertical);
            tex.type = typeName;
            tex.path = path;
            textures.Add(tex);
            loadedTextures.Add(tex);
        }

        return textures;
    }


}