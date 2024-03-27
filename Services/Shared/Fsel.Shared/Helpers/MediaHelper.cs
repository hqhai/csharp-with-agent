// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;
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

        public static async Task<int?> GetMediaDurationAsync(IFormFile mediaFile)
        {
            try
            {
                if (mediaFile == null || mediaFile.Length == 0)
                {
                    return null;
                }

                using (var stream = mediaFile.OpenReadStream())
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-i pipe:0 -hide_banner -f null -",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = psi })
                    {
                        process.Start();

                        // Đọc và ghi dữ liệu từng phần nhỏ vào luồng đầu vào chuẩn của quá trình ffmpeg
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                        {
                            await process.StandardInput.BaseStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                        }
                        process.StandardInput.Close();

                        string output = await process.StandardError.ReadToEndAsync();

                        if (!output.Contains("Duration", StringComparison.OrdinalIgnoreCase))
                        {
                            return null;
                        }

                        string durationTag = "Duration: ";
                        int start = output.IndexOf(durationTag, StringComparison.CurrentCulture) + durationTag.Length;
                        int end = output.IndexOf(",", start, StringComparison.CurrentCulture);
                        string durationString = output.Substring(start, end - start);
                        try
                        {
                            TimeSpan duration = TimeSpan.Parse(durationString, CultureInfo.InvariantCulture);
                            await process.WaitForExitAsync();
                            return (int)duration.TotalSeconds;
                        }
                        catch
                        {
                            await process.WaitForExitAsync();
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MediaHelper.GetMediaDurationAsync: {ex}");
                return null;
            }
        }

        public static EnumMediaType? GetMediaType(string? mediaUrl)
        {
            return !string.IsNullOrEmpty(mediaUrl) ? (Regex.IsMatch(mediaUrl, PATTERNVIDEO) ? EnumMediaType.Video : Regex.IsMatch(mediaUrl, PATTERNAUDIO) ? EnumMediaType.Audio : default) : default;
        }
    }
}
