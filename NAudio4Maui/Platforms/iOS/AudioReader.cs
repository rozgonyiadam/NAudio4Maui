using NAudio.Wave;

namespace NAudio4Maui
{
    public class AudioReader : WaveStream
    {
        public AudioReader(Stream stream) { }

        public override WaveFormat WaveFormat => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
