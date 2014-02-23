using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace MeasuringStreams
{
    public class DevNullStream : EventedStream
    {
        public override void Flush() { }
        public override void SetLength(long value) { }

        public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
        public override int Read(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }

        public override bool CanRead { get { return false; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }

        public override void Write(byte[] buffer, int offset, int count)
        {
			Contract.Requires(buffer != null);
			Position += count;
        }
        public override long Length
        {
            get { return Position; }
        }
    }
}
