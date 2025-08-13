// Copyright (c) Atlantic. All rights reserved.

using System.Diagnostics;
using System.Globalization;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using Fsel.Storage.Domain.Enums.ErrorCodes;
using Fsel.Storage.Infrastructure.ValueSettings;
using Humanizer.Bytes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Fsel.Storage.Application.Services.AmazonS3Services
{
    public class AmazonS3Service : IAmazonS3Service, IDisposable
    {
        private readonly AppSetting _appSetting;
        private readonly AmazonS3Client _amazonS3Client;
        private readonly TransferUtility _transferUtility;
        private readonly ILogger<AmazonS3Service> _logger;
        private readonly ISystemFileProvider _systemFileProvider;
        private readonly float _targetWidthResize = 270F;
        private readonly float _targetHeightResize = 180F;
        private readonly double _partSize = ByteSize.FromMegabytes(100).Bytes; // Size of each part (100 MB)
        private readonly ICognitiveProvider _cognitiveProvider;
        private readonly RandomSecureHelper _randomSecure;

        private readonly Dictionary<EnumFolderType, double> _maximumCapacity = new Dictionary<EnumFolderType, double>
        {
            { EnumFolderType.Fsis, ByteSize.FromGigabytes(5).Bytes }, //maximum question size (5 GB)
            { EnumFolderType.AG, ByteSize.FromGigabytes(5).Bytes }, //maximum question size (5 GB)
            { EnumFolderType.HRM, ByteSize.FromGigabytes(5).Bytes }, //maximum question size (5 GB)
            { EnumFolderType.LHP, ByteSize.FromGigabytes(5).Bytes }, //maximum question size (5 GB)
            { EnumFolderType.Videos, ByteSize.FromGigabytes(5).Bytes }, //maximum video size (5 GB)
            { EnumFolderType.Files, ByteSize.FromGigabytes(5).Bytes }, //maximum file size (5 GB)
            { EnumFolderType.Questions, ByteSize.FromMegabytes(6).Bytes }, //maximum question size (6 MB)
            { EnumFolderType.Images, ByteSize.FromMegabytes(500).Bytes } //maximum image size (500 MB)
        };

        public AmazonS3Service(AppSetting appSetting, ISystemFileProvider systemFileProvider, ILogger<AmazonS3Service> logger, ICognitiveProvider cognitiveProvider)
        {
            _appSetting = appSetting;

            ArgumentNullException.ThrowIfNull(_appSetting);
            ArgumentNullException.ThrowIfNull(_appSetting.StorageConfig);
            ArgumentNullException.ThrowIfNull(_appSetting.StorageConfig.Folders);
            _amazonS3Client = new AmazonS3Client(_appSetting.StorageConfig.AwsAccessKey, _appSetting.StorageConfig.AwsSecretAccessKey, new AmazonS3Config
            {
                ServiceURL = PathHelper.AddScheme(_appSetting.StorageConfig.AwsS3BaseUrl!),
            });
            _transferUtility = new TransferUtility(_amazonS3Client);
            _systemFileProvider = systemFileProvider;
            _logger = logger;
            _cognitiveProvider = cognitiveProvider;
            _randomSecure = new RandomSecureHelper();
        }

        private async Task<string> UploadFileAsync(EnumBucketType? bucketType, Stream? stream, string? key)
        {
            if (stream == null || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var parts = new List<UploadPartResponse>();

            var bucketName = bucketType.HasValue ? bucketType.Value.GetDescription() : _appSetting.StorageConfig!.BucketName;

            var initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
            };
            var initiateResponse = await _amazonS3Client.InitiateMultipartUploadAsync(initiateRequest);

            using (var sourceStream = stream)
            {
                var buffer = new byte[(long)_partSize];
                int bytesRead;
                int partNumber = 1;

                var partUploadTasks = new List<Task>();

                while ((bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    var partBuffer = new byte[bytesRead];
                    Array.Copy(buffer, 0, partBuffer, 0, bytesRead);

                    var uploadPartRequest = new UploadPartRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        UploadId = initiateResponse.UploadId,
                        PartNumber = partNumber,
                        PartSize = bytesRead,
                        InputStream = new MemoryStream(partBuffer)
                    };

                    partUploadTasks.Add(Task.Run(async () =>
                    {
                        var uploadPartResponse = await _amazonS3Client.UploadPartAsync(uploadPartRequest);
                        parts.Add(uploadPartResponse);
                    }));

                    partNumber++;
                }

                await Task.WhenAll(partUploadTasks).ConfigureAwait(false);

                // Complete the upload process
                var completeMultipartUploadRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    UploadId = initiateResponse.UploadId,
                    PartETags = parts.Select(p => new PartETag { PartNumber = p.PartNumber, ETag = p.ETag }).ToList()
                };

                var complete = await _amazonS3Client.CompleteMultipartUploadAsync(completeMultipartUploadRequest);
                if (complete.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    return string.Empty;
                }

                return GenerateAwsFileUrl(_appSetting.StorageConfig?.AwsS3BaseUrl, bucketName, key) ?? string.Empty;
            }
        }

        private Stream OpenReadStreamResize(IFormFile file)
        {
            var stream = file.OpenReadStream();

            if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                using (var image = Image.Load(stream))
                {
                    float thumbWidth = _targetWidthResize;
                    float thumbHeight = _targetHeightResize;
                    //calculate  image  size
                    if (image.Width > image.Height)
                    {
                        thumbHeight = ((float)image.Height / image.Width) * thumbWidth;
                    }
                    else
                    {
                        thumbWidth = ((float)image.Width / image.Height) * thumbHeight;
                    }

                    // Tạo một ảnh vuông với kích thước đã resize
                    image.Mutate(x => x
                        .Resize(new ResizeOptions
                        {
                            Size = new Size((int)thumbWidth, (int)thumbHeight),
                        }));

                    // Tạo một memory stream cho ảnh đã resize
                    var resizedStream = new MemoryStream();
                    image.Save(resizedStream, new JpegEncoder());
                    resizedStream.Seek(0, SeekOrigin.Begin); // Đưa con trỏ của stream về đầu
                    return resizedStream;
                }
            }

            return stream;
        }

        private async Task<MethodResult<string?>> IsValidFileAsync(IFormFile? file, EnumFolderType folderType, bool isValidEmpty = false)
        {
            var result = new MethodResult<string?>();
            if (file == null || !_appSetting.StorageConfig!.IsValid())
            {
                result.AddErrorBadRequest(nameof(EnumFileErrorCode.FileCannotEmpty), nameof(file));
                return result;
            }

            var capacity = _maximumCapacity.FirstOrDefault(x => x.Key == folderType).Value;
            if (file.Length > capacity)
            {
                result.AddErrorBadRequest(nameof(EnumFileErrorCode.FileIsLargerThanAllowedSize), nameof(file), file.Length);
                return result;
            }

            //if (isValidEmpty && (file.IsFileType(Common.Enums.EnumFileType.Video) || file.IsFileType(Common.Enums.EnumFileType.Audio)))
            //{
            //    var text = await _cognitiveProvider.GetTranscriptionAsync(file);
            //    if (string.IsNullOrEmpty(text))
            //    {
            //        result.AddErrorBadRequest(nameof(EnumMediaErrorCode.EmptyMediaFile), nameof(file), text);
            //        return result;
            //    }

            //    var time = await MediaHelper.GetMediaDurationAsync(file, _systemFileProvider);
            //    if (!time.HasValue)
            //    {
            //        result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(time), time);
            //        return result;
            //    }

            //    if (time / Shared.Helpers.StringHelper.CountWords(text) > 5)
            //    {
            //        result.AddErrorBadRequest(nameof(EnumMediaErrorCode.NotEnough1WordEvery5Seconds), nameof(time), time);
            //        return result;
            //    }
            //}

            return result;
        }

        private static string? GenerateAwsFileUrl(string? baseUrl, string? bucketName, string? fileName)
        {
            var url = $"{baseUrl}/{bucketName}/{fileName}";
            return PathHelper.AddScheme(url);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _amazonS3Client.Dispose();
                _transferUtility.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<MethodResult<IList<string>>> UploadFilesAsync(EnumBucketType? bucketType, IList<IFormFile> files, EnumFolderType folderType, bool isResize = false, bool isValidEmpty = false, bool isAddSuffix = true)
        {
            MethodResult<IList<string>> results = new MethodResult<IList<string>>();
            results.Result = new List<string>();

            if (files != null)
            {
                foreach (var file in files)
                {
                    var uploadFile = UploadFileAsync(bucketType, file, folderType, isResize, isValidEmpty, isAddSuffix);
                    var result = await uploadFile.WaitAsync(cancellationToken: CancellationToken.None).ConfigureAwait(true);
                    if (!string.IsNullOrEmpty(result.Result))
                    {
                        results.Result.Add(result.Result);
                    }
                }
            }

            return results;
        }

        public async Task<string> UploadFileAsync(EnumBucketType? bucketType, string? file, string? folderName)
        {
            if (string.IsNullOrEmpty(file))
            {
                return string.Empty;
            }

            var key = PathHelper.Combine(folderName, Path.GetFileName(file));
            using Stream stream = new FileStream(file, FileMode.Open);

            return await UploadFileAsync(bucketType, stream, key);
        }

        public async Task<string?> UploadFileAsync(EnumBucketType? bucketType, IFormFile? file, string? folder, bool isResize = false, bool isAddSuffix = true)
        {
            if (file == null)
            {
                return default;
            }

            var fileName = file.FileName;
            if (isAddSuffix)
            {
                var randomValue = _randomSecure.Next(9999).ToString(CultureInfo.InvariantCulture);
                fileName = file.FileName.ReplaceSpecialChars().AddSuffix(randomValue, DateTime.UtcNow);
            }
            var key = PathHelper.Combine(folder, fileName);
            Stream stream;
            if (isResize)
            {
                stream = OpenReadStreamResize(file);
            }
            else
            {
                stream = file.OpenReadStream();
            }

            return await UploadFileAsync(bucketType, stream, key);
        }

        public async Task<MethodResult<string?>> UploadFileAsync(EnumBucketType? bucketType, IFormFile? file, EnumFolderType folderType, bool isResize = false, bool isValidEmpty = false, bool isAddSuffix = true)
        {
            MethodResult<string?> result = await IsValidFileAsync(file, folderType, isValidEmpty);
            if (file == null || !result.IsOK)
            {
                return result;
            }

            var filePath = await UploadFileAsync(bucketType, file, folderType.ToString(), isResize, isAddSuffix);
            if (string.IsNullOrEmpty(filePath))
            {
                result.AddErrorServer();
                return result;
            }

            result.Result = filePath;
            return result;
        }

        public async Task<IList<string>> UploadFilesAsync(EnumBucketType? bucketType, IList<IFormFile> files, string? folder, bool isResize = false)
        {
            var results = new List<string>();

            if (files != null)
            {
                foreach (var file in files)
                {
                    var uploadFile = UploadFileAsync(bucketType, file, folder, isResize);
                    var result = await uploadFile.WaitAsync(cancellationToken: CancellationToken.None).ConfigureAwait(true);
                    if (!string.IsNullOrEmpty(result))
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        public async Task<string> UploadFolderAsync(EnumBucketType? bucketType, string? folderPath, string? folderName)
        {
            var results = new List<string>();

            if (string.IsNullOrEmpty(folderPath))
            {
                return string.Empty;
            }

            var files = Directory.GetFiles(folderPath);
            if (files != null)
            {
                foreach (var file in files)
                {
                    var uploadFile = await UploadFileAsync(bucketType, file, folderName);
                    if (!string.IsNullOrEmpty(uploadFile))
                    {
                        results.Add(uploadFile);
                    }
                }
            }

            var bucketName = bucketType.HasValue ? bucketType.Value.GetDescription() : _appSetting.StorageConfig!.BucketName;
            return GenerateAwsFileUrl(_appSetting.StorageConfig!.AwsS3BaseUrl, bucketName, folderName) ?? string.Empty;
        }

        private async Task<MethodResult<string>> StartResolutions(EnumBucketType? bucketType, string? inputPath)
        {
            _logger.LogInformation($"Start resolution 1");

            var result = new MethodResult<string>();
            if (string.IsNullOrEmpty(inputPath))
            {
                return result;
            }

            string videoName = Path.GetFileNameWithoutExtension(inputPath);
            string rootFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, videoName);
            Directory.CreateDirectory(rootFolderPath);

            // Generate M3U8 playlists with multiple quality options for each video
            var qualities = new[]
            {
                new { Name = "240p", Resolution = "426x240", Bitrate = "300k" },
                new { Name = "360p", Resolution = "640x360", Bitrate = "400k" },
                new { Name = "480p", Resolution = "854x480", Bitrate = "800k" },
                new { Name = "720p", Resolution = "1280x720", Bitrate = "1500k" },
                new { Name = "1080p", Resolution = "1920x1080", Bitrate = "3000k" }
                // Add more quality options as needed
            };

            _logger.LogInformation($"Start resolution 2");

            foreach (var quality in qualities)
            {
                string outputM3U8 = Path.Combine(rootFolderPath, $"{quality.Name}.m3u8");
                string ffmpegArgs = $"-i \"{inputPath}\" -c:v libx264 -preset ultrafast -b:v {quality.Bitrate} -vf \"scale={quality.Resolution}\" -c:a aac -b:a 128k -hls_time 10 -hls_list_size 0 -f hls \"{outputM3U8}\"";

                await FfmpegStart(ffmpegArgs);
            }

            _logger.LogInformation($"Start resolution 3");

            // Create a master M3U8 playlist for each video
            using (var masterM3U8Writer = new StreamWriter(Path.Combine(rootFolderPath, $"{videoName}.m3u8")))
            {
                await masterM3U8Writer.WriteLineAsync("#EXTM3U");
                foreach (var quality in qualities)
                {
                    await masterM3U8Writer.WriteLineAsync($"#EXT-X-STREAM-INF:BANDWIDTH={quality.Bitrate.Substring(0, quality.Bitrate.Length - 1)}000,RESOLUTION={quality.Resolution}");
                    await masterM3U8Writer.WriteLineAsync($"{quality.Name}.m3u8");
                }
            }

            _logger.LogInformation($"Start resolution 4");

            var folderPath = PathHelper.Combine(EnumFolderType.Videos.ToString(), videoName);
            var folderRemoteUrl = await UploadFolderAsync(bucketType, rootFolderPath, folderPath);

            _logger.LogInformation($"Start resolution 5");

            _systemFileProvider.DeleteFiles(inputPath);
            _systemFileProvider.DeleteFolders(true, rootFolderPath);

            _logger.LogInformation($"Start resolution 6: {folderRemoteUrl}");

            if (string.IsNullOrEmpty(folderRemoteUrl))
            {
                result.AddErrorServer();
                return result;
            }

            result.Result = PathHelper.Combine(folderRemoteUrl, $"{videoName}.m3u8");
            return result;
        }

        public async Task<MethodResult<string>> UploadResolutions(EnumBucketType? bucketType, string? url)
        {
            var result = new MethodResult<string>();
            if (string.IsNullOrEmpty(url))
            {
                return result;
            }

            var inputPath = await _systemFileProvider.SaveFileFromUrl(url);
            result = await StartResolutions(bucketType, inputPath);
            return result;
        }

        public async Task<MethodResult<string>> UploadResolutions(EnumBucketType? bucketType, IFormFile? file)
        {
            var result = new MethodResult<string>();
            if (file == null)
            {
                return result;
            }

            _logger.LogInformation($"Start save file to disk");

            var inputPath = await _systemFileProvider.SaveFile(file);

            _logger.LogInformation($"End save file to disk");

            _logger.LogInformation($"Start resolutions");

            result = await StartResolutions(bucketType, inputPath);

            _logger.LogInformation($"End resolutions");
            return result;
        }

        private static async Task FfmpegStart(string ffmpegArgs)
        {
            var ffmpegProcess = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = new Process { StartInfo = ffmpegProcess })
            {
                process.Start();
                await process.WaitForExitAsync();
                process.WaitForExit();
            }
        }
    }
}
