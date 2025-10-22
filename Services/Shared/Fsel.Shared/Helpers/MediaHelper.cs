// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Http;

    public static class MediaHelper
    {
        private const string PATTERNVIDEO = @"\.mp4$|\.avi$|\.mkv$";
        private const string PATTERNAUDIO = @"\.mp3$|\.wav$|\.ogg$|\.m4a$";

        public static int? GetMediaDurationAsync(string? mediaUrl)
        {
            if (string.IsNullOrEmpty(mediaUrl))
            {
                return null;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{mediaUrl}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {

                    process.Start();

                    string output = process.StandardError.ReadToEnd();

                    if (output.Contains("N/A", StringComparison.CurrentCulture))
                    {
                        return null;
                    }

                    string durationTag = "Duration: ";
                    int start = output.IndexOf(durationTag, StringComparison.CurrentCulture) + durationTag.Length;
                    int end = output.IndexOf(",", start, StringComparison.CurrentCulture);
                    string durationString = output.Substring(start, end - start);
                    try
                    {
                        TimeSpan duration = TimeSpan.Parse(durationString, CultureInfo.CurrentCulture);
                        process.WaitForExit();
                        return (int)duration.TotalSeconds;
                    }
                    catch
                    {
                        process.WaitForExit();
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MediaHelper.GetMediaDurationAsync: {ex}");
                return null;
            }
        }

        public static bool IsProbablyFileUrl(string? s)
        {
            if (!Uri.TryCreate(s, UriKind.Absolute, out var uri))
            {
                return false;
            }
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }
            // Rất đơn giản: đường dẫn URL có phần mở rộng ở cuối
            return Path.HasExtension(uri.AbsolutePath);
        }

        public static async Task<int?> GetMediaDurationAsync(IFormFile mediaFile, ISystemFileProvider systemFileProvider)
        {
            ArgumentNullException.ThrowIfNull(systemFileProvider);
            var path = await systemFileProvider.SaveFile(mediaFile);
            var result = GetMediaDurationAsync(path);
            systemFileProvider.DeleteFiles(path);
            return result;
        }

        public static EnumMediaType? GetMediaType(string? mediaUrl)
        {
            return !string.IsNullOrEmpty(mediaUrl) ? (Regex.IsMatch(mediaUrl, PATTERNVIDEO) ? EnumMediaType.Video : Regex.IsMatch(mediaUrl, PATTERNAUDIO) ? EnumMediaType.Audio : default) : default;
        }
    }
}
