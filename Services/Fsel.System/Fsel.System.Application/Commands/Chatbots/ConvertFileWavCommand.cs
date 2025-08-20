// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.StorageServices;
    using global::System;
    using global::System.Diagnostics;
    using global::System.Text;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class ConvertFileWavCommand : IRequest<MethodResult<string>>
    {
        public string? File { get; set; }
    }

    public class ConvertFileWavCommandHandler : IRequestHandler<ConvertFileWavCommand, MethodResult<string>>
    {
        private readonly IStorageService _storageService;
        private readonly ILogger<ConvertFileWavCommandHandler> _logger;
        private const string _ffmpegPath = "ffmpeg";

        public ConvertFileWavCommandHandler(IStorageService storageService, ILogger<ConvertFileWavCommandHandler> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<MethodResult<string>> Handle(ConvertFileWavCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();
            methodResult.Result = await UpdateFileWav(request.File ?? string.Empty, cancellationToken);
            return methodResult;
        }

        private static string ReadFourCC(BinaryReader br)
        {
            var b = br.ReadBytes(4);
            return b.Length == 4 ? Encoding.ASCII.GetString(b) : string.Empty;
        }

        private static bool TryProbeWavHeader(
            string path,
            out ushort audioFormat, out ushort channels, out uint sampleRate, out ushort bitsPerSample)
        {
            audioFormat = channels = bitsPerSample = 0;
            sampleRate = 0;

            try
            {
                using var fs = File.OpenRead(path);
                using var br = new BinaryReader(fs);

                var riff = ReadFourCC(br);  // RIFF/RF64/RIFX
                _ = br.ReadUInt32();        // file size (skip)
                var wave = ReadFourCC(br);  // WAVE
                if (!((riff == "RIFF" || riff == "RF64" || riff == "RIFX") && wave == "WAVE"))
                {
                    return false;
                }
                // scan đến "fmt "
                while (fs.Position + 8 <= fs.Length)
                {
                    var chunkId = ReadFourCC(br);
                    var chunkSize = br.ReadUInt32();

                    if (chunkId == "fmt ")
                    {
                        audioFormat = br.ReadUInt16(); // 1 = PCM
                        channels = br.ReadUInt16();
                        sampleRate = br.ReadUInt32();
                        _ = br.ReadUInt32();            // byteRate (skip)
                        _ = br.ReadUInt16();            // blockAlign (skip)
                        bitsPerSample = br.ReadUInt16();

                        var remain = (int)chunkSize - 16;
                        if (remain > 0)
                        {
                            fs.Position += remain;
                        }
                        return true;
                    }

                    // skip + padding nếu chunkSize lẻ
                    fs.Position += chunkSize + (chunkSize % 2);
                }

                return false;
            }
            catch
            {
                return false; // không parse được => coi như không đúng
            }
        }

        private static bool IsPcm16kMono16BitWav(string path)
        {
            return TryProbeWavHeader(path,
                    out var fmt, out var ch, out var sr, out var bps)
                && fmt == 1 && ch == 1 && sr == 16000 && bps == 16;
        }

        private static bool IsTargetWav(string path)
        {
            // Chỉ coi là 'đã chuẩn' khi đuôi .wav và header đúng PCM 16k/mono/16-bit
            return string.Equals(Path.GetExtension(path), ".wav", StringComparison.OrdinalIgnoreCase)
                   && IsPcm16kMono16BitWav(path);
        }

        // ==== Materialize URL/local path to a local file ====

        private static async Task<(string localPath, bool isTemp)> MaterializeLocalAsync(
            string pathOrUrl, CancellationToken ct = default)
        {
            if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                var ext = Path.GetExtension(uri.AbsolutePath);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    ext = ".bin";
                }
                var tmp = Path.Combine(Path.GetTempPath(), $"audio_{Guid.NewGuid()}{ext}");

                using var http = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(100)
                };

                using var resp = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
                resp.EnsureSuccessStatusCode();

                await using var fs = new FileStream(tmp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await resp.Content.CopyToAsync(fs, ct);
                return (tmp, true);
            }

            // local path: trả nguyên (không phải temp)
            return (pathOrUrl, false);
        }

        // ==== FFMPEG availability check (fail sớm nếu không có ffmpeg) ====

        private static void EnsureFfmpegAvailable()
        {
            try
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                if (!p.WaitForExit(5000) || p.ExitCode != 0)
                    throw new InvalidOperationException("ffmpeg not available");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ffmpeg is not installed or not in PATH.", ex);
            }
        }

        // ==== Convert to WAV PCM 16k/mono/16-bit if needed ====

        private async Task<string> EnsurePcm16kMonoWavLocalAsync(string inputPath, CancellationToken ct = default)
        {
            EnsureFfmpegAvailable(); // kiểm tra 1 lần trước khi chạy
            var outputPath = Path.Combine(Path.GetTempPath(), $"audio_{Guid.NewGuid()}.wav");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    // -vn: bỏ video track nếu có; -y: overwrite; -hide_banner: gọn stderr
                    Arguments = $"-hide_banner -y -i \"{inputPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            process.Start();

            string err;
            try
            {
                err = await process.StandardError.ReadToEndAsync(ct);
            }
            catch
            {
                using var ms = new MemoryStream();
                await process.StandardError.BaseStream.CopyToAsync(ms, ct);
                err = Encoding.UTF8.GetString(ms.ToArray());
            }

            await process.WaitForExitAsync(ct);
            if (process.ExitCode != 0 || !File.Exists(outputPath))
            {
                _logger.LogError("ffmpeg convert failed (exit {ExitCode}). Stderr: {Stderr}", process.ExitCode, err);
                throw new InvalidOperationException($"ffmpeg convert failed: {err}");
            }

            return outputPath;
        }

        // ==== Upload and return URL ====
        private async Task<string> UploadWavAndGetUrlAsync(string localWavPath, CancellationToken ct = default)
        {
            await using var fs = File.OpenRead(localWavPath);
            var filePart = new StreamPart(fs, Path.GetFileName(localWavPath), "application/octet-stream");
            var resp = await _storageService.UpLoadFile(EnumFolderType.Files, EnumBucketType.FselPublic, filePart);
            var url = resp.Content?.Result;

            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("Upload failed: empty URL");
            }
            return url!;
        }

        // ==== Facade: nhận path/URL, đảm bảo WAV PCM 16k/mono/16-bit, upload và trả URL ====

        public async Task<string> UpdateFileWav(string audioFilePath, CancellationToken ct = default)
        {
            string? materialized = null;
            string? ensured = null;
            bool isTempDownload = false;

            try
            {
                (materialized, isTempDownload) = await MaterializeLocalAsync(audioFilePath, ct);

                // Nếu file đã là WAV chuẩn
                if (IsTargetWav(materialized))
                {
                    if (isTempDownload)
                    {
                        // Input ban đầu là URL và sau khi materialize thấy đã chuẩn -> trả lại URL gốc
                        return audioFilePath;
                    }

                    // Input là local path -> vẫn phải upload để trả URL hợp lệ cho client
                    return await UploadWavAndGetUrlAsync(materialized, ct);
                }

                // Convert nếu chưa đúng chuẩn
                ensured = await EnsurePcm16kMonoWavLocalAsync(materialized, ct);
                return await UploadWavAndGetUrlAsync(ensured, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý file âm thanh (audioFilePath={AudioFilePath})", audioFilePath);
                throw new InvalidOperationException("File âm thanh không hợp lệ hoặc không thể chuyển sang WAV PCM 16k/mono/16-bit.", ex);
            }
            finally
            {
                // dọn dẹp file tạm: chỉ xóa materialized nếu nó là file tải về từ URL
                TryDeleteIfTemp(isTempDownload ? materialized : null);

                // xóa file convert nếu khác materialized
                TryDeleteIfTemp(ensured != null && ensured != materialized ? ensured : null);
            }

            static void TryDeleteIfTemp(string? path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
