using System;
using Uno;
using Uno.IO;
using Uno.Collections;
using Uno.Diagnostics;

namespace Outracks.Simulator.Runtime
{
	public class ArrayStream : Uno.IO.Stream
	{
		private byte[] _buffer = new byte[0];
		private int _nextIncrease = 256;
		private long _length;


		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanTimeout
		{
			get { return false; }
		}

		public override long Length
		{
			get
			{
				return _length;
			}
		}

		public int Capacity
		{
			get
			{
				return _buffer.Length;
			}
		}

		public override long Position { get; set; }

		public ArrayStream(byte[] buffer)
		{
			_buffer = buffer;
			_length = _buffer.Length;
		}

		public override void Write(byte[] src, int byteOffset, int byteCount)
		{
			EnsureCapacity(byteCount);
			for (int i = byteOffset; i < byteOffset + byteCount; i++)
			{
				_buffer[(int)Position] = src[i];
				Position += 1;
			}
			if (Position > Length)
			{
				_length = Position;
			}
		}

		public override int Read(byte[] dst, int byteOffset, int byteCount)
		{
			int i = 0;
			for (; i < byteCount && Position + i < Length; i++)
			{
				dst[i + byteOffset] = _buffer[(int)Position + i];
			}
			Position += i;
			return i;
		}

		public override long Seek(long byteOffset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					Position = byteOffset;
					break;
				case SeekOrigin.End:
					Position = Length + byteOffset;
					break;
				default:
					Position = Position + byteOffset;
					break;
			}
			return Position;
		}

		private void EnsureCapacity(int byteCount)
		{
			if (Position + byteCount <= Capacity)
			{
				return;
			}
			else if (Position + byteCount <= Capacity + _nextIncrease)
			{
				ResizeTo(Capacity + _nextIncrease);
			}
			else
			{
				ResizeTo((int)Position + byteCount);
			}
		}

		private void ResizeTo(int newSize)
		{
			var newBuffer = new byte[newSize];
			Array.Copy(_buffer, newBuffer, _buffer.Length);
			_buffer = newBuffer;
			_nextIncrease = Capacity;
		}

		public virtual byte[] GetBuffer()
		{
			return _buffer;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Flush()
		{
		}
	}
}
