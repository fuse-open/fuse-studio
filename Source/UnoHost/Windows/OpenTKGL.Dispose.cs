using System;
using System.Collections.Generic;
using System.Linq;

namespace Outracks.UnoHost.Windows
{
	partial class OpenTKGL
	{
		void AddContextObject(TextureDisposable obj) { _textures.AddLast(obj); }
		void AddContextObject(FramebufferDisposable obj) { _framebuffers.AddLast(obj); }
		void AddContextObject(BufferDisposable obj) { _buffers.AddLast(obj); }
		void AddContextObject(RenderbufferDisposable obj) { _renderbuffers.AddLast(obj); }
		void AddContextObject(ShaderDisposable obj) { _shaders.AddLast(obj); }
		void AddContextObject(ProgramDisposable obj) { _programs.AddLast(obj); }

		public void DisposeContext()
		{
			DisposeAndRemoveObjects(_textures);
			DisposeAndRemoveObjects(_framebuffers);
			DisposeAndRemoveObjects(_buffers);
			DisposeAndRemoveObjects(_renderbuffers);
			DisposeAndRemoveObjects(_shaders);
			DisposeAndRemoveObjects(_programs);
		}


		void DisposeAndRemoveObject<T>(LinkedList<T> objects, int handle) where T : IContextObjectDisposable
		{
			var obj = objects.FirstOrDefault(o => o.HandleName == handle);
			if (obj == null) throw new InvalidOperationException("Trying to dispose object not created by this instance of IGL");

			obj.Dispose();
			objects.Remove(obj);
		}

		void DisposeAndRemoveObjects<T>(LinkedList<T> objects) where T : IContextObjectDisposable
		{
			foreach (var obj in objects) obj.Dispose();
			objects.Clear();
		}


		interface IContextObjectDisposable
		{
			int HandleName { get; }

			void Dispose();
		}

		class TextureDisposable : IContextObjectDisposable
		{
			public int HandleName { get; private set; }

			public TextureDisposable(int handleName)
			{
				HandleName = handleName;
			}

			public void Dispose()
			{
				OpenTK.Graphics.OpenGL.GL.DeleteTexture(HandleName);
			}
		}

		class FramebufferDisposable : IContextObjectDisposable
		{
			public int HandleName { get; private set; }

			public FramebufferDisposable(int handleName)
			{
				HandleName = handleName;
			}

			public void Dispose()
			{
				OpenTK.Graphics.OpenGL.GL.DeleteFramebuffer(HandleName);
			}
		}

		class BufferDisposable : IContextObjectDisposable
		{
			public int HandleName { get; private set; }

			public BufferDisposable(int handleName)
			{
				HandleName = handleName;
			}

			public void Dispose()
			{
				OpenTK.Graphics.OpenGL.GL.DeleteBuffer(HandleName);
			}
		}

		class RenderbufferDisposable : IContextObjectDisposable
		{
			public int HandleName { get; private set; }

			public RenderbufferDisposable(int handleName)
			{
				HandleName = handleName;
			}

			public void Dispose()
			{
				OpenTK.Graphics.OpenGL.GL.DeleteRenderbuffer(HandleName);
			}
		}

		class ShaderDisposable : IContextObjectDisposable
		{
			public int HandleName { get; private set; }

			public ShaderDisposable(int handleName)
			{
				HandleName = handleName;
			}

			public void Dispose()
			{
				OpenTK.Graphics.OpenGL.GL.DeleteShader(HandleName);
			}
		}

		class ProgramDisposable : IContextObjectDisposable
		{
			public int HandleName { get; private set; }

			public ProgramDisposable(int handleName)
			{
				HandleName = handleName;
			}

			public void Dispose()
			{
				OpenTK.Graphics.OpenGL.GL.DeleteProgram(HandleName);
			}
		}
	}
}