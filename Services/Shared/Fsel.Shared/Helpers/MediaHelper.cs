// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Diagnostics;
    using System.Globalization;

    public static class MediaHelper
    {
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
    }
}
