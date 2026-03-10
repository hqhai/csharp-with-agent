// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;
    using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class SpeakingEvaluationAIService : ISpeakingEvaluationAIService
    {
        private readonly AppSetting _appSetting;
        private readonly ISystemFileProvider _systemFileProvider;
        private readonly ILogger<SpeakingEvaluationAIService> _logger;
        private readonly AuthContext _authContext;
        private const string LanguageUS = "en-US";

        public SpeakingEvaluationAIService(AppSetting appSetting,
            ISystemFileProvider systemFileProvider,
            ILogger<SpeakingEvaluationAIService> logger,
            AuthContext authContext)
        {
            _appSetting = appSetting;
            _systemFileProvider = systemFileProvider;
            _logger = logger;
            _authContext = authContext;
        }

        public async Task<double> EvaluationSpeaking(string? question, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return 0;
            }
            // Creates an instance of a speech config with specified subscription key and service region.
            var config = SpeechConfig.FromSubscription(_appSetting.AzureAiConfig?.SecondApiKey, _appSetting.AzureAiConfig?.Location);
            string topic = question ?? string.Empty;
            //url = "https://s3-sgn10.fptcloud.com/fsel-public/Videos/What_is_your_name_1716804591.mp3";

            // Download the file
            var localPath = _systemFileProvider.TransformFileFromUrl(url, ".wav");

            // Create AudioConfig from local file path
            var audioConfig = AudioConfig.FromWavFileInput(localPath);
            var speechRecognizer = new SpeechRecognizer(config, LanguageUS.Replace("_", "-"), audioConfig);

            var connection = Connection.FromRecognizer(speechRecognizer);

            var phraseDetectionConfig = new
            {
                enrichment = new
                {
                    pronunciationAssessment = new
                    {
                        referenceText = "",
                        gradingSystem = "HundredMark",
                        granularity = "Word",
                        dimension = "Comprehensive",
                        enableMiscue = "False"
                    },
                    contentAssessment = new
                    {
                        topic = topic
                    }
                }
            };
            connection.SetMessageProperty("speech.context", "phraseDetection", JsonConvert.SerializeObject(phraseDetectionConfig));

            var phraseOutputConfig = new
            {
                format = "Detailed",
                detailed = new
                {
                    options = new[]
                    {
                        "WordTimings",
                        "PronunciationAssessment",
                        "ContentAssessment",
                        "SNR",
                    }
                }
            };
            connection.SetMessageProperty("speech.context", "phraseOutput", JsonConvert.SerializeObject(phraseOutputConfig));

            var done = false;
            var fullRecognizedText = "";

            speechRecognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine("ClOSING on {0}", e);
                done = true;
            };

            speechRecognizer.Canceled += (s, e) =>
            {
                Console.WriteLine("ClOSING on {0}", e);
                done = true;
            };
            PronunciationAssessment pronunciationScore = new PronunciationAssessment();
            connection.MessageReceived += (s, e) =>
            {
                if (e.Message.IsTextMessage())
                {
                    var messageText = e.Message.GetTextMessage();
                    var json = Newtonsoft.Json.Linq.JObject.Parse(messageText);
                    if (json.ContainsKey("NBest"))
                    {
                        string pronunciationScoreStr = json["NBest"]?[0]?["PronunciationAssessment"]?.ToString()?.Trim() ?? string.Empty;

                        if (!string.IsNullOrEmpty(pronunciationScoreStr))
                        {
                            pronunciationScore = ConvertHelper.Deserialize<PronunciationAssessment>(pronunciationScoreStr);
                        }
                    }
                }
            };

            // Starts continuous recognition.
            await speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            while (!done)
            {
                // Allow the program to run and process results continuously.
                await Task.Delay(1000); // Adjust the delay as needed.
            }

            await speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

            // Ensure audioConfig and speechRecognizer are disposed properly
            audioConfig.Dispose();
            speechRecognizer.Dispose();

            // Remove the file
            await RemoveFile(localPath);

            return pronunciationScore.PronScore;
        }

        public async Task<double> EvaluationSpeakingV1(string? question, string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return ValueSettings.ValueDefault;
            }
            var config = SpeechConfig.FromSubscription(
                _appSetting.AzureAiConfig?.SecondApiKey,
                _appSetting.AzureAiConfig?.Location
            );
            config.SpeechRecognitionLanguage = _appSetting.AzureAiConfig?.SpeechRecognitionLanguage ?? LanguageUS;

            var listPronScore = new List<double>();
            var topic = question ?? string.Empty;
            var localPath = _systemFileProvider.TransformFileFromUrl(url, ".wav");

            try
            {
                using var audioConfig = AudioConfig.FromWavFileInput(localPath);
                using var recognizer = new SpeechRecognizer(config, audioConfig);

                var pronConfig = new PronunciationAssessmentConfig(
                    referenceText: string.Empty,
                    gradingSystem: GradingSystem.HundredMark,
                    granularity: Granularity.Word,
                    enableMiscue: false
                );
                pronConfig.EnableProsodyAssessment();
                pronConfig.ApplyTo(recognizer);

                var stopRecognition = new TaskCompletionSource<int>();

                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        var pronResult = PronunciationAssessmentResult.FromResult(e.Result);
                        if (pronResult != null && pronResult.Words.Any())
                        {
                            listPronScore.Add(pronResult.PronunciationScore);
                        }
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        _logger.LoggerRequest($"Code={nameof(EnumOtherErrorCode.NoMatch)} | Question='{question}' | URL='{url}' | UserId='{_authContext.CurrentUserId}'");
                    }
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    stopRecognition.TrySetResult(ValueSettings.ValueDefault);
                };

                recognizer.Canceled += (s, e) =>
                {
                    stopRecognition.TrySetResult(ValueSettings.ValueDefault);
                };

                await recognizer.StartContinuousRecognitionAsync();
                await stopRecognition.Task;
                await recognizer.StopContinuousRecognitionAsync();

                return listPronScore.Any() ? listPronScore.Average() : ValueSettings.ValueDefault;
            }
            catch (Exception ex)
            {
                _logger.LoggerRequest($"Code={nameof(EnumOtherErrorCode.EvaluationSpeaking)} | Question='{question}' | URL={url} | Ex={ex.Message} | UserId='{_authContext.CurrentUserId}'");
                return ValueSettings.ValueDefault;
            }
            finally
            {
                await RemoveFile(localPath);
            }
        }

        private async Task RemoveFile(string filePath)
        {
            _systemFileProvider.DeleteFiles(filePath);
        }

        private async Task<string> ConvertToWavIfNeeded(string filePath)
        {
            var wavFilePath = Path.ChangeExtension(filePath, ".wav");

            if (Path.GetExtension(filePath).ToLower() != ".wav")
            {
                var ffmpegPath = "ffmpeg"; // Ensure ffmpeg is in the system PATH
                var startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-i \"{filePath}\" \"{wavFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"FFmpeg conversion failed: {error}");
                    }
                }
            }
            else
            {
                wavFilePath = filePath;
            }

            return wavFilePath;
        }
    }

    public class PronunciationAssessment
    {
        public double AccuracyScore { get; set; }
        public double FluencyScore { get; set; }
        public double CompletenessScore { get; set; }
        public double PronScore { get; set; }
    }
}
