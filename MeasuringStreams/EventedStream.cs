using System;
using System.IO;

namespace MeasuringStreams
{
    public abstract class EventedStream : Stream
    {
        public delegate void dataLoadListener(object obj, long bytesLoaded);
        public event dataLoadListener onLoading;
        protected long _position;
        public override long Position
        {
            get { return _position; }
            set
            {
                _position = value;
                onLoading(this, _position);
            }
        }

    }
}
