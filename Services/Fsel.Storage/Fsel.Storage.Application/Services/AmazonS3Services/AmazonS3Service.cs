// Copyright (c) Atlantic. All rights reserved.

using System.Diagnostics;
using System.Reflection.Metadata;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Interfaces;
using Fsel.Storage.Domain.Enums;
using Fsel.Storage.Domain.Enums.ErrorCodes;
using Fsel.Storage.Infrastructure.ValueSettings;
using Humanizer.Bytes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OneSignalApi.Model;
using SixLabors.ImageSharp.Formats.Jpeg;

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

        private readonly Dictionary<EnumFolderType, double> _maximumCapacity = new Dictionary<EnumFolderType, double>
        {
            { EnumFolderType.Fsis, ByteSize.FromGigabytes(1).Bytes }, //maximum question size (1 GB)
            { EnumFolderType.Videos, ByteSize.FromGigabytes(5).Bytes }, //maximum video size (5 GB)
            { EnumFolderType.Files, ByteSize.FromMegabytes(6).Bytes }, //maximum file size (6 MB)
            { EnumFolderType.Questions, ByteSize.FromMegabytes(6).Bytes }, //maximum question size (6 MB)
            { EnumFolderType.Images, ByteSize.FromMegabytes(500).Bytes } //maximum image size (500 MB)
        };

        public AmazonS3Service(AppSetting appSetting, ISystemFileProvider systemFileProvider, ILogger<AmazonS3Service> logger)
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
        }

        private async Task<string> UploadFileAsync(Stream? stream, string? key)
        {
            if (stream == null || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var parts = new List<UploadPartResponse>();

            var initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = _appSetting.StorageConfig!.BucketName,
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
                        BucketName = _appSetting.StorageConfig!.BucketName,
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
                    BucketName = _appSetting.StorageConfig!.BucketName,
                    Key = key,
                    UploadId = initiateResponse.UploadId,
                    PartETags = parts.Select(p => new PartETag { PartNumber = p.PartNumber, ETag = p.ETag }).ToList()
                };

                var complete = await _amazonS3Client.CompleteMultipartUploadAsync(completeMultipartUploadRequest);
                if (complete.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    return string.Empty;
                }

                return GenerateAwsFileUrl(_appSetting.StorageConfig.BucketName, _appSetting.StorageConfig.AwsS3BaseUrl, key) ?? string.Empty;
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

        private MethodResult<string?> IsValidFile(IFormFile? file, EnumFolderType folderType)
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

            return result;
        }

        private static string? GenerateAwsFileUrl(string? bucketName, string? baseUrl, string? fileName)
        {
            var url = $"{bucketName}.{baseUrl}/{fileName}";
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

        public async Task<MethodResult<IList<string>>> UploadFilesAsync(IList<IFormFile> files, EnumFolderType folderType, bool isResize = false)
        {
            MethodResult<IList<string>> results = new MethodResult<IList<string>>();
            results.Result = new List<string>();

            if (files != null)
            {
                foreach (var file in files)
                {
                    var uploadFile = UploadFileAsync(file, folderType, isResize);
                    var result = await uploadFile.WaitAsync(cancellationToken: CancellationToken.None).ConfigureAwait(true);
                    if (!string.IsNullOrEmpty(result.Result))
                    {
                        results.Result.Add(result.Result);
                    }
                }
            }

            return results;
        }

        public async Task<string> UploadFileAsync(string? file, string? folderName)
        {
            if (string.IsNullOrEmpty(file))
            {
                return string.Empty;
            }

            var key = PathHelper.Combine(folderName, Path.GetFileName(file));
            using Stream stream = new FileStream(file, FileMode.Open);

            return await UploadFileAsync(stream, key);
        }

        public async Task<string?> UploadFileAsync(IFormFile? file, string? folder, bool isResize = false)
        {
            if (file == null)
            {
                return default;
            }

            var key = PathHelper.Combine(folder, file.FileName.ReplaceSpecialChars().AddSuffix());
            Stream stream;
            if (isResize)
            {
                stream = OpenReadStreamResize(file);
            }
            else
            {
                stream = file.OpenReadStream();
            }

            return await UploadFileAsync(stream, key);
        }

        public async Task<MethodResult<string?>> UploadFileAsync(IFormFile? file, EnumFolderType folderType, bool isResize = false)
        {
            MethodResult<string?> result = IsValidFile(file, folderType);
            if (file == null || !result.IsOK)
            {
                return result;
            }

            var filePath = await UploadFileAsync(file, folderType.ToString(), isResize);
            if (string.IsNullOrEmpty(filePath))
            {
                result.AddErrorServer();
                return result;
            }

            result.Result = filePath;
            return result;
        }

        public async Task<IList<string>> UploadFilesAsync(IList<IFormFile> files, string? folder, bool isResize = false)
        {
            var results = new List<string>();

            if (files != null)
            {
                foreach (var file in files)
                {
                    var uploadFile = UploadFileAsync(file, folder, isResize);
                    var result = await uploadFile.WaitAsync(cancellationToken: CancellationToken.None).ConfigureAwait(true);
                    if (!string.IsNullOrEmpty(result))
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        public async Task<string> UploadFolderAsync(string? folderPath, string? folderName)
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
                    var uploadFile = await UploadFileAsync(file, folderName);
                    if (!string.IsNullOrEmpty(uploadFile))
                    {
                        results.Add(uploadFile);
                    }
                }
            }

            return GenerateAwsFileUrl(_appSetting.StorageConfig!.BucketName, _appSetting.StorageConfig.AwsS3BaseUrl, folderName) ?? string.Empty;
        }

        private async Task<MethodResult<string>> StartResolutions(string? inputPath)
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
                new { Name = "360p", Resolution = "640x360", Bitrate = "400k" },
                new { Name = "480p", Resolution = "854x480", Bitrate = "800k" },
                new { Name = "720p", Resolution = "1280x720", Bitrate = "1500k" }
                // Add more quality options as needed
            };

            _logger.LogInformation($"Start resolution 2");

            foreach (var quality in qualities)
            {
                string outputM3U8 = Path.Combine(rootFolderPath, $"{quality.Name}.m3u8");
                string ffmpegArgs = $"-i {inputPath} -c:v libx264 -b:v {quality.Bitrate} -vf \"scale={quality.Resolution}\" -c:a aac -b:a 128k -hls_time 60 -hls_list_size 0 -f hls {outputM3U8}";

                FfmpegStart(ffmpegArgs);
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
            var folderRemoteUrl = await UploadFolderAsync(rootFolderPath, folderPath);

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

        public async Task<MethodResult<string>> UploadResolutions(string? url)
        {
            var result = new MethodResult<string>();
            if (string.IsNullOrEmpty(url))
            {
                return result;
            }

            var inputPath = await _systemFileProvider.SaveFileFromUrl(url);
            result = await StartResolutions(inputPath);
            return result;
        }

        public async Task<MethodResult<string>> UploadResolutions(IFormFile? file)
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

            result = await StartResolutions(inputPath);

            _logger.LogInformation($"End resolutions");
            return result;
        }

        private static void FfmpegStart(string ffmpegArgs)
        {
            var ffmpegProcess = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = Process.Start(ffmpegProcess);
            if (process != null)
            {
                process.WaitForExit();
            }
        }
    }
}
