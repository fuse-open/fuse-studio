using System;
using System.Collections.Generic;
using OpenGL;
using OpenTK.Graphics.ES20;
using Uno;
using Uno.Diagnostics;
using GL = OpenTK.Graphics.ES20.GL;

namespace Outracks.UnoHost.Windows
{
	partial class OpenTKGL : IGL
	{
		// Note: Workaround for InvalidProgramException
		GLFramebufferHandle _currentFramebufferBinding;
		GLRenderbufferHandle _currentRenderbufferBinding;

		readonly LinkedList<TextureDisposable> _textures = new LinkedList<TextureDisposable>();
		readonly LinkedList<FramebufferDisposable> _framebuffers = new LinkedList<FramebufferDisposable>();
		readonly LinkedList<BufferDisposable> _buffers = new LinkedList<BufferDisposable>();
		readonly LinkedList<RenderbufferDisposable> _renderbuffers = new LinkedList<RenderbufferDisposable>();
		readonly LinkedList<ShaderDisposable> _shaders = new LinkedList<ShaderDisposable>();
		readonly LinkedList<ProgramDisposable> _programs = new LinkedList<ProgramDisposable>();

		public GLError GetError()
		{
			return (GLError)GL.GetError();
		}

		public void Finish()
		{
			GL.Finish();
		}

		public void Flush()
		{
			GL.Flush();
		}

		public int GetInteger(GLIntegerName pname)
		{
			return GL.GetInteger((GetPName)pname);
		}

		public Int4 GetInteger(GLInteger4Name name)
		{
			var v = new int[4];
			GL.GetInteger((GetPName)name, v);
			return new Int4(v[0], v[1], v[2], v[3]);
		}

		public GLFramebufferStatus CheckFramebufferStatus(GLFramebufferTarget target)
		{
			return (GLFramebufferStatus)GL.CheckFramebufferStatus((FramebufferTarget)target);
		}

		public void Clear(GLClearBufferMask mask)
		{
			GL.Clear((ClearBufferMask)mask);
		}

		public void ClearColor(float red, float green, float blue, float alpha)
		{
			GL.ClearColor(red, green, blue, alpha);
		}

		public void ClearDepth(float depth)
		{
			GL.ClearDepth(depth);
		}

		public void ColorMask(bool red, bool green, bool blue, bool alpha)
		{
			GL.ColorMask(red, green, blue, alpha);
		}

		public void DepthMask(bool flag)
		{
			GL.DepthMask(flag);
		}

		public GLTextureHandle CreateTexture()
		{
			int texture = GL.GenTexture();
			AddContextObject(new TextureDisposable(texture));

			return (GLTextureHandle) texture;
		}

		public void DeleteTexture(GLTextureHandle texture)
		{
			DisposeAndRemoveObject(_textures, (int) texture);
		}

		public void ActiveTexture(GLTextureUnit texture)
		{
			GL.ActiveTexture((TextureUnit)texture);
		}

		public void BindTexture(GLTextureTarget target, GLTextureHandle texture)
		{
			GL.BindTexture((TextureTarget)target, (int) texture);
		}

		public void TexParameter(GLTextureTarget target, GLTextureParameterName pname, GLTextureParameterValue param)
		{
			GL.TexParameter((TextureTarget)target, (TextureParameterName)pname, (int)param);
		}

		public void TexImage2D(GLTextureTarget target, int level, GLPixelFormat internalFormat, int width, int height, int border, GLPixelFormat format, GLPixelType type, IntPtr data)
		{
			GL.TexImage2D((TextureTarget2d)target, level,
					(TextureComponentCount)internalFormat, width, height, border,
					(PixelFormat)format, (PixelType)type,
					data);
		}

		public void TexSubImage2D(GLTextureTarget target, int level, int xoffset, int yoffset, int width, int height, GLPixelFormat format, GLPixelType type, IntPtr data)
		{
			GL.TexSubImage2D((TextureTarget2d)target, level,
				xoffset, yoffset, width, height,
				(PixelFormat)format, (PixelType)type,
				data);
		}

		public void GenerateMipmap(GLTextureTarget target)
		{
			GL.GenerateMipmap((TextureTarget)target);
		}

		public void PixelStore(GLPixelStoreParameter pname, int param)
		{
			GL.PixelStore((PixelStoreParameter)pname, param);
		}

		public GLRenderbufferHandle CreateRenderbuffer()
		{
			int r;
			GL.GenRenderbuffers(1, out r);
			AddContextObject(new RenderbufferDisposable(r));
			return (GLRenderbufferHandle) r;
		}

		public void DeleteRenderbuffer(GLRenderbufferHandle renderbuffer)
		{
			DisposeAndRemoveObject(_renderbuffers, (int) renderbuffer);
		}

		public void BindRenderbuffer(GLRenderbufferTarget target, GLRenderbufferHandle renderbuffer)
		{
			_currentRenderbufferBinding = renderbuffer;
			GL.BindRenderbuffer((RenderbufferTarget)target, (int) renderbuffer);
		}

		public void RenderbufferStorage(GLRenderbufferTarget target, GLRenderbufferStorage internalFormat, int width, int height)
		{
			GL.RenderbufferStorage((RenderbufferTarget)target, (RenderbufferInternalFormat)internalFormat, width, height);
		}

		public GLFramebufferHandle CreateFramebuffer()
		{
			int r;
			GL.GenFramebuffers(1, out r);
			AddContextObject(new FramebufferDisposable(r));
			return (GLFramebufferHandle) r;
		}

		public void DeleteFramebuffer(GLFramebufferHandle fb)
		{
			DisposeAndRemoveObject(_framebuffers, (int) fb);
		}

		public void BindFramebuffer(GLFramebufferTarget target, GLFramebufferHandle fb)
		{
			//if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
			//	System.Diagnostics.Debug.WriteLine(AppDomain.CurrentDomain.FriendlyName + " bound " + fb);
			_currentFramebufferBinding = fb;
			GL.BindFramebuffer((FramebufferTarget)target, (int) fb);
		}

		public void FramebufferTexture2D(GLFramebufferTarget target, GLFramebufferAttachment attachment, GLTextureTarget textarget, GLTextureHandle texture, int level)
		{
			GL.FramebufferTexture2D((FramebufferTarget)target, (FramebufferAttachment)attachment, (TextureTarget2d)textarget, (int)texture, level);
		}

		public void FramebufferRenderbuffer(GLFramebufferTarget target, GLFramebufferAttachment attachment, GLRenderbufferTarget renderbuffertarget, GLRenderbufferHandle renderbuffer)
		{
			GL.FramebufferRenderbuffer((FramebufferTarget)target, (FramebufferAttachment)attachment, (RenderbufferTarget)renderbuffertarget, (int)renderbuffer);
		}

		public void UseProgram(GLProgramHandle program)
		{
			GL.UseProgram((int) program);
		}

		public void BindAttribLocation(GLProgramHandle handle, int index, string name)
		{
			GL.BindAttribLocation((int) handle, index, name);
		}

		public int GetAttribLocation(GLProgramHandle program, string name)
		{
			return GL.GetAttribLocation((int) program, name);
		}

		public int GetUniformLocation(GLProgramHandle program, string name)
		{
			return GL.GetUniformLocation((int) program, name);
		}

		public void Uniform1(int location, int value)
		{
			GL.Uniform1(location, value);
		}

		public void Uniform2(int location, Int2 value)
		{
			GL.Uniform2(location, value.X, value.Y);
		}

		public void Uniform3(int location, Int3 value)
		{
			GL.Uniform3(location, value.X, value.Y, value.Z);
		}

		public void Uniform4(int location, Int4 value)
		{
			GL.Uniform4(location, value.X, value.Y, value.Z, value.W);
		}

		public void Uniform1(int location, float value)
		{
			GL.Uniform1(location, value);
		}

		public void Uniform2(int location, Float2 value)
		{
			GL.Uniform2(location, value.X, value.Y);
		}

		public void Uniform3(int location, Float3 value)
		{
			GL.Uniform3(location, value.X, value.Y, value.Z);
		}

		public void Uniform4(int location, Float4 value)
		{
			GL.Uniform4(location, value.X, value.Y, value.Z, value.W);
		}

		public void UniformMatrix2(int location, bool transpose, Float2x2 value)
		{
			GL.UniformMatrix2(location, 1, transpose, ref value.M11);
		}

		public void UniformMatrix3(int location, bool transpose, Float3x3 value)
		{
			GL.UniformMatrix3(location, 1, transpose, ref value.M11);
		}

		public void UniformMatrix4(int location, bool transpose, Float4x4 value)
		{
			// Who is always changing this function creating a temp array? Please stop committing your workarounds. Thanks
			GL.UniformMatrix4(location, 1, transpose, ref value.M11);
		}

		public void Uniform1(int location, int[] value)
		{
			GL.Uniform1(location, value.Length, value);
		}

		public void Uniform2(int location, Int2[] value)
		{
			if (value.Length > 0)
			{
				unsafe
				{
					fixed (int* p = &value[0].X)
						GL.Uniform2(location, value.Length, p);
				}
			}
		}

		public void Uniform3(int location, Int3[] value)
		{
			if (value.Length > 0)
			{
				unsafe
				{
					fixed (int* p = &value[0].X)
						GL.Uniform3(location, value.Length, p);
				}
			}
		}

		public void Uniform4(int location, Int4[] value)
		{
			if (value.Length > 0)
			{
				unsafe
				{
					fixed (int* p = &value[0].X)
						GL.Uniform4(location, value.Length, p);
				}
			}
		}

		public void Uniform1(int location, float[] value)
		{
			GL.Uniform1(location, value.Length, value);
		}

		public void Uniform2(int location, Float2[] value)
		{
			if (value.Length > 0)
			{
				unsafe
				{
					fixed (float* p = &value[0].X)
						GL.Uniform2(location, value.Length, p);
				}
			}
		}

		public void Uniform3(int location, Float3[] value)
		{
			if (value.Length > 0)
			{
				unsafe
				{
					fixed (float* p = &value[0].X)
						GL.Uniform3(location, value.Length, p);
				}
			}
		}

		public void Uniform4(int location, Float4[] value)
		{
			if (value.Length > 0)
			{
				unsafe
				{
					fixed (float* p = &value[0].X)
						GL.Uniform4(location, value.Length, p);
				}
			}
		}

		public void UniformMatrix2(int location, bool transpose, Float2x2[] value)
		{
			if (value.Length > 0)
				GL.UniformMatrix2(location, value.Length, transpose, ref value[0].M11);
		}

		public void UniformMatrix3(int location, bool transpose, Float3x3[] value)
		{
			if (value.Length > 0)
				GL.UniformMatrix3(location, value.Length, transpose, ref value[0].M11);
		}

		public void UniformMatrix4(int location, bool transpose, Float4x4[] value)
		{
			if (value.Length > 0)
				GL.UniformMatrix4(location, value.Length, transpose, ref value[0].M11);
		}

		public void EnableVertexAttribArray(int index)
		{
			GL.EnableVertexAttribArray(index);
		}

		public void DisableVertexAttribArray(int index)
		{
			GL.DisableVertexAttribArray(index);
		}

		public void VertexAttribPointer(int index, int size, GLDataType type, bool normalized, int stride, int offset)
		{
			GL.VertexAttribPointer(index, size, (VertexAttribPointerType)type, normalized, stride, offset);
		}

		public void DrawArrays(GLPrimitiveType mode, int first, int count)
		{
			GL.DrawArrays((PrimitiveType)mode, first, count);
		}

		public void DrawElements(GLPrimitiveType mode, int count, GLIndexType type, int offset)
		{
			GL.DrawElements((BeginMode)mode, count, (DrawElementsType)type, offset);
		}

		public GLBufferHandle CreateBuffer()
		{
			int r;
			GL.GenBuffers(1, out r);
			AddContextObject(new BufferDisposable(r));
			return (GLBufferHandle) r;
		}

		public void DeleteBuffer(GLBufferHandle buffer)
		{
			DisposeAndRemoveObject(_buffers, (int) buffer);
		}

		public void BindBuffer(GLBufferTarget target, GLBufferHandle buffer)
		{
			GL.BindBuffer((BufferTarget)target, (int) buffer);
		}

		public void BufferData(GLBufferTarget target, int sizeInBytes, IntPtr data, GLBufferUsage usage)
		{
			GL.BufferData((BufferTarget)target, (IntPtr)sizeInBytes, data, (BufferUsageHint)usage);
		}

		public void BufferSubData(GLBufferTarget target, int offset, int sizeInBytes, IntPtr data)
		{
			GL.BufferSubData((BufferTarget)target, (IntPtr)offset, (IntPtr)sizeInBytes, data);
		}

		public void Enable(GLEnableCap cap)
		{
			GL.Enable((EnableCap)cap);
		}

		public void Disable(GLEnableCap cap)
		{
			GL.Disable((EnableCap)cap);
		}

		public bool IsEnabled(GLEnableCap cap)
		{
			return GL.IsEnabled((EnableCap)cap);
		}

		public void BlendFunc(GLBlendingFactor src, GLBlendingFactor dst)
		{
			GL.BlendFunc((BlendingFactorSrc)src, (BlendingFactorDest)dst);
		}

		public void BlendFuncSeparate(GLBlendingFactor srcRGB, GLBlendingFactor dstRGB, GLBlendingFactor srcAlpha, GLBlendingFactor dstAlpha)
		{
			GL.BlendFuncSeparate((BlendingFactorSrc)srcRGB, (BlendingFactorDest)dstRGB,
				(BlendingFactorSrc)srcAlpha, (BlendingFactorDest)dstAlpha);
		}

		public void BlendEquation(GLBlendEquation mode)
		{
			GL.BlendEquation((BlendEquationMode)mode);
		}

		public void BlendEquationSeparate(GLBlendEquation modeRgb, GLBlendEquation modeAlpha)
		{
			GL.BlendEquationSeparate((BlendEquationMode)modeRgb, (BlendEquationMode)modeAlpha);
		}

		public void CullFace(GLCullFaceMode mode)
		{
			GL.CullFace((CullFaceMode)mode);
		}

		public void FrontFace(GLFrontFaceDirection mode)
		{
			GL.FrontFace((FrontFaceDirection)mode);
		}

		public void DepthFunc(GLDepthFunction func)
		{
			GL.DepthFunc((DepthFunction)func);
		}

		public void Scissor(int x, int y, int width, int height)
		{
			GL.Scissor(x, y, width, height);
		}

		public void Viewport(int x, int y, int width, int height)
		{
			GL.Viewport(x, y, width, height);
		}

		public void LineWidth(float width)
		{
			GL.LineWidth(width);
		}

		public void PolygonOffset(float factor, float units)
		{
			GL.PolygonOffset(factor, units);
		}

		public void DepthRange(float zNear, float zFar)
		{
			GL.DepthRange(zNear, zFar);
		}

		public GLShaderHandle CreateShader(GLShaderType type)
		{
			int shaderName = GL.CreateShader((ShaderType)type);
			AddContextObject(new ShaderDisposable(shaderName));
			return (GLShaderHandle) shaderName;
		}

		public void DeleteShader(GLShaderHandle shader)
		{
			DisposeAndRemoveObject(_shaders, (int) shader);
		}

		public void ShaderSource(GLShaderHandle shader, string source)
		{
			GL.ShaderSource((int) shader, source);
		}

		public void ReadPixels(int x, int y, int width, int height, GLPixelFormat format, GLPixelType type, byte[] buffer)
		{
			GL.ReadPixels(x, y, width, height, (PixelFormat)format, (PixelType)type, buffer);
		}

		public void CompileShader(GLShaderHandle shader)
		{
			GL.CompileShader((int) shader);
		}

		public int GetShaderParameter(GLShaderHandle shader, GLShaderParameter pname)
		{
			int result;
			GL.GetShader((int) shader, (ShaderParameter)pname, out result);
			return result;
		}

		public string GetShaderInfoLog(GLShaderHandle shader)
		{
			return GL.GetShaderInfoLog((int) shader);
		}

		public bool HasGetShaderPrecisionFormat { get { return false; }}

		public GLShaderPrecisionFormat GetShaderPrecisionFormat(GLShaderType shader, GLShaderPrecision precision)
		{
			throw new NotImplementedException();
		}

		public GLProgramHandle CreateProgram()
		{
			int programName = GL.CreateProgram();
			AddContextObject(new ProgramDisposable(programName));

			return (GLProgramHandle) programName;
		}

		public void DeleteProgram(GLProgramHandle program)
		{
			DisposeAndRemoveObject(_programs, (int) program);
		}

		public void AttachShader(GLProgramHandle program, GLShaderHandle shader)
		{
			GL.AttachShader((int) program, (int) shader);
		}

		public void DetachShader(GLProgramHandle program, GLShaderHandle shader)
		{
			GL.DetachShader((int) program, (int) shader);
		}

		public void LinkProgram(GLProgramHandle program)
		{
			GL.LinkProgram((int) program);
		}

		public int GetProgramParameter(GLProgramHandle program, GLProgramParameter pname)
		{
			int result;
			GL.GetProgram((int) program, (GetProgramParameterName)pname, out result);
			return result;
		}

		public string GetProgramInfoLog(GLProgramHandle program)
		{
			return GL.GetProgramInfoLog((int) program);
		}


		public string GetString(GLStringName name)
		{
			var value = GL.GetString((StringName)name);

			if (name == GLStringName.Version && value.Contains("ANGLE"))
				return "2.1";

			return value;
		}

		public GLRenderbufferHandle GetRenderbufferBinding()
		{
			return _currentRenderbufferBinding;
			//return OpenTK.Graphics.OpenGL.OpenTKWrapper.GetInteger(OpenTK.Graphics.OpenGL.GetPName.RenderbufferBinding);
		}

		public GLFramebufferHandle GetFramebufferBinding()
		{
			return _currentFramebufferBinding;
			//return OpenTK.Graphics.OpenGL.OpenTKWrapper.GetInteger(OpenTK.Graphics.OpenGL.GetPName.FramebufferBinding);
		}
	}
}
