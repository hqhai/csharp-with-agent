// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Diagnostics;
    using System.Globalization;

    public static class MediaHelper
    {
        public static int? GetMediaDurationAsync(string? mediaUrl)
        {
            try
            {
                string command = $"-i \"{mediaUrl}\"";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    string? durationLine = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                                 .FirstOrDefault(line => line.Contains("Duration", StringComparison.CurrentCulture))
                                           ?? null;

                    if (!string.IsNullOrEmpty(durationLine))
                    {
                        int start = durationLine.IndexOf("Duration: ", StringComparison.CurrentCulture) + "Duration: ".Length;
                        int end = durationLine.IndexOf(",", StringComparison.CurrentCulture);
                        string duration = durationLine.Substring(start, end - start).Trim();

                        TimeSpan timeSpan = TimeSpan.Parse(duration, CultureInfo.CurrentCulture);
                        return (int)timeSpan.TotalSeconds;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
