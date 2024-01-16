using Android.Media;
using NAudio.Wave;

namespace NAudio4Maui
{
    public class AudioReader : WaveStream
    {
        private WaveFormat waveFormat;
        private long length;
        private long position;

        private MediaExtractor mediaExtractor;
        private MediaCodec mediaCodec;
        private MemoryStream decodedData;

        private int channelCount;
        private int sampleRate;
        private long duration;
        private string mimeType;
        public AudioReader(System.IO.Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            mediaExtractor = new MediaExtractor();
            mediaExtractor.SetDataSource(new StreamMediaDataSource(ms));

            Initialize();
        }

        private void Initialize()
        {
            int trackIndex = SelectTrack(mediaExtractor);
            MediaFormat mediaFormat = mediaExtractor.GetTrackFormat(trackIndex);

            channelCount = mediaFormat.GetInteger(MediaFormat.KeyChannelCount);
            sampleRate = mediaFormat.GetInteger(MediaFormat.KeySampleRate);
            duration = mediaFormat.GetLong(MediaFormat.KeyDuration);
            mimeType = mediaFormat.GetString(MediaFormat.KeyMime);

            mediaCodec = MediaCodec.CreateDecoderByType(mediaFormat.GetString(MediaFormat.KeyMime));
            mediaCodec.Configure(mediaFormat, null, null, 0);
            mediaCodec.Start();

            decodedData = new MemoryStream();

            DecodeAudio();
        }

        private void DecodeAudio()
        {
            Java.Nio.ByteBuffer inputBuffer = null;
            Java.Nio.ByteBuffer outputBuffer = null;

            MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();

            while (true)
            {
                int inputBufferIndex = mediaCodec.DequeueInputBuffer(-1);

                if (inputBufferIndex >= 0)
                {
                    inputBuffer = mediaCodec.GetInputBuffer(inputBufferIndex);
                    int sampleSize = mediaExtractor.ReadSampleData(inputBuffer, 0);

                    if (sampleSize < 0)
                    {
                        mediaCodec.QueueInputBuffer(inputBufferIndex, 0, 0, 0, MediaCodecBufferFlags.EndOfStream);
                    }
                    else
                    {
                        mediaCodec.QueueInputBuffer(inputBufferIndex, 0, sampleSize, mediaExtractor.SampleTime, 0);
                        mediaExtractor.Advance();
                    }
                }

                int outputBufferIndex = mediaCodec.DequeueOutputBuffer(bufferInfo, 0);

                if (outputBufferIndex >= 0)
                {
                    outputBuffer = mediaCodec.GetOutputBuffer(outputBufferIndex);

                    byte[] outputData = new byte[bufferInfo.Size];
                    outputBuffer.Get(outputData, 0, bufferInfo.Size);

                    decodedData.Write(outputData, 0, bufferInfo.Size);

                    mediaCodec.ReleaseOutputBuffer(outputBufferIndex, false);
                }

                if ((bufferInfo.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
                {
                    break;
                }
            }
        }

        private int SelectTrack(MediaExtractor extractor)
        {
            for (int i = 0; i < extractor.TrackCount; i++)
            {
                MediaFormat format = extractor.GetTrackFormat(i);
                string mime = format.GetString(MediaFormat.KeyMime);

                if (mime.StartsWith("audio/"))
                {
                    extractor.SelectTrack(i);
                    return i;
                }
            }

            throw new InvalidOperationException("No audio track found in the media file.");
        }

        public override WaveFormat WaveFormat
        {
            get
            {
                return WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount);
            }
        }

        public override long Length
        {
            get
            {
                return decodedData.Length;
            }
        }

        public override long Position
        {
            get
            {
                return decodedData.Position;
            }
            set
            {
                decodedData.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;

            bytesRead = decodedData.Read(buffer, offset, count);

            return bytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mediaCodec.Stop();
                mediaCodec.Release();
                mediaExtractor.Release();
                decodedData.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
