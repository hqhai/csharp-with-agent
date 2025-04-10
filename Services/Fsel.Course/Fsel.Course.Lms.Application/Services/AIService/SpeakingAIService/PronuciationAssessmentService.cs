using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
using Fsel.Course.Infrastructure.ValueSettings;
using Fsel.Shared.Enums;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService
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

        /// <summary>
        /// Khởi tạo dịch vụ đánh giá phát âm với Azure
        /// </summary>
        /// <param name="subscriptionKey">Khóa đăng ký của Azure</param>
        /// <param name="region">Khu vực của dịch vụ</param>
        public PronuciationAssessmentService(AppSetting appSetting)
        {
            _appSetting = appSetting;
            _speechConfig = SpeechConfig.FromSubscription(_appSetting!.AzureAiConfig!.SecondApiKey, _appSetting!.AzureAiConfig!.Location);
            _speechConfig.SpeechRecognitionLanguage = _appSetting!.AzureAiConfig!.SpeechRecognitionLanguage;
        }

        /// <summary>
        /// Đánh giá phát âm từ microphone
        /// </summary>
        /// <param name="referenceText">Văn bản tham chiếu</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        public async Task<PronunciationAssessmentModel> AssessPronunciationFromMicrophoneAsync(string referenceText)
        {
            if (string.IsNullOrEmpty(referenceText))
                throw new ArgumentNullException(nameof(referenceText), "Văn bản tham chiếu không được để trống");

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
                throw new ArgumentNullException(nameof(audioFilePath), "Đường dẫn file âm thanh không được để trống");

            if (string.IsNullOrEmpty(referenceText))
                throw new ArgumentNullException(nameof(referenceText), "Văn bản tham chiếu không được để trống");

            try
            {
                string localFilePath = audioFilePath;

                // Kiểm tra nếu là URL thì tải file về máy
                if (Uri.TryCreate(audioFilePath, UriKind.Absolute, out Uri? uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    // Tải file từ URL
                    localFilePath = Path.Combine(Path.GetTempPath(), $"audio_{Guid.NewGuid()}.wav");

                    using (var httpClient = new HttpClient())
                    {
                        using (var response = await httpClient.GetAsync(audioFilePath))
                        {
                            response.EnsureSuccessStatusCode();
                            using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await response.Content.CopyToAsync(fileStream);
                            }
                        }
                    }
                }
                else if (!File.Exists(localFilePath))
                {
                    throw new FileNotFoundException("Không tìm thấy file âm thanh", audioFilePath);
                }

                try
                {
                    var pronunciationConfig = PronunciationAssessmentConfig.FromJson($"{{\"referenceText\":\"{referenceText}\",\"gradingSystem\":\"HundredMark\",\"granularity\":\"Phoneme\",\"enableSyllableLevelAssessment\":true,\"enablePhonemeLevelAssessment\":true,\"enableProsodyAssessment\":true,\"enableMiscue\":\"true\"}}");
                    pronunciationConfig.PhonemeAlphabet = "IPA";

                    // var pronunciationConfig = new PronunciationAssessmentConfig(referenceText, GradingSystem.HundredMark, Granularity.Phoneme, true);

                    using var audioConfig = AudioConfig.FromWavFileInput(localFilePath);
                    using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);

                    pronunciationConfig.ApplyTo(recognizer);
                    var result = await recognizer.RecognizeOnceAsync();

                    return await ProcessPronunciationResult(result);
                }
                finally
                {
                    // Nếu là file tạm từ URL, xóa sau khi xử lý
                    if (localFilePath != audioFilePath && File.Exists(localFilePath))
                    {
                        File.Delete(localFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                return new PronunciationAssessmentModel
                {
                    ErrorMessage = $"Lỗi khi đánh giá phát âm: {ex.Message}",
                    SpokenWords = new List<SpokenWord>()
                };
            }
        }


        /// <summary>
        /// Xử lý kết quả đánh giá phát âm
        /// </summary>
        /// <param name="result">Kết quả nhận dạng giọng nói</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        private async Task<PronunciationAssessmentModel> ProcessPronunciationResult(SpeechRecognitionResult result)
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
                    var wordImprovement = new SpokenWord
                    {
                        Word = word.Word,
                        AccuracyScore = word.AccuracyScore,
                        Syllables = new List<SyllableInfo>()
                    };

                    // Thêm thông tin syllables
                    if (word.Syllables != null)
                    {
                        foreach (var syllable in word.Syllables)
                        {
                            var syllableInfo = new SyllableInfo
                            {
                                Syllable = syllable.Syllable,
                                IPASyllable = syllable.Syllable,
                                AccuracyScore = syllable.AccuracyScore,
                                Offset = syllable.Offset,
                                Duration = syllable.Duration
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
                            var accuracyScore = phoneme.AccuracyScore;
                            var color = GetColorBasedOnAccuracy(accuracyScore);

                            var phonemeInfo = new PhonemeInfo
                            {
                                Phoneme = phoneme.Phoneme,
                                AccuracyScore = accuracyScore,
                                Offset = phoneme.Offset,
                                Duration = phoneme.Duration,
                                Color = color
                            };
                            wordImprovement.Phonemes.Add(phonemeInfo);
                        }
                    }

                    response.SpokenWords.Add(wordImprovement);
                }
            }
            else
            {
                response.ErrorMessage = $"Không thể nhận dạng giọng nói: {result.Reason}";
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
