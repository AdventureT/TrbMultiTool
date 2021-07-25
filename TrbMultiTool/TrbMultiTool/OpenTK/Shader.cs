using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace SEViewer.Core
{
    public class Shader : IDisposable
    {
        #region Constructor
        public Shader(string name, string vertShader, string fragShader)
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertShader);
            CompileShader(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragShader);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle, name);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);
                _uniformLocations.Add(key, location);
            }

            this.name = name;
            shaders.Add(this);
        }

        public Shader(string name, string shader, ShaderType shaderType)
        {
            var shaderHandle = GL.CreateShader(shaderType);
            GL.ShaderSource(shaderHandle, shader);
            CompileShader(shaderHandle);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, shaderHandle);

            LinkProgram(Handle, name);

            GL.DetachShader(Handle, shaderHandle);
            GL.DeleteShader(shaderHandle);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);
                _uniformLocations.Add(key, location);
            }

            this.name = name;
            shaders.Add(this);
        }

        #endregion

        #region Methods

        #region Private
        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program, string name)
        {
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                GL.GetProgram(program, GetProgramParameterName.InfoLogLength, out var maxLength);
                GL.GetProgramInfoLog(program, maxLength, out int length, out string info);

                throw new Exception($"Error occurred whilst linking Program({name}): {info}");
            }
        }

        #endregion

        #region Public

        #region OpenGL

        public static Shader Create(string name, string vertPath, string fragPath)
        {
            return new Shader(name, File.ReadAllText(vertPath), File.ReadAllText(fragPath));
        }

        public void Use()
        {
            if (this.disposed)
                throw new ObjectDisposedException("Shader: " + name);
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            if (this.disposed)
                throw new ObjectDisposedException("Shader: " + name);
            return GL.GetAttribLocation(Handle, attribName);
        }

        #endregion

        #region Disposing

        public static void DeleteShaders()
        {
            foreach(Shader shader in shaders.ToList())
            {
                shader.Dispose();
            }
        }

        public void Dispose()
        {
            if(!this.disposed)
            {
                shaders.Remove(this);
                this.disposed = true;
                GL.DeleteShader(Handle);
            }
        }

        #endregion

        #region Set Uniforms

        #region Vector1

        public void SetBoolean(string name, bool data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);
            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform1(_uniformLocations[name], data ? 1 : 0);
            }
        }

        public void SetInt(string name, int data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);
            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform1(_uniformLocations[name], data);
            }
        }

        public void SetInt(string name, int[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform1(_uniformLocations[string.Format("{0}[{1}]", name, i)], data[i]);
                }
            }
        }

        public void SetFloat(string name, float data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform1(_uniformLocations[name], data);
            }
        }

        public void SetFloat(string name, float[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform1(_uniformLocations[string.Format("{0}[{1}]", name, i)], data[i]);
                }
            }
        }

        public void SetDouble(string name, double data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform1(_uniformLocations[name], data);
            }
        }

        public void SetDouble(string name, double[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform1(_uniformLocations[string.Format("{0}[{1}]", name, i)], data[i]);
                }
            }
        }

        #endregion

        #region Vector2

        public void SetVector2(string name, Vector2 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform2(_uniformLocations[name], data);
            }
        }

        public void SetVector2(string name, Vector2[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform2(_uniformLocations[string.Format("{0}[{1}]", name, i)], data[i]);
                }
            }
        }

        #endregion

        #region Vector3

        public void SetVector3(string name, Vector3 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform3(_uniformLocations[name], data);
            }
        }

        public void SetVector3(string name, Vector3[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform3(_uniformLocations[string.Format("{0}[{1}]", name, i)], data[i]);
                }
            }
        }

        public void SetVector3(string name, Color4 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform3(_uniformLocations[name], new Vector3(data.R, data.G, data.B));
            }
        }

        public void SetVector3(string name, Color4[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform3(_uniformLocations[string.Format("{0}[{1}]", name, i)], new Vector3(data[i].R, data[i].G, data[i].B));
                }
            }
        }

        #endregion

        #region Vector4

        public void SetVector4(string name, Vector4 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform4(_uniformLocations[name], data);
            }
        }

        public void SetVector4(string name, Vector4[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform4(_uniformLocations[string.Format("{0}[{1}]", name, i)], data[i]);
                }
            }
        }

        public void SetVector4(string name, Quaternion data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform4(_uniformLocations[name], data);
            }
        }

        public void SetVector4(string name, Quaternion[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform4(_uniformLocations[string.Format("{0}[{1}]", name, i)], data[i]);
                }
            }
        }

        public void SetVector4(string name, Color4 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.Uniform4(_uniformLocations[name], data);
            }
        }

        public void SetVector4(string name, Color4[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.Uniform4(_uniformLocations[string.Format("{0}[{1}]", name, i)], data[i]);
                }
            }
        }

        #endregion

        #region Matrices

        public void SetMatrix(string name, Matrix4 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.UniformMatrix4(_uniformLocations[name], true, ref data);
            }
        }

        public void SetMatrix(string name, Matrix4[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.UniformMatrix4(_uniformLocations[string.Format("{0}[{1}]", name, i)], true, ref data[i]);
                }
            }
        }

        public void SetMatrix(string name, Matrix4x3 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                float[] mat4x3 = new float[]
                {
                    data.M11, data.M12, data.M13,
                    data.M21, data.M22, data.M23,
                    data.M31, data.M32, data.M33,
                    data.M41, data.M42, data.M43
                };

                GL.UseProgram(Handle);
                GL.UniformMatrix4x3(_uniformLocations[name], 48, true, mat4x3);
            }
        }

        public void SetMatrix(string name, Matrix4x3[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    float[] mat4x3 = new float[]
                    {
                        data[i].M11, data[i].M12, data[i].M13,
                        data[i].M21, data[i].M22, data[i].M23,
                        data[i].M31, data[i].M32, data[i].M33,
                        data[i].M41, data[i].M42, data[i].M43
                    };
                    GL.UniformMatrix4x3(_uniformLocations[string.Format("{0}[{1}]", name, i)], 48, true, mat4x3);
                }
            }
        }

        public void SetMatrix(string name, Matrix3x4 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                float[] mat3x4 = new float[]
                {
                    data.M11, data.M12, data.M13, data.M14,
                    data.M21, data.M22, data.M23, data.M24,
                    data.M31, data.M32, data.M33, data.M34
                };

                GL.UseProgram(Handle);
                GL.UniformMatrix3x4(_uniformLocations[name], 48, true, mat3x4);
            }
        }

        public void SetMatrix(string name, Matrix3x4[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    float[] mat3x4 = new float[]
                    {
                        data[i].M11, data[i].M12, data[i].M13, data[i].M14,
                        data[i].M21, data[i].M22, data[i].M23, data[i].M24,
                        data[i].M31, data[i].M32, data[i].M33, data[i].M34
                    };
                    GL.UniformMatrix4x3(_uniformLocations[string.Format("{0}[{1}]", name, i)], 48, true, mat3x4);
                }
            }
        }

        public void SetMatrix(string name, Matrix3 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.UniformMatrix3(_uniformLocations[name], true, ref data);
            }
        }

        public void SetMatrix(string name, Matrix3[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.UniformMatrix3(_uniformLocations[string.Format("{0}[{1}]", name, i)], true, ref data[i]);
                }
            }
        }

        public void SetMatrix(string name, Matrix2 data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);

            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                GL.UniformMatrix2(_uniformLocations[name], true, ref data);
            }
        }

        public void SetMatrix(string name, Matrix2[] data)
        {
            if (disposed)
                throw new ObjectDisposedException("Shader: " + name);
            if (_uniformLocations.ContainsKey(name))
            {
                GL.UseProgram(Handle);
                for (int i = 0; i < data.Length; i++)
                {
                    GL.UniformMatrix2(_uniformLocations[string.Format("{0}[{1}]", name, i)], true, ref data[i]);
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #region Fields

        private string name;

        private bool disposed;

        private readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public static List<Shader> shaders = new List<Shader>();

        #endregion
    }
}