// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.AIConfigService;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SubmitTranslationAICommand : AITranslationRequestModel, IRequest<AITranslationResultModel>
    {
        /// <summary>
        /// GradingAlFeedback đã được truyền trực tiếp (không cần query lại từ DB)
        /// </summary>
        public string? GradingAlFeedback { get; set; }
    }

    public class SubmitTranslationAICommandHandler : IRequestHandler<SubmitTranslationAICommand, AITranslationResultModel>
    {
        private readonly IAIConfigSubmitService _aiConfigSubmitService;
        private readonly TranslationResultPublisher _translationResultPublisher;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;

        public SubmitTranslationAICommandHandler(IAIConfigSubmitService aiConfigSubmitService,
            TranslationResultPublisher translationResultPublisher,
            IClassForumDetailResultRepository classForumDetailResultRepository)
        {
            _aiConfigSubmitService = aiConfigSubmitService;
            _translationResultPublisher = translationResultPublisher;
            _classForumDetailResultRepository = classForumDetailResultRepository;
        }

        public async Task<AITranslationResultModel> Handle(SubmitTranslationAICommand request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = new AITranslationResultModel
            {
                ClassForumDetailResultId = request.ClassForumDetailResultId
            };

            // Lấy GradingAlFeedback từ request trước, nếu không có thì query từ DB
            string? gradingAlFeedback = request.GradingAlFeedback;
            ClassForumDetailResult? classForumDetailResult = null;

            if (string.IsNullOrEmpty(gradingAlFeedback))
            {
                classForumDetailResult = await _classForumDetailResultRepository.Queryable
                    .FirstOrDefaultAsync(x => x.Id == request.ClassForumDetailResultId, cancellationToken);

                gradingAlFeedback = classForumDetailResult?.GradingAlFeedback;
            }

            if (string.IsNullOrEmpty(gradingAlFeedback))
            {
                return new AITranslationResultModel { ClassForumDetailResultId = request.ClassForumDetailResultId };
            }

            var translatedContent = await ExecuteTranslationAsync(gradingAlFeedback, cancellationToken);
            result.TranslatedContent = translatedContent;

            // Save translated content to database (chỉ khi cần query từ DB)
            if (classForumDetailResult != null)
            {
                await UpdateClassForumDetailResultAsync(classForumDetailResult, translatedContent, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(translatedContent))
            {
                // Nếu GradingAlFeedback được truyền trực tiếp, cần query entity để update
                classForumDetailResult = await _classForumDetailResultRepository.Queryable
                    .FirstOrDefaultAsync(x => x.Id == request.ClassForumDetailResultId, cancellationToken);

                if (classForumDetailResult != null)
                {
                    await UpdateClassForumDetailResultAsync(classForumDetailResult, translatedContent, cancellationToken);
                }
            }

            await PublishTranslationResultAsync(result, cancellationToken);

            return result;
        }

        #region Private Methods

        private async Task<string?> ExecuteTranslationAsync(string gradingAlFeedback, CancellationToken cancellationToken)
        {
            return await _aiConfigSubmitService.SubmitByObjectIdAsync(
                null,
                null,
                gradingAlFeedback,
                EnumSubFeatureType.AiResponseTranslation,
                EnumFeatureMultiple.Lesson,
                cancellationToken
            );
        }

        private async Task UpdateClassForumDetailResultAsync(ClassForumDetailResult classForumDetailResult, string? translatedContent, CancellationToken cancellationToken)
        {
            classForumDetailResult.AITranslationContent = translatedContent;

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
                    c.PronunciationAlFeedback
                };
            });
        }

        private async Task PublishTranslationResultAsync(AITranslationResultModel result, CancellationToken cancellationToken)
        {
            await _translationResultPublisher.Publish(result, cancellationToken);
        }

        #endregion Private Methods
    }
}
