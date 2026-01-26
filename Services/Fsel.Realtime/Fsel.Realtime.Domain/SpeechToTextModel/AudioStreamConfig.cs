// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Domain.SpeechToTextModel
{
    /// <summary>
    /// Configuration for audio stream format
    /// Encapsulates audio format settings
    /// </summary>
    public class AudioStreamConfig
    {
        /// <summary>
        /// Default PCM configuration (16kHz, 16-bit, mono)
        /// </summary>
        public static readonly AudioStreamConfig DefaultPcm = new()
        {
            SampleRate = 16000,
            BitsPerSample = 16,
            Channels = 1
        };

        /// <summary>
        /// Sample rate in Hz (e.g., 16000, 44100)
        /// </summary>
        public int SampleRate { get; set; } = 16000;

        /// <summary>
        /// Number of bits per sample (e.g., 16, 24)
        /// </summary>
        public int BitsPerSample { get; set; } = 16;

        /// <summary>
        /// Number of audio channels (1 = mono, 2 = stereo)
        /// </summary>
        public int Channels { get; set; } = 1;

        /// <summary>
        /// Validate the configuration
        /// </summary>
        public bool IsValid()
        {
            return SampleRate > 0
                && BitsPerSample > 0
                && Channels > 0;
        }
    }
}
