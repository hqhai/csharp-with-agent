// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SubmitClassforumAICommand : IRequest<bool>
    {
        public ClassForumResult? ClassForumResult { get; set; }

        public ClassForum? ClassForum { get; set; }

        public string? WordContent { get; set; }
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
            var classForumResult = request.ClassForumResult;
            var classForum = request.ClassForum;

            if (classForumResult == null || classForum == null)
            {
                return false;
            }

            var userAiConfig = request.ClassForum!.UserAlConfig?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);
            var aIResponse = await _mediator.Send(new SubmitAICommand
            {
                SettingModel = classForum.SettingModel,
                SettingTemperature = classForum.SettingTemperature,
                SettingFrequecy = classForum.SettingFrequecy,
                SettingWordMaxLength = classForum.SettingWordMaxLength,
                SettingPresence = classForum.SettingPresence,
                SettingTopP = classForum.SettingTopP,
                SystemRoleAlConfig = classForum.SystemRoleAlConfig,
                UserAIConfig = userAiConfig
            }, cancellationToken).ConfigureAwait(false);


            classForumResult.GradingAlFeedback = aIResponse;
            classForumResult.GradingAlFeedback = aIResponse;

            _classForumResultRepository.Update(classForumResult);
            await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);


            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = aIResponse,
                ClassForumResultId = classForumResult.Id,
            }, cancellationToken);

            return true;
        }
    }
}
