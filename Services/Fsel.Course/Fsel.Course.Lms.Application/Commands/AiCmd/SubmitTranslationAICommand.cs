// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.AIConfigService;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SubmitTranslationAICommand : AITranslationRequestModel, IRequest<AITranslationResultModel>
    {
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

            var translatedContent = await ExecuteTranslationAsync(request, cancellationToken);
            result.TranslatedContent = translatedContent;

            // Save translated content to database
            await UpdateClassForumDetailResultAsync(request.ClassForumDetailResultId, translatedContent, cancellationToken);

            await PublishTranslationResultAsync(result, cancellationToken);

            return result;
        }

        #region Private Methods

        private async Task<string?> ExecuteTranslationAsync(SubmitTranslationAICommand request, CancellationToken cancellationToken)
        {
            return await _aiConfigSubmitService.SubmitByObjectIdAsync(
                null,
                request.AiResponseContent ?? string.Empty,
                EnumSubFeatureType.AiResponseTranslation,
                EnumFeatureMultiple.Lesson,
                cancellationToken
            );
        }

        private async Task UpdateClassForumDetailResultAsync(Guid classForumDetailResultId, string? translatedContent, CancellationToken cancellationToken)
        {
            var classForumDetailResult = await _classForumDetailResultRepository.Queryable
                .FirstOrDefaultAsync(x => x.Id == classForumDetailResultId, cancellationToken);

            if (classForumDetailResult != null)
            {
                classForumDetailResult.AITranslationContent = translatedContent;
                _classForumDetailResultRepository.Update(classForumDetailResult);
                await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task PublishTranslationResultAsync(AITranslationResultModel result, CancellationToken cancellationToken)
        {
            await _translationResultPublisher.Publish(result, cancellationToken);
        }

        #endregion
    }
}
