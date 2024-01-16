using NAudio.Wave;

namespace NAudio4Maui
{
    // All the code in this file is only included on Windows.
    public class AudioOut : WaveOut
    {
        public new double GetPosition() => base.GetPosition();
    }
}