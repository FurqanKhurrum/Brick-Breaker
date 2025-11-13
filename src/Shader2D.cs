using System;
using OpenTK.Graphics.OpenGL4;

namespace Breakout
{
    internal class Shader2D : IDisposable
    {
        public int Handle { get; private set; }

        public Shader2D(string vertexSrc, string fragmentSrc)
        {
            int vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, vertexSrc);
            GL.CompileShader(vertex);
            GL.GetShader(vertex, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
                throw new Exception("Vertex shader compile error: " + GL.GetShaderInfoLog(vertex));

            int fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fragmentSrc);
            GL.CompileShader(fragment);
            GL.GetShader(fragment, ShaderParameter.CompileStatus, out status);
            if (status == 0)
                throw new Exception("Fragment shader compile error: " + GL.GetShaderInfoLog(fragment));

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertex);
            GL.AttachShader(Handle, fragment);
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out status);
            if (status == 0)
                throw new Exception("Program link error: " + GL.GetProgramInfoLog(Handle));

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
        }

        public void Use() => GL.UseProgram(Handle);

        public void SetVector2(string name, OpenTK.Mathematics.Vector2 v)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            if (loc != -1)
                GL.Uniform2(loc, v);
        }

        public void SetVector3(string name, OpenTK.Mathematics.Vector3 v)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            if (loc != -1)
                GL.Uniform3(loc, v);
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteProgram(Handle);
                Handle = 0;
            }
        }
    }
}
