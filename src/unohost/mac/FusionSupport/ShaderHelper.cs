using System;
using OpenTK.Graphics.OpenGL;

namespace Outracks.UnoHost.Mac.FusionSupport
{
	public static class ShaderHelper
	{
		public static int CompileShader(ShaderType type, string source)
		{
			int handle = GL.CreateShader(type);
			GL.ShaderSource(handle, source);
			GL.CompileShader(handle);

			int compileStatus;
			GL.GetShader(handle, ShaderParameter.CompileStatus, out compileStatus);
			if (compileStatus != 1)
			{
				string log = GL.GetShaderInfoLog(handle);
				throw new Exception((((("Error compiling shader (" + (object)type) + "):\n\n") + log) + "\n\nSource:\n\n") + source);
			}

			return handle;
		}

		public static int LinkProgram(int vertexShader, int fragmentShader)
		{
			int handle = GL.CreateProgram();
			GL.AttachShader(handle, vertexShader);
			GL.AttachShader(handle, fragmentShader);
			GL.LinkProgram(handle);

			int linkStatus;
			GL.GetProgram(handle, ProgramParameter.LinkStatus, out linkStatus);
			if (linkStatus != 1)
			{
				string log = GL.GetProgramInfoLog(handle);
				throw new Exception("Error linking shader program:\n\n" + log);
			}

			GL.UseProgram(handle);
			return handle;
		}
	}
}
