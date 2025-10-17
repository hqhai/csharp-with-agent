// Copyright (c) Atlantic. All rights reserved.

// File này đã được thay thế bởi PronunciationAssessmentCmd.cs
// Vui lòng sử dụng PronunciationAssessmentCmd thay vì AzurePronTestCmd để đánh giá phát âm

namespace Fsel.ExamPractice.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Models.EntityModels;
    using Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService.Interface;
    using MediatR;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Command để đánh giá phát âm từ file âm thanh
    /// </summary>
    public class AzurePronTestCmd : IRequest<MethodResult<PronunciationAssessmentModel>>
    {
        /// <summary>
        /// Đường dẫn hoặc URL tới file âm thanh
        /// </summary>
        public string? AudioPath { get; set; }

        /// <summary>
        /// Văn bản tham chiếu để đánh giá phát âm
        /// </summary>
        public string? TranscriptionText { get; set; }
    }

    /// <summary>
    /// Handler xử lý command đánh giá phát âm
    /// </summary>
    public class AzurePronTestCmdHandler :
        IRequestHandler<AzurePronTestCmd, MethodResult<PronunciationAssessmentModel>>
    {
        private readonly ILogger<AzurePronTestCmdHandler> _logger;
        private readonly IPronuciationAssessmentService _pronuciationService;

        /// <summary>
        /// Khởi tạo handler
        /// </summary>
        public AzurePronTestCmdHandler(
            ILogger<AzurePronTestCmdHandler> logger,
            IPronuciationAssessmentService pronuciationService)
        {
            _logger = logger;
            _pronuciationService = pronuciationService;
        }

        /// <summary>
        /// Xử lý command đánh giá phát âm
        /// </summary>
        public async Task<MethodResult<PronunciationAssessmentModel>> Handle(
            AzurePronTestCmd request,
            CancellationToken cancellationToken)
        {
            MethodResult<PronunciationAssessmentModel> methodResult = new MethodResult<PronunciationAssessmentModel>();
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrEmpty(request.AudioPath) || string.IsNullOrEmpty(request.TranscriptionText))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.AudioPath), request.AudioPath);
                return methodResult;
            }

            try
            {
                // Kiểm tra định dạng file và chuyển đổi nếu cần
                string extension = Path.GetExtension(request.AudioPath).ToLower(CultureInfo.InvariantCulture);
                try
                {
                    _logger.LogInformation("Đang đánh giá phát âm cho file: {FilePath}", request.AudioPath);
                    var response = await _pronuciationService.AssessPronunciationFromFileAsync(request.AudioPath, request.TranscriptionText);

                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        _logger.LogError("Lỗi đánh giá phát âm: {Error}", response.ErrorMessage);
                    }

                    var result = response;
                    methodResult.Result = result;
                    return methodResult;
                }
                finally
                {
                    // Dọn dẹp các file tạm nếu cần
                    if (Uri.TryCreate(request.AudioPath, UriKind.Absolute, out _) &&
                        File.Exists(request.AudioPath))
                    {
                        _logger.LogInformation("Đang xóa file tạm: {FilePath}", request.AudioPath);
                        File.Delete(request.AudioPath);
                    }

                    if (File.Exists(request.AudioPath))
                    {
                        _logger.LogInformation("Đang xóa file WAV tạm: {FilePath}", request.AudioPath);
                        File.Delete(request.AudioPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh giá phát âm: {Message}", ex.Message);
            }

            return methodResult;
        }
    }
}
