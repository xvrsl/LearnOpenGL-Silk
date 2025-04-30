using System.Runtime.InteropServices;
using Common;
using Silk.NET.Assimp;
using Silk.NET.Core.Contexts;

public class Model
{
    Assimp assimp;
    List<Mesh> meshes;
    string directory;

    public void Draw(Shader shader)
    {
        for (int i = 0; i < meshes.Count; i++)
        {
            meshes[i].Draw(shader);
        }
    }

    public Model(string path)
    {
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
        directory = path.Substring(0,path.LastIndexOf('/'));

        ProcessNode(scene->MRootNode, ref scene);
    }
    private unsafe void ProcessNode(Node* node, ref readonly Scene* scene)
    {

        throw new NotImplementedException();
    }

    private unsafe Mesh ProcessMesh(Silk.NET.Assimp.Mesh mesh, ref readonly Scene* scene)
    {
        throw new NotImplementedException();
    }

    List<Texture> LoadMaterialTextures(Silk.NET.Assimp.Material mat, Silk.NET.Assimp.TextureType type, string typeName)
    {
        throw new NotImplementedException();
    }


}