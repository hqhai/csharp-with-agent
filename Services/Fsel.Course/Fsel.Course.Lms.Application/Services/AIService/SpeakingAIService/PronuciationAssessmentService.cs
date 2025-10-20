using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.ValueSettings;
using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
using Fsel.Shared.Enums;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.Extensions.Logging;

namespace Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService
{
    /// <summary>
    /// Dịch vụ xử lý giọng nói sử dụng Azure Cognitive Services
    /// </summary>
    public class PronuciationAssessmentService : IPronuciationAssessmentService
    {
        private SpeechConfig _speechConfig;
        private readonly AppSetting _appSetting;
        private readonly ILogger<PronuciationAssessmentService> _logger;
        private const int LowAccuracyScore = 60;
        private const int MediumAccuracyScore = 80;

        /// <summary>
        /// Khởi tạo dịch vụ đánh giá phát âm với Azure
        /// </summary>
        /// <param name="subscriptionKey">Khóa đăng ký của Azure</param>
        /// <param name="region">Khu vực của dịch vụ</param>
        public PronuciationAssessmentService(AppSetting appSetting, ILogger<PronuciationAssessmentService> logger)
        {
            _appSetting = appSetting;
            _logger = logger;
            _speechConfig = SpeechConfig.FromSubscription(_appSetting!.AzureAiConfig!.SecondApiKey, _appSetting!.AzureAiConfig!.Location);
            _speechConfig.SpeechRecognitionLanguage = _appSetting!.AzureAiConfig!.SpeechRecognitionLanguage;

            // Thêm cấu hình cho speech config
            _speechConfig.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "1000");
            _speechConfig.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "1000");
            _speechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "1000");
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
        /// #FSEL-5683 , Issue File
        /// Kiểm tra và chuyển đổi file âm thanh sang WAV PCM nếu cần
        /// Xử lý lỗi SPXERR_INVALID_HEADER (0xa) của Azure Speech Services
        /// </summary>
        /// <param name="inputFile">Đường dẫn file âm thanh đầu vào</param>
        /// <returns>Đường dẫn đến file WAV PCM (có thể là file gốc hoặc file đã chuyển đổi)</returns>
        private async Task<string> EnsureWavPcmFormat(string inputFile)
        {
            // Tạo tên file output nếu cần chuyển đổi
            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!, $"converted_{Path.GetFileName(inputFile)}");

            try
            {
                // Kiểm tra header của file WAV để xác định có phải WAV PCM không
                using (var reader = new BinaryReader(File.OpenRead(inputFile)))
                {
                    // Đọc RIFF header - 4 bytes đầu tiên phải là "RIFF"
                    var riff = new string(reader.ReadChars(4));
                    var size = reader.ReadInt32();  // Kích thước file
                    var wave = new string(reader.ReadChars(4));  // Phải là "WAVE"

                    // Đọc fmt chunk - chứa thông tin về format
                    var fmt = new string(reader.ReadChars(4));  // Phải là "fmt "
                    var fmtSize = reader.ReadInt32();  // Kích thước của fmt chunk
                    var format = reader.ReadInt16();   // 1 = PCM
                    var channels = reader.ReadInt16(); // 1 = mono
                    var sampleRate = reader.ReadInt32();  // 16000 = 16kHz
                    var byteRate = reader.ReadInt32();    // Tốc độ byte
                    var blockAlign = reader.ReadInt16();  // Căn chỉnh block
                    var bitsPerSample = reader.ReadInt16();  // 16 = 16-bit

                    // Kiểm tra tất cả điều kiện của WAV PCM format
                    // - RIFF header
                    // - WAVE format
                    // - PCM format (1)
                    // - Mono channel
                    // - 16kHz sample rate
                    // - 16-bit per sample
                    if (riff == "RIFF" && wave == "WAVE" && format == 1
                        && channels == 1 && sampleRate == 16000 && bitsPerSample == 16)
                    {
                        // File đã đúng định dạng, không cần chuyển đổi
                        return inputFile;
                    }
                }
            }
            catch
            {
                // Nếu không đọc được header hoặc không phải file WAV
                // Tiếp tục để chuyển đổi file
            }

            // Chuyển đổi sang WAV PCM với FFmpeg
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ffmpeg",  // Đảm bảo FFmpeg đã được cài đặt
                    // Các tham số chuyển đổi:
                    // -y: Tự động ghi đè file output
                    // -i: File input
                    // -acodec pcm_s16le: Chuyển sang PCM 16-bit
                    // -ar 16000: Sample rate 16kHz
                    // -ac 1: 1 channel (mono)
                    Arguments = $"-y -i \"{inputFile}\" -acodec pcm_s16le -ar 16000 -ac 1 \"{outputFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _logger.LogInformation("Đang chuyển đổi file âm thanh sang WAV PCM: {InputFile}", inputFile);

                process.Start();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Lỗi khi chuyển đổi với FFmpeg: {Error}", error);
                    throw new InvalidOperationException($"Không thể chuyển đổi file âm thanh sang định dạng WAV PCM. Chi tiết: {error}");
                }
            }

            return outputFile;
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
                throw new ArgumentNullException(nameof(audioFilePath), "Đường dẫn file âm thanh không được để trống");

            if (string.IsNullOrEmpty(referenceText))
                throw new ArgumentNullException(nameof(referenceText), "Văn bản tham chiếu không được để trống");

            string errorMessage = string.Empty;
            string? convertedFilePath = null;
            string localFilePath = audioFilePath;

            try
            {
                // Bước 1: Tải file từ URL nếu input là URL
                if (Uri.TryCreate(audioFilePath, UriKind.Absolute, out Uri? uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    // Tải file từ URL
                    localFilePath = Path.Combine(Path.GetTempPath(), $"audio_{Guid.NewGuid()}.wav");
                    using (var httpClient = new HttpClient())
                    using (var response = await httpClient.GetAsync(audioFilePath))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                }

                // Bước 2: Đảm bảo file là WAV PCM format
                // #FSEL-5683 , Issue File
                try
                {
                    convertedFilePath = await EnsureWavPcmFormat(localFilePath);
                    if (convertedFilePath != localFilePath)
                    {
                        localFilePath = convertedFilePath;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý file âm thanh");
                    throw new InvalidOperationException("File âm thanh không hợp lệ hoặc không thể chuyển đổi sang định dạng WAV PCM", ex);
                }

                // Bước 3: Cấu hình và thực hiện đánh giá phát âm
                var pronunciationConfig = PronunciationAssessmentConfig.FromJson($"{{\"referenceText\":\"{referenceText}\",\"gradingSystem\":\"HundredMark\",\"granularity\":\"Phoneme\",\"enableSyllableLevelAssessment\":true,\"enablePhonemeLevelAssessment\":true,\"enableProsodyAssessment\":true,\"enableMiscue\":\"true\"}}");

                // Đảm bảo sử dụng IPA
                pronunciationConfig.PhonemeAlphabet = "IPA";

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
                        File.Delete(localFilePath);
                    }
                    if (convertedFilePath != null && convertedFilePath != localFilePath && File.Exists(convertedFilePath))
                    {
                        File.Delete(convertedFilePath);
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
    }
}
