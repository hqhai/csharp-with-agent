// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.FFmpegServices.Models
{
    using global::System.Text.Json.Serialization;

    public class InfoAudioModel
    {
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("audio_info")]
        public AudioInfo? AudioInfo { get; set; }
    }

    public class AudioInfo
    {
        [JsonPropertyName("codec")]
        public string? Codec { get; set; }

        [JsonPropertyName("sample_rate")]
        public int SampleRate { get; set; }

        [JsonPropertyName("channels")]
        public int Channels { get; set; }

        [JsonPropertyName("bit_depth")]
        public string? BitDepth { get; set; }

        [JsonPropertyName("mean_volume")]
        public double MeanVolume { get; set; }

        [JsonPropertyName("max_volume")]
        public double MaxVolume { get; set; }

        [JsonPropertyName("audio_time")]
        public double AudioTime { get; set; }
    }
}
