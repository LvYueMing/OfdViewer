namespace OFDViewer.Parse
{
    /// <summary>
    /// 对归档条目的实际读取量实施硬限制，防止异常压缩数据绕过条目声明长度。
    /// </summary>
    internal sealed class LimitedReadStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly long _maxBytes;
        private long _bytesRead;

        public LimitedReadStream(Stream innerStream, long maxBytes)
        {
            _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
            if (maxBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBytes));

            _maxBytes = maxBytes;
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => _bytesRead;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int allowedCount = GetAllowedReadCount(count);
            int read = _innerStream.Read(buffer, offset, allowedCount);
            _bytesRead += read;
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            int allowedCount = GetAllowedReadCount(buffer.Length);
            int read = _innerStream.Read(buffer[..allowedCount]);
            _bytesRead += read;
            return read;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            int allowedCount = GetAllowedReadCount(buffer.Length);
            int read = await _innerStream.ReadAsync(buffer[..allowedCount], cancellationToken).ConfigureAwait(false);
            _bytesRead += read;
            return read;
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            return ReadAsyncCore(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _innerStream.Dispose();

            base.Dispose(disposing);
        }

        private int GetAllowedReadCount(int requestedCount)
        {
            if (requestedCount == 0)
                return 0;

            long remaining = _maxBytes - _bytesRead;
            if (remaining <= 0)
            {
                int nextByte = _innerStream.ReadByte();
                if (nextByte >= 0)
                    throw new InvalidDataException($"归档条目实际读取量超过限制：{_maxBytes} 字节");

                return 0;
            }

            return (int)Math.Min(requestedCount, remaining);
        }

        private async Task<int> ReadAsyncCore(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            int allowedCount = GetAllowedReadCount(count);
            int read = await _innerStream
                .ReadAsync(buffer, offset, allowedCount, cancellationToken)
                .ConfigureAwait(false);
            _bytesRead += read;
            return read;
        }
    }
}
