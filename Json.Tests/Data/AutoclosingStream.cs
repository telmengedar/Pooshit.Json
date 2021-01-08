using System.IO;

namespace Json.Tests.Data {
    public class AutoclosingStream : Stream {
        readonly Stream basestream;

        public AutoclosingStream(Stream basestream) {
            this.basestream = basestream;
        }

        public override void Flush() {
            basestream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int result = basestream.Read(buffer, offset, count);
            if (basestream.Position >= basestream.Length)
                basestream.Dispose();
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return basestream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            basestream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            basestream.Write(buffer, offset, count);
        }

        public override bool CanRead => basestream.CanRead;
        public override bool CanSeek => basestream.CanSeek;
        public override bool CanWrite => basestream.CanWrite;
        public override long Length => basestream.Length;

        public override long Position {
            get => basestream.Position;
            set => basestream.Position = value;
        }
    }
}