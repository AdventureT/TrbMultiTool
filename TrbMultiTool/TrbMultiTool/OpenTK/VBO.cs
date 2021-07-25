using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static TrbMultiTool.TmdlWindow;

namespace SEViewer.Core
{
    /// ------------------------------------------------------------------------------------------------
    /// Original Code: https://github.com/giawa/opengl4csharp/blob/dotnetcore/OpenGL/Constructs/VBO.cs -
    /// ------------------------------------------------------------------------------------------------
    public class VBO<T> : IDisposable
        where T : struct
    {
        #region Constructor, Destructor

        public VBO([In, Out] T[] data, BufferTarget target = BufferTarget.ArrayBuffer, BufferUsageHint hint = BufferUsageHint.StaticDraw)
        {
            this.Handle = GL.GenBuffer();
            if(Handle != 0)
            {
                GL.BindBuffer(target, Handle);
                GL.BufferData<T>(target, (data.Length * Marshal.SizeOf<T>()), data, hint);
            }

            this.BufferTarget = target;
            this.Size = TypeComponentSize[typeof(T)];
            this.PointerType = TypeAttribPointerType[typeof(T)];
            this.IPointerType = TypeAttribIPointerType[typeof(T)];
            this.IPointer = TypeIsAttribIPointer[typeof(T)];
            this.Count = data.Length;
        }

        #endregion

        #region Methods

        #region Binding

        public void BindToShaderAttribute(Shader shader, string attributeName)
        {
            int location = shader.GetAttribLocation(attributeName);

            GL.EnableVertexAttribArray(location);
            GL.BindBuffer(BufferTarget, Handle);
            if(this.IPointer)
                GL.VertexAttribIPointer(location, Size, IPointerType, Marshal.SizeOf<T>(), IntPtr.Zero);
            else
                GL.VertexAttribPointer(location, Size, PointerType, true, Marshal.SizeOf<T>(), IntPtr.Zero);
        }

        #endregion

        #region Disposing

        public void Dispose()
        {
            if (!this.disposed)
            {
                if (Handle != 0)
                    GL.DeleteBuffer(Handle);

                this.disposed = true;
            }
        }

        #endregion

        #endregion

        #region Properties, Fields

        #region Public

        public int Handle { get; private set; }

        public int Count { get; private set; }

        public int Size { get; private set; }

        public bool IPointer { get; private set; }

        public VertexAttribPointerType PointerType { get; private set; }

        public VertexAttribIntegerType IPointerType { get; private set; }

        public BufferTarget BufferTarget { get; private set; }

        #endregion

        #region Private

        private static readonly Dictionary<Type, int> TypeComponentSize = new Dictionary<Type, int>()
        {
            [typeof(sbyte)] = 1,
            [typeof(byte)] = 1,
            [typeof(short)] = 1,
            [typeof(ushort)] = 1,
            [typeof(int)] = 1,
            [typeof(uint)] = 1,
            [typeof(float)] = 1,
            [typeof(double)] = 1,
            [typeof(Vector2)] = 2,
            [typeof(Vector3)] = 3,
            [typeof(Vector4)] = 4,
            [typeof(Color4)] = 4,
        };

        private static readonly Dictionary<Type, VertexAttribPointerType> TypeAttribPointerType = new Dictionary<Type, VertexAttribPointerType>()
        {
            [typeof(sbyte)] = VertexAttribPointerType.Byte,
            [typeof(byte)] = VertexAttribPointerType.UnsignedByte,
            [typeof(short)] = VertexAttribPointerType.Short,
            [typeof(ushort)] = VertexAttribPointerType.UnsignedShort,
            [typeof(int)] = VertexAttribPointerType.Int,
            [typeof(uint)] = VertexAttribPointerType.UnsignedInt,
            [typeof(float)] = VertexAttribPointerType.Float,
            [typeof(double)] = VertexAttribPointerType.Double,
            [typeof(Vector2)] = VertexAttribPointerType.Float,
            [typeof(Vector3)] = VertexAttribPointerType.Float,
            [typeof(Vector4)] = VertexAttribPointerType.Float,
            [typeof(Color4)] = VertexAttribPointerType.Float
        };

        private static readonly Dictionary<Type, bool> TypeIsAttribIPointer = new Dictionary<Type, bool>()
        {
            [typeof(sbyte)] = true,
            [typeof(byte)] = true,
            [typeof(short)] = true,
            [typeof(ushort)] = true,
            [typeof(int)] = true,
            [typeof(uint)] = true,
            [typeof(float)] = false,
            [typeof(double)] = false,
            [typeof(Vector2)] = false,
            [typeof(Vector3)] = false,
            [typeof(Vector4)] = false,
        };

        private static readonly Dictionary<Type, VertexAttribIntegerType> TypeAttribIPointerType = new Dictionary<Type, VertexAttribIntegerType>()
        {
            [typeof(sbyte)] = VertexAttribIntegerType.Byte,
            [typeof(byte)] = VertexAttribIntegerType.UnsignedByte,
            [typeof(short)] = VertexAttribIntegerType.Short,
            [typeof(ushort)] = VertexAttribIntegerType.UnsignedShort,
            [typeof(int)] = VertexAttribIntegerType.Int,
            [typeof(uint)] = VertexAttribIntegerType.UnsignedInt,
            [typeof(float)] = VertexAttribIntegerType.Int,
            [typeof(double)] = VertexAttribIntegerType.Int,
            [typeof(Vector2)] = VertexAttribIntegerType.Int,
            [typeof(Vector3)] = VertexAttribIntegerType.Int,
            [typeof(Vector4)] = VertexAttribIntegerType.Int,
            [typeof(Color4)] = VertexAttribIntegerType.Int
        };

        private bool disposed;

        #endregion

        #endregion
    }
}
