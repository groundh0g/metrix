using System.IO;

namespace MetrixWeb
{
    public class CountingStream : Stream
    {
        private Stream Stream { get; set; }

        public long Count { get; set; }

        public CountingStream(Stream stream)
        {
            this.Stream = stream;
        }

        public override void Flush()
        {
            this.Stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.Stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.Stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.Count += count;
            this.Stream.Write(buffer, offset, count);
            this.Stream.Flush();
        }

        public override bool CanRead { get { return this.Stream.CanRead; } }
        public override bool CanSeek { get { return this.Stream.CanSeek; } }
        public override bool CanWrite { get { return this.Stream.CanWrite; } }
        public override long Length { get { return this.Stream.Length; } }

        public override long Position
        {
            get { return this.Stream.Position; } 
            set { this.Stream.Position = value; }
        }
    }
}