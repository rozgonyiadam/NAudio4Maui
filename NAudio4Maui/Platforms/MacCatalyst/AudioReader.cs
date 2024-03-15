using AVFoundation;
using Foundation;
using NAudio.Wave;

namespace NAudio4Maui
{
    public class AudioReader : WaveStream
    {
        private readonly AVAudioPlayer _audioPlayer;
        private readonly WaveFormat _waveFormat;

        public AudioReader(Stream audioStream)
        {
            double? sampleRateDouble = _audioPlayer.SoundSetting.SampleRate;
            int? sampleRate = sampleRateDouble.HasValue ? (int?)Convert.ToInt32(sampleRateDouble.Value) : null;

            int? numberChannels = _audioPlayer.SoundSetting.NumberChannels;

            if (!sampleRate.HasValue || !numberChannels.HasValue)
            {
                throw new InvalidOperationException("Audio settings are not valid or missing.");
            }

            NSData audioData = NSData.FromStream(audioStream);

            _audioPlayer = AVAudioPlayer.FromData(audioData);
            _waveFormat = new WaveFormat(sampleRate.Value, numberChannels.Value);
        }

        public override WaveFormat WaveFormat => _waveFormat;

        public override long Length => unchecked((long)_audioPlayer.Data.Length);

        public override long Position
        {
            get => Convert.ToInt64(_audioPlayer.CurrentTime * _waveFormat.AverageBytesPerSecond);
            set => _audioPlayer.CurrentTime = value / (double)_waveFormat.AverageBytesPerSecond;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Stream dataStream = _audioPlayer.Data.AsStream();

            int bytesRead = dataStream.Read(buffer, offset, count);

            if (bytesRead == 0)
            {
                return -1;
            }

            return bytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _audioPlayer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
