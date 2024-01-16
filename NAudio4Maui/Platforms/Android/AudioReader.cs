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
            // Válaszd ki a megfelelő hangfájl sávját és formátumát
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

            // Olvasd be és dekódold az adatokat, majd írd be a MemoryStream-be
            DecodeAudio();
        }

        private void DecodeAudio()
        {
            //int bufferSize = 1024 * 2; // Példa: 2 KB buffer méret
            //byte[] inputBuffer = new byte[bufferSize];
            //byte[] outputBuffer = new byte[bufferSize];

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

                    // Olvassuk ki a dekódolt adatokat a ByteBuffer-ból
                    byte[] outputData = new byte[bufferInfo.Size];
                    outputBuffer.Get(outputData, 0, bufferInfo.Size);

                    // Itt kezeld a dekódolt adatokat (pl. írd be a MemoryStream-be)
                    decodedData.Write(outputData, 0, bufferInfo.Size);

                    mediaCodec.ReleaseOutputBuffer(outputBufferIndex, false);
                }

                if ((bufferInfo.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
                {
                    break; // Kilép a ciklusból, ha elérte a fájl végét
                }
            }
        }

        private int SelectTrack(MediaExtractor extractor)
        {
            // Válaszd ki a megfelelő sávot a hangfájlból
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
                //if (mediaCodec == null)
                //    throw new InvalidOperationException("MediaCodec is not initialized.");

                //MediaFormat outputFormat = mediaCodec.OutputFormat;

                //// Ellenőrizd, hogy a kimeneti formátum PCM-e
                //if (outputFormat.GetString(MediaFormat.KeyMime).StartsWith("audio/"))
                //{
                //    int sampleRate = outputFormat.GetInteger(MediaFormat.KeySampleRate);
                //    int channelCount = outputFormat.GetInteger(MediaFormat.KeyChannelCount);

                //    return WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount);
                //}
                //else
                //{
                //    throw new InvalidOperationException("Unsupported audio format.");
                //}

                return WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount);
            }
        }

        public override long Length
        {
            get
            {
                // Visszaadja a WaveStream hosszát, például a dekódált adat hosszával
                return decodedData.Length;
            }
        }

        public override long Position
        {
            get
            {
                // Visszaadja a WaveStream pozícióját, például a MemoryStream pozíciójával
                return decodedData.Position;
            }
            set
            {
                // Beállítja a WaveStream pozícióját, például a MemoryStream pozíciójával
                decodedData.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Olvasd ki a WaveStream-ből a megfelelő mennyiségű dekódált adatot
            int bytesRead = 0;

            // Például olvasd ki a dekódált adatokat a MemoryStream-ből
            bytesRead = decodedData.Read(buffer, offset, count);

            // Kezelheted az olvasás során fellépő esetleges hibákat

            return bytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            // Felszabadítsd az eszközöket (pl. MediaExtractor, MediaCodec, MemoryStream)
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
