
using System.Drawing;
using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace Common
{
    public class Shader
    {
        readonly GL gl;
        public readonly uint ID;
        public Shader(GL gl, string vertexPath, string fragmentPath)
        {
            this.gl = gl;

            string vertexCode = File.ReadAllText(vertexPath);
            string fragmentCode = File.ReadAllText(fragmentPath);

            uint vertShader = MakeShader(vertexCode, ShaderType.VertexShader);
            uint fragShader = MakeShader(fragmentCode, ShaderType.FragmentShader);

            ID = gl.CreateProgram();
            gl.AttachShader(ID, vertShader);
            gl.AttachShader(ID, fragShader);
            gl.LinkProgram(ID);

            if (gl.GetProgram(ID, ProgramPropertyARB.LinkStatus) != 1)
            {
                var info = gl.GetProgramInfoLog(ID);
                Console.WriteLine($"Shader link error:\n{info}");
            }

            gl.DeleteShader(vertShader);
            gl.DeleteShader(fragShader);
        }

        uint MakeShader(string code, ShaderType type)
        {
            uint id = gl.CreateShader(type);
            gl.ShaderSource(id, code);
            gl.CompileShader(id);
            int compileSucceed = gl.GetShader(id, ShaderParameterName.CompileStatus);
            if (compileSucceed != 1)
            {
                Console.WriteLine($"Shader Compile Error:\n {gl.GetShaderInfoLog(id)}");
            }
            return id;
        }


        public void Use()
        {
            gl.UseProgram(ID);
        }
        public void SetBool(string name, bool value)
        {
            gl.Uniform1(gl.GetUniformLocation(ID, name), value ? 1 : 0);
        }
        public void SetInt(string name, int value)
        {
            gl.Uniform1(gl.GetUniformLocation(ID, name), value);
        }
        public void SetFloat(string name, float value)
        {
            gl.Uniform1(gl.GetUniformLocation(ID, name), value);
        }
        public void SetVector2(string name, Vector2 value)
        {
            gl.Uniform2(gl.GetUniformLocation(ID, name), value.X, value.Y);
        }

        public void SetVector3(string v1, float v2, float v3, float v4)
        {
            gl.Uniform3(gl.GetUniformLocation(ID, v1), v2, v3, v4);
        }

        public void SetMatrix(string v, Matrix4X4<float> matrix4X4)
        {
            gl.UniformMatrix4(gl.GetUniformLocation(ID,v), false, matrix4X4.ToSpan());
        }

        public void SetVector3(string v, Vector3D<float> lightPos)
        {
            gl.Uniform3(gl.GetUniformLocation(ID,v), (Vector3)lightPos);
        }

        public void SetVector4(string v,Vector4 value)
        {
            gl.Uniform4(gl.GetUniformLocation(ID,v),value);
        }
    }

}