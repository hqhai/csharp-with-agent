// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SubmitClassforumAICommand : ClassForumAIResponseModel, IRequest<bool>
    {
    }

    public class SubmitAIResponseCommandHandler : IRequestHandler<SubmitClassforumAICommand, bool>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly IMediator _mediator;

        public SubmitAIResponseCommandHandler(IClassForumResultRepository classForumResultRepository, SubmitAIResponsePublisher submitAIResponsePublisher, IMediator mediator)
        {
            _classForumResultRepository = classForumResultRepository;
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _mediator = mediator;
        }

        public async Task<bool> Handle(SubmitClassforumAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var classForumResult = await _classForumResultRepository.GetByIdAsync(request.ClassForumResultId);
            var userAiConfig = request!.UserAIConfig?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);
            var aIResponse = await _mediator.Send(new SubmitAICommand
            {
                SettingModel = request.SettingModel,
                SettingTemperature = request.SettingTemperature,
                SettingFrequecy = request.SettingFrequecy,
                SettingWordMaxLength = request.SettingWordMaxLength,
                SettingPresence = request.SettingPresence,
                SettingTopP = request.SettingTopP,
                SystemRoleAlConfig = request.SystemRoleAlConfig,
                UserAIConfig = userAiConfig,
            }, cancellationToken).ConfigureAwait(false);

            if (classForumResult != null)
            {
                if (request.IsRetry != null && (bool)request.IsRetry)
                {
                    classForumResult.RetryGradingAlFeedBack = aIResponse;
                }
                else
                {
                    classForumResult.GradingAlFeedback = aIResponse;
                }

                _classForumResultRepository.Update(classForumResult);
                await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = aIResponse,
                ClassForumResultId = request.ClassForumResultId,
            }, cancellationToken);

            return true;
        }
    }
}
