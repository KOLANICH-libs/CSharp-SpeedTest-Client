using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace MeasuringStreams
{
	public class RandomBytesStream : EventedStream
	{
		public override void Flush() {}

		public override long Seek(long offset, SeekOrigin origin) {throw new NotImplementedException();}
		public override void Write(byte[] buffer, int offset, int count){throw new NotImplementedException();}

		public override bool CanRead {get { return true; }}
		public override bool CanSeek {get { return false; }}
		public override bool CanWrite {get { return false; }}



		public RandomBytesStream(long count): base() {
            SetLength(count);
		}
		protected Random rng = new Random();
		private long len;


		public override void SetLength(long value){
			len = value;
		}

		public override int Read(byte[] buffer, int offset, int count){
			var newPos = Position + count;
			if (newPos >= Length) {
				newPos = Length;
				count = (int) (newPos - Position);
			}
			//Debug.WriteLine("{0}\t{1}\t{2}", Position, newPos, count);
			Position=newPos;
			var a=new ArraySegment<byte>(buffer, offset, count);
			rng.NextBytes(a.Array);
			return count;
		}

		public override int ReadByte() {
			Position++;
			return rng.Next();
		}

		public override long Length {
			get { return len; }
		}
	}
	public class RangedRandomBytesStream : RandomBytesStream
	{
		protected byte lower, upper;
		public RangedRandomBytesStream(long count, byte lowerBound, byte upperBound):base(count) {
			lower = lowerBound;
			upper = upperBound;
		}

		public override int Read(byte[] buffer, int offset, int count) {
			Contract.Requires(buffer != null);
			var newPos = Position + count;
			if (newPos >= Length) {
				newPos = Length;
				count = (int)(newPos - Position);
			}
			Position = newPos;
			for (int i = offset,end=offset+count; i < end; i++)
				buffer[i] = (byte)(ReadByte());
			return count;
		}

		public override int ReadByte() {
			Position++;
			return rng.Next(lower,upper);
		}
	}
}
