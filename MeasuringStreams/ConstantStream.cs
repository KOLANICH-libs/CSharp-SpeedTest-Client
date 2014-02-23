using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace MeasuringStreams
{
    public class ConstantStream : EventedStream
    {
        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }

        protected byte[] _pattern={0};
        protected uint patternPos;
        public byte[] pattern {
            set{
                _pattern = value;
                patternPos = 0;
            }
            get {
                return _pattern;
            }
        }

        public ConstantStream(long count) : base()
        {
			Contract.Requires(count >= 0);
			SetLength(count);
        }
		public ConstantStream(byte[] pattern) : this(pattern.Length)
		{
			Contract.Requires(pattern!=null);
			this.pattern = pattern;
		}
		public ConstantStream(long count, byte[] pattern) : base()
        {
			Contract.Requires(count >=0);
			Contract.Requires(pattern != null);
			SetLength(count);
			this.pattern = pattern;
		}

		private long len;


        public override void SetLength(long value)
        {
            len = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var newPos = Position + count;
            if (newPos >= Length)
            {
                newPos = Length;
                count = (int)(newPos - Position);
            }
            //Debug.WriteLine("{0}\t{1}\t{2}", Position, newPos, count);
            Position = newPos;
            for(uint i = 0; i < count; i++)
            {
                buffer[offset + i] = _pattern[(patternPos + i)% _pattern.Length];
            }
            patternPos = (uint)(patternPos + _pattern.Length) % (uint)_pattern.Length;
            return count;
        }

        public override int ReadByte()
        {
            Position++;
            patternPos = (uint)(patternPos + 1) % (uint)_pattern.Length;
            return _pattern[patternPos];
        }

        public override long Length
        {
            get { return len; }
        }
    }
}
