using Silk.NET.OpenGL;
using StbImageSharp;

namespace Common;

public class Texture
{
    public readonly GL gl;
    public readonly uint ID;
    public unsafe Texture(GL gl, string path, PixelFormat format, GLEnum wrapMode, GLEnum magFilter)
    {
        this.gl = gl;

        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path));
        int width = result.Width;
        int height = result.Height;
        uint texture = gl.GenTexture();
        ID = texture;
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)wrapMode);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)wrapMode);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)magFilter);

        fixed (byte* ptr = result.Data) gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)result.Width, (uint)result.Height, 0, format, PixelType.UnsignedByte, ptr);
        //gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)result.Width, (uint)result.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (ReadOnlySpan<byte>)result.Data.AsSpan());
        gl.GenerateMipmap(TextureTarget.Texture2D);

        gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind(uint shaderID, string uniformName, TextureUnit target = TextureUnit.Texture0)
    {
        int uniformLocation = gl.GetUniformLocation(shaderID, uniformName);
        int bindTarget = (int)target - (int)TextureUnit.Texture0;
        Console.WriteLine($"bind {bindTarget}");
        gl.Uniform1(uniformLocation, bindTarget);
        gl.ActiveTexture(target);
        gl.BindTexture(TextureTarget.Texture2D, ID);
    }
}