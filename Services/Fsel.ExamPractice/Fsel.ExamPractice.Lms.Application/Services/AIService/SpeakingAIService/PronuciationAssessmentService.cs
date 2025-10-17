using System.Net.Http.Headers;
using Fsel.Common.Helpers;
using Fsel.ExamPractice.Domain.Models.EntityModels;
using Fsel.ExamPractice.Infrastructure.ValueSettings;
using Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService.Interface;
using Fsel.ExamPractice.Lms.Application.Services.FFmpegServices;
using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Refit;

namespace Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService
{
    /// <summary>
    /// Dịch vụ xử lý giọng nói sử dụng Azure Cognitive Services
    /// </summary>
    public class PronuciationAssessmentService : IPronuciationAssessmentService
    {
        private SpeechConfig _speechConfig;
        private readonly AppSetting _appSetting;
        private const int LowAccuracyScore = 60;
        private const int MediumAccuracyScore = 80;
        private readonly ILogger<PronuciationAssessmentService> _logger;
        private readonly IFFmpegServices _fFmpegServices;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Khởi tạo dịch vụ đánh giá phát âm với Azure
        /// </summary>
        /// <param name="subscriptionKey">Khóa đăng ký của Azure</param>
        /// <param name="region">Khu vực của dịch vụ</param>
        public PronuciationAssessmentService(AppSetting appSetting,
            ILogger<PronuciationAssessmentService> logger,
            IFFmpegServices fFmpegServices,
            IHttpClientFactory httpClientFactory)
        {
            _appSetting = appSetting;
            _speechConfig = SpeechConfig.FromSubscription(_appSetting!.AzureAiConfig!.SecondApiKey, _appSetting!.AzureAiConfig!.Location);
            _speechConfig.SpeechRecognitionLanguage = _appSetting!.AzureAiConfig!.SpeechRecognitionLanguage;

            // Thêm cấu hình cho speech config
            _speechConfig.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "1000");
            _speechConfig.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "1000");
            _speechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "1000");

            _logger = logger;
            _fFmpegServices = fFmpegServices;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Đánh giá phát âm từ microphone
        /// </summary>
        /// <param name="referenceText">Văn bản tham chiếu</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        public async Task<PronunciationAssessmentModel> AssessPronunciationFromMicrophoneAsync(string referenceText)
        {
            if (string.IsNullOrEmpty(referenceText))
            {
                throw new ArgumentNullException(nameof(referenceText), "Văn bản tham chiếu không được để trống");
            }
            var pronunciationConfig = PronunciationAssessmentConfig.FromJson($"{{\"referenceText\":\"{referenceText}\",\"gradingSystem\":\"HundredMark\",\"granularity\":\"Word\",\"enableSyllableLevelAssessment\":true,\"enablePhonemeLevelAssessment\":true,\"enableProsodyAssessment\":true}}");

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);

            pronunciationConfig.ApplyTo(recognizer);
            var result = await recognizer.RecognizeOnceAsync();
            return await ProcessPronunciationResult(result);
        }

        /// <summary>
        /// Đánh giá phát âm từ file âm thanh
        /// </summary>
        /// <param name="audioFilePath">Đường dẫn đến file âm thanh</param>
        /// <param name="referenceText">Văn bản tham chiếu</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        public async Task<PronunciationAssessmentModel> AssessPronunciationFromFileAsync(string audioFilePath, string referenceText)
        {
            if (string.IsNullOrEmpty(audioFilePath))
            {
                throw new ArgumentNullException(nameof(audioFilePath), "Đường dẫn file âm thanh không được để trống");
            }
            if (string.IsNullOrEmpty(referenceText))
            {
                throw new ArgumentNullException(nameof(referenceText), "Văn bản tham chiếu không được để trống");
            }
            string errorMessage = string.Empty;
            string localFilePath = audioFilePath;
            string wavUrl = audioFilePath;

            try
            {
                // Check if file is already a wav file
                bool isWavFile = audioFilePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
                if (!isWavFile)
                {
                    var formFile = await DownloadAsIFormFileAsync(audioFilePath, CancellationToken.None);
                    var streamPart = ToStreamPart(formFile, forceWav: false);
                    wavUrl = await ConvertAndUploadAsync(streamPart);
                }

                // Bước 1: Tải file từ URL nếu input là URL
                if (Uri.TryCreate(wavUrl, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    // Tải file từ URL
                    localFilePath = Path.Combine(Path.GetTempPath(), $"audio_{Guid.NewGuid()}.wav");
                    using (var httpClient = new HttpClient())
                    using (var response = await httpClient.GetAsync(wavUrl))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                }

                _logger.LogInformation("WavUrl {WavUrl}", wavUrl);
                _logger.LogInformation("LocalFilePath {LocalFilePath}", localFilePath);
                _logger.LogInformation("ReferenceText {ReferenceText}", referenceText);
                _logger.LogInformation("SpeechConfig {0}", _speechConfig.Serialize());

                // Bước 3: Cấu hình và thực hiện đánh giá phát âm
                var pronunciationConfig = PronunciationAssessmentConfig.FromJson($"{{\"referenceText\":\"{referenceText}\",\"gradingSystem\":\"HundredMark\",\"granularity\":\"Phoneme\",\"enableSyllableLevelAssessment\":true,\"enablePhonemeLevelAssessment\":true,\"enableProsodyAssessment\":true,\"enableMiscue\":\"true\"}}");

                // Đảm bảo sử dụng IPA
                pronunciationConfig.PhonemeAlphabet = "IPA";

                if (string.IsNullOrWhiteSpace(localFilePath) || !File.Exists(localFilePath))
                {
                    _logger.LogInformation($"Audio file not found: {localFilePath}");
                }
                else
                {
                    _logger.LogInformation($"Audio file : {localFilePath}");
                }

                using var audioConfig = AudioConfig.FromWavFileInput(localFilePath);
                using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);

                // Áp dụng cấu hình
                pronunciationConfig.ApplyTo(recognizer);

                // Thêm cấu hình cho recognizer để cải thiện độ chính xác
                recognizer.Properties.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "1000");
                recognizer.Properties.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "1000");
                recognizer.Properties.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "1000");

                var result = await recognizer.RecognizeOnceAsync();
                return await ProcessPronunciationResult(result, errorMessage);
            }
            finally
            {
                // Dọn dẹp file tạm sau khi xử lý
                try
                {
                    if (localFilePath != audioFilePath && File.Exists(localFilePath))
                    {
                        _logger.LogInformation($"Đã xóa File Audio file : {localFilePath}");
                        File.Delete(localFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Lỗi khi xóa file tạm");
                }
            }
        }

        /// <summary>
        /// Xử lý kết quả đánh giá phát âm
        /// </summary>
        /// <param name="result">Kết quả nhận dạng giọng nói</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        private async Task<PronunciationAssessmentModel> ProcessPronunciationResult(SpeechRecognitionResult result, string? errorMessage = "")
        {
            var response = new PronunciationAssessmentModel
            {
                Transcription = result.Text,
                SpokenWords = new List<SpokenWord>()
            };

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                var pronunciationResult = PronunciationAssessmentResult.FromResult(result);

                response.AccuracyScore = pronunciationResult.AccuracyScore;
                response.FluencyScore = pronunciationResult.FluencyScore;
                response.ProsodyScore = pronunciationResult.ProsodyScore;

                // Tính điểm tổng hợp
                if (response.FluencyScore == 0)
                {
                    response.PronunciationScore = (int)((0.65 * response.AccuracyScore) + (0.35 * response.ProsodyScore));
                }
                else if (response.ProsodyScore == 0)
                {
                    response.PronunciationScore = (int)((0.60 * response.AccuracyScore) + (0.40 * response.FluencyScore));
                }
                else if (response.AccuracyScore != 0 && response.FluencyScore != 0 && response.ProsodyScore != 0)
                {
                    response.PronunciationScore = (int)(response.AccuracyScore * 0.5 + response.FluencyScore * 0.3 + response.ProsodyScore * 0.2);
                }

                // Xử lý từng từ
                var words = pronunciationResult.Words;
                foreach (var word in words)
                {
                    var accuracyScoreWord = word.AccuracyScore;
                    EnumSyllableMarked colorWord = GetColorBasedOnAccuracy(accuracyScoreWord);
                    var wordImprovement = new SpokenWord
                    {
                        Word = word.Word,
                        AccuracyScore = accuracyScoreWord,
                        Syllables = new List<SyllableInfo>(),
                        Color = colorWord
                    };

                    // Thêm thông tin syllables
                    if (word.Syllables != null)
                    {
                        foreach (var syllable in word.Syllables)
                        {
                            EnumSyllableMarked color = GetColorBasedOnAccuracy(syllable.AccuracyScore);
                            var syllableInfo = new SyllableInfo
                            {
                                Syllable = syllable.Syllable,
                                IPASyllable = syllable.Syllable,
                                AccuracyScore = syllable.AccuracyScore,
                                Offset = syllable.Offset,
                                Duration = syllable.Duration,
                                Grapheme = syllable.Grapheme,
                                Color = color,
                            };
                            wordImprovement.Syllables.Add(syllableInfo);
                        }
                    }

                    // Thêm thông tin phonemes với màu sắc dựa trên điểm chính xác
                    if (word.Phonemes != null)
                    {
                        wordImprovement.Phonemes = new List<PhonemeInfo>();
                        foreach (var phoneme in word.Phonemes)
                        {
                            var phonemeInfo = new PhonemeInfo
                            {
                                Phoneme = phoneme.Phoneme,
                                AccuracyScore = phoneme.AccuracyScore,
                                Offset = phoneme.Offset,
                                Duration = phoneme.Duration,
                            };
                            wordImprovement.Phonemes.Add(phonemeInfo);
                        }
                    }

                    response.SpokenWords.Add(wordImprovement);
                }
            }
            else
            {
                response.ErrorMessage = $"Không thể nhận dạng giọng nói: {result.Reason},{errorMessage}";
            }

            return response;
        }

        /// <summary>
        /// Xác định màu dựa trên điểm chính xác
        /// </summary>
        /// <param name="accuracyScore">Điểm chính xác</param>
        /// <returns>Màu tương ứng</returns>
        private static EnumSyllableMarked GetColorBasedOnAccuracy(double accuracyScore)
        {
            if (accuracyScore < LowAccuracyScore)
            {
                return EnumSyllableMarked.Red;
            }
            else if (accuracyScore < MediumAccuracyScore)
            {
                return EnumSyllableMarked.Yellow;
            }
            else
            {
                return EnumSyllableMarked.Green;
            }
        }

        /// <summary>
        /// Calls ffmpeg service to convert; streams from returned S3 URL straight into storage upload.
        /// </summary>
        private async Task<string> ConvertAndUploadAsync(StreamPart sourceFile)
        {
            // 1) Convert via ffmpeg service
            var ffmpegConvert = await _fFmpegServices.Convert(sourceFile);
            if (!ffmpegConvert.IsSuccessStatusCode || string.IsNullOrEmpty(ffmpegConvert.Content?.Paths3))
            {
                throw new InvalidOperationException("FFmpeg convert failed.");
            }
            return ffmpegConvert.Content!.Paths3;
        }

        private static string ResolveFileName(HttpContentHeaders headers, string urlFallback)
        {
            var cd = headers.ContentDisposition;
            var name = cd?.FileNameStar ?? cd?.FileName;
            name = name?.Trim('\"');

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            try
            {
                var uri = new Uri(urlFallback, UriKind.Absolute);
                var fromPath = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrWhiteSpace(fromPath))
                {
                    return fromPath;
                }
            }
            catch
            {
                // ignore
            }

            return "downloaded.bin";
        }

        public static StreamPart ToStreamPart(IFormFile file, bool forceWav = false)
        {
            if (file is null || file.Length == 0)
            {
                throw new ArgumentException("File is required.", nameof(file));
            }
            var stream = file.OpenReadStream(); // disposed by Refit after request
            var fileName = string.IsNullOrWhiteSpace(file.FileName) ? "upload.bin" : file.FileName;
            if (forceWav)
            {
                fileName = Path.ChangeExtension(fileName, ".wav");
            }
            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? (forceWav ? "audio/wav" : "application/octet-stream")
                : file.ContentType;

            return new StreamPart(stream, fileName, contentType);
        }

        public async Task<IFormFile> DownloadAsIFormFileAsync(string url, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Url is required.", nameof(url));
            }
            var client = _httpClientFactory.CreateClient();
            using var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();

            var fileName = ResolveFileName(resp.Content.Headers, url);
            var contentType = resp.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            // Disk-backed to avoid large in-memory buffers
            var tempPath = Path.Combine(Path.GetTempPath(), $"dl_{Guid.NewGuid():N}{Path.GetExtension(fileName)}");
            await using (var fs = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read,
                64 * 1024,
                FileOptions.Asynchronous))
            {
                await using var net = await resp.Content.ReadAsStreamAsync(ct);
                await net.CopyToAsync(fs, ct);
            }

            var ms = new MemoryStream(await File.ReadAllBytesAsync(tempPath, ct)); // if you MUST return IFormFile
            ms.Position = 0;

            var formFile = new FormFile(ms, 0, ms.Length, "file", fileName)
            {
                Headers = new HeaderDictionary()
            };
            formFile.Headers[HeaderNames.ContentType] = contentType;
            return formFile;
        }
    }
}
