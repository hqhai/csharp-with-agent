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

    public class SubmitMockTestAnswerCommand : MockTestAnswerResponseModel, IRequest<bool>
    {
    }

    public class SubmitMockTestAnswerCommandHandler : IRequestHandler<SubmitMockTestAnswerCommand, bool>
    {
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly IMediator _mediator;
        public SubmitMockTestAnswerCommandHandler(SubmitAIResponsePublisher submitAIResponsePublisher, IMediator mediator, IMockTestAnswerRepository mockTestAnswerRepository)
        {
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _mediator = mediator;
            _mockTestAnswerRepository = mockTestAnswerRepository;
        }

        public async Task<bool> Handle(SubmitMockTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var mockTestAnswer =  _mockTestAnswerRepository.Queryable.FirstOrDefault(x => x.SectionId == request.SectionId);

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


            if (mockTestAnswer != null)
            {

                mockTestAnswer.GradingAlFeedback = aIResponse;

                _mockTestAnswerRepository.Update(mockTestAnswer);
                await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = aIResponse,
            }, cancellationToken);

            return true;
        }
    }
}
