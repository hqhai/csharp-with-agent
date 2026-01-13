// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class ClassForumPronunciationCommand : IRequest<MethodResult<bool>>
    {
        public Guid ClassForumDetailResultId { get; set; }
    }

    public class ClassForumPronunciationCommandHandler : IRequestHandler<ClassForumPronunciationCommand, MethodResult<bool>>
    {
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IPronuciationAssessmentService _pronuciationService;
        private readonly IContinuousPronunciationAssessmentService _continuousPronuciationService;
        private readonly ILogger<ClassForumPronunciationCommand> _logger;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;

        public ClassForumPronunciationCommandHandler(IClassForumDetailResultRepository classForumDetailResultRepository,
                                                     IPronuciationAssessmentService pronuciationService,
                                                     ILogger<ClassForumPronunciationCommand> logger,
                                                     SubmitAIResponsePublisher submitAIResponsePublisher,
                                                     IContinuousPronunciationAssessmentService continuousPronuciationService)
        {
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _pronuciationService = pronuciationService;
            _logger = logger;
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _continuousPronuciationService = continuousPronuciationService;
        }

        public async Task<MethodResult<bool>> Handle(ClassForumPronunciationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classForumDetailResult = await _classForumDetailResultRepository.Queryable
                                                                                .Include(x => x.ClassForumResultFiles)
                                                                                .FirstOrDefaultAsync(x => x.Id == request.ClassForumDetailResultId, cancellationToken);
            if (classForumDetailResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var filePath = classForumDetailResult.ClassForumResultFiles.FirstOrDefault()?.FilePath;
            var workContent = classForumDetailResult.WordContent;

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(workContent))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(filePath), filePath);
                return methodResult;
            }

            try
            {

                // Kiểm tra định dạng file và chuyển đổi nếu cần
                string extension = Path.GetExtension(filePath).ToLower(CultureInfo.InvariantCulture);
                try
                {
                    _logger.LogInformation("Đang đánh giá phát âm cho file: {FilePath}", filePath);
                    var response = await _continuousPronuciationService.AssessPronunciationFromFileContinuousAsync(filePath, workContent);

                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        _logger.LogError("Lỗi đánh giá phát âm: {Error}", response.ErrorMessage);
                    }

                    await _classForumDetailResultRepository.ExecuteTransactionAsync(async () =>
                    {
                        classForumDetailResult.PronunciationAlFeedback = ConvertHelper.Serialize(response);
                        await _classForumDetailResultRepository.BulkUpdateList(new List<ClassForumDetailResult> { classForumDetailResult }, bulk =>
                        {
                            bulk.IgnoreOnUpdateExpression = c => new
                            {
                                c.WordContent,
                                c.Content,
                                c.WordCount,
                                c.SubmissionCount,
                                c.ProcessDate,
                                c.CompletionDate,
                                c.Status,
                                c.ClassForumResultId,
                                c.GradingAlFeedback
                            };
                        });
                        methodResult.Result = true;
                        return methodResult;
                    });


                    await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
                    {
                        PronunciationAlFeedback = ConvertHelper.Serialize(response),
                        ClassForumResultId = classForumDetailResult.ClassForumResultId,
                        EnumSubmissionCount = classForumDetailResult.SubmissionCount ?? EnumSubmissionCount.FirstSubmit,
                        PronunciationScore = classForumDetailResult.PronunciationScore,
                        CorrectTotal = classForumDetailResult.CorrectTotal,
                        CorrectCount = classForumDetailResult.CorrectCount
                    }, cancellationToken);

                    return methodResult;
                }
                finally
                {
                    // Dọn dẹp các file tạm nếu cần
                    if (Uri.TryCreate(filePath, UriKind.Absolute, out _) &&
                        File.Exists(filePath))
                    {
                        _logger.LogInformation("Đang xóa file tạm: {FilePath}", filePath);
                        File.Delete(filePath);
                    }

                    if (File.Exists(filePath))
                    {
                        _logger.LogInformation("Đang xóa file WAV tạm: {FilePath}", filePath);
                        File.Delete(filePath);
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
