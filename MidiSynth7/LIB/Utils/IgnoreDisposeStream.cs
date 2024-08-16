using System;
using System.IO;
/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.Utils
{
    /// <summary>
    /// Pass-through stream that ignores Dispose
    /// Useful for dealing with MemoryStreams that you want to re-use
    /// </summary>
    public class IgnoreDisposeStream : Stream
    {
        /// <summary>
        /// The source stream all other methods fall through to
        /// </summary>
        public Stream SourceStream { get; private set; }

        /// <summary>
        /// If true the Dispose will be ignored, if false, will pass through to the SourceStream
        /// Set to true by default
        /// </summary>
        public bool IgnoreDispose { get; set; }

        /// <summary>
        /// Creates a new IgnoreDisposeStream
        /// </summary>
        /// <param name="sourceStream">The source stream</param>
        public IgnoreDisposeStream(Stream sourceStream)
        {
            SourceStream = sourceStream;
            IgnoreDispose = true;
        }

        /// <summary>
        /// Can Read
        /// </summary>
        public override bool CanRead => SourceStream.CanRead;

        /// <summary>
        /// Can Seek
        /// </summary>
        public override bool CanSeek => SourceStream.CanSeek;

        /// <summary>
        /// Can write to the underlying stream
        /// </summary>
        public override bool CanWrite => SourceStream.CanWrite;

        /// <summary>
        /// Flushes the underlying stream
        /// </summary>
        public override void Flush()
        {
            SourceStream.Flush();
        }

        /// <summary>
        /// Gets the length of the underlying stream
        /// </summary>
        public override long Length => SourceStream.Length;

        /// <summary>
        /// Gets or sets the position of the underlying stream
        /// </summary>
        public override long Position
        {
            get
            {
                return SourceStream.Position;
            }
            set
            {
                SourceStream.Position = value;
            }
        }

        /// <summary>
        /// Reads from the underlying stream
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return SourceStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Seeks on the underlying stream
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return SourceStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the underlying stream
        /// </summary>
        public override void SetLength(long value)
        {
            SourceStream.SetLength(value);
        }

        /// <summary>
        /// Writes to the underlying stream
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            SourceStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Dispose - by default (IgnoreDispose = true) will do nothing,
        /// leaving the underlying stream undisposed
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!IgnoreDispose)
            {
                SourceStream.Dispose();
                SourceStream = null;
            }
        }
    }
}
