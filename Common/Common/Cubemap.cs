using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.IO;
using Common;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

public class Cubemap
{
    public readonly GL gl;
    public readonly uint ID;
    public void Bind()
    {
        gl.BindTexture(TextureTarget.TextureCubeMap, ID);
    }
    public unsafe Cubemap(GL gl, string folder, string[] texture_faces)
    {
        this.gl = gl;
        ID = gl.GenTexture();
        gl.BindTexture(TextureTarget.TextureCubeMap, ID);

        for (int i = 0; i < texture_faces.Length; i++)
        {
            string path = folder + "/" + texture_faces[i];
            ImageResult result = ImageResult.FromMemory(System.IO.File.ReadAllBytes(path));
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
    }

}