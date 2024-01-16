using NAudio.Wave;

namespace NAudio4Maui
{
    public class AudioReader : StreamMediaFoundationReader
    {
        public AudioReader(Stream stream, MediaFoundationReaderSettings settings = null) : base(stream, settings)
        {
        }
    }
}
