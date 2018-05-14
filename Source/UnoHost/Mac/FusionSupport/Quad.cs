using System;
using OpenTK.Graphics.OpenGL;

namespace Outracks.UnoHost.OSX.FusionSupport
{
	public class Quad
	{
		const string vertSource =
			@"
uniform vec2 position;
uniform vec2 size;
uniform vec4 clipRect;

attribute vec2 vertex;
varying vec2 texCoord;
void main()
{
	vec2 pos = position + vertex * size;
    texCoord = clipRect.xy + vec2(vertex.x, vertex.y) * clipRect.zw;
    gl_Position = vec4(pos,0,1);
}
";

		const string fragSource =
			@"
uniform sampler2DRect tex;
uniform int hasTexture;
uniform vec2 texSize;
varying vec2 texCoord;
void main()
{
    if(hasTexture == 1)
      gl_FragColor = texture2DRect(tex, vec2(texCoord.x, texCoord.y) * texSize);
    else
      gl_FragColor = vec4(texCoord.xy*.5 + .5, 0.3, 1);
}
";

		byte []vertexData = new byte[]
		{
			0, 0,
			1, 0,
			1, 1,
			1, 1,
			0, 1,
			0, 0,
		};

		int _vbo;
		int _program;

		int _textureLoc;
		int _positionLoc;
		int _sizeLoc;
		int _clipLoc;

		int _vertexLoc;

		bool _inited = false;

		public Quad()
		{
		}

		public void Render(TextureTarget target, TextureHandle texture, Size<Pixels> textureSize, Rectangle<ClipSpaceUnits> rectangle, Rectangle<ClipSpaceUnits> clip)
		{
			Init ();

			GL.UseProgram (_program);
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture (target, texture);
			GL.Uniform1 (_textureLoc, 0);
			GL.Uniform1 (GL.GetUniformLocation(_program, "hasTexture"), 1);
			GL.Uniform2 (GL.GetUniformLocation(_program, "texSize"), (float)textureSize.Width, (float)textureSize.Height);

			GL.Uniform2 (_positionLoc, rectangle.Position.X, rectangle.Position.Y);
			GL.Uniform2 (_sizeLoc, rectangle.Size.Width, rectangle.Size.Height);
			GL.Uniform4(_clipLoc, (float)clip.Position.X, (float)clip.Position.Y, (float)clip.Size.Width, (float)clip.Size.Height);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
			GL.VertexAttribPointer (_vertexLoc, 2, VertexAttribPointerType.Byte, false, 0, 0);
			GL.EnableVertexAttribArray (_vertexLoc);

			GL.DrawArrays(BeginMode.Triangles, 0, 6);
		}

		public void Render(Rectangle<ClipSpaceUnits> rectangle)
		{
			Init ();

			GL.UseProgram (_program);
			GL.Uniform1 (GL.GetUniformLocation(_program, "hasTexture"), 0);

			GL.Uniform2 (_positionLoc, rectangle.Position.X, rectangle.Position.Y);
			GL.Uniform2 (_sizeLoc, rectangle.Size.Width, rectangle.Size.Height);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
			GL.VertexAttribPointer (_vertexLoc, 2, VertexAttribPointerType.Byte, false, 0, 0);
			GL.EnableVertexAttribArray (_vertexLoc);

			GL.UseProgram(_program);
			GL.DrawArrays(BeginMode.Triangles, 0, 6);
		}

		void Init()
		{
			if (_inited)
				return;

			GL.GenBuffers(1, out _vbo);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertexData.Length), vertexData, BufferUsageHint.StaticDraw);

			var vertShader = ShaderHelper.CompileShader(ShaderType.VertexShader, vertSource);
			var fragShader = ShaderHelper.CompileShader(ShaderType.FragmentShader, fragSource);
			_program = ShaderHelper.LinkProgram(vertShader, fragShader);

			_textureLoc = GL.GetUniformLocation (_program, "tex");
			_positionLoc = GL.GetUniformLocation (_program, "position");
			_sizeLoc = GL.GetUniformLocation (_program, "size");
			_clipLoc = GL.GetUniformLocation(_program, "clipRect");
			_vertexLoc = GL.GetAttribLocation (_program, "vertex");
			_inited = true;
		}
	}
}