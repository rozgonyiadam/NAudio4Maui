using Android.Media;

namespace NAudio4Maui
{
    //Originally copied from: https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/src/Plugin.Maui.Audio/StreamMediaDataSource.android.cs
    class StreamMediaDataSource : MediaDataSource
    {
        System.IO.Stream data;

        public StreamMediaDataSource(System.IO.Stream data)
        {
            this.data = data;
        }

        public override long Size => data.Length;

        public override int ReadAt(long position, byte[]? buffer, int offset, int size)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (data.CanSeek)
            {
                data.Seek(position, SeekOrigin.Begin);
            }

            return data.Read(buffer, offset, size);
        }

        public override void Close()
        {
            data.Dispose();
            data = System.IO.Stream.Null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            data.Dispose();
            data = System.IO.Stream.Null;
        }
    }
}
