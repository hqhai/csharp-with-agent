// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SubmitMockTestAnswerAICommand : MockTestAnswerResponseModel, IRequest<bool>
    {
    }

    public class SubmitMockTestAnswerCommandHandler : IRequestHandler<SubmitMockTestAnswerAICommand, bool>
    {
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly IMockTestAISettingRepository _aiGradeSettingRepository;
        private readonly IMediator _mediator;
        public SubmitMockTestAnswerCommandHandler(SubmitAIResponsePublisher submitAIResponsePublisher, IMediator mediator, IMockTestAnswerRepository mockTestAnswerRepository, IMockTestAISettingRepository aiGradeSettingRepository)
        {
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _mediator = mediator;
            _mockTestAnswerRepository = mockTestAnswerRepository;
            _aiGradeSettingRepository = aiGradeSettingRepository;
        }

        public async Task<bool> Handle(SubmitMockTestAnswerAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var mockTestAnswer = _mockTestAnswerRepository.Queryable.FirstOrDefault(x => x.SectionId == request.ObjectId);

            var aiConfig = _aiGradeSettingRepository.Queryable.FirstOrDefault(x => x.ObjectId == request.ObjectId);


            var resultDictionary = new Dictionary<EnumMockTestAIType, string>();

            foreach (var item in aiConfig!.Prompts!)
            {
                //var answer = string.Concat(aiConfig.Task!, item.PromptContent!);

                var answer = string.Concat(new string[] { aiConfig.Task!, Environment.NewLine, item.PromptContent! });

                var userAiConfig = answer?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);

                if (userAiConfig == null)
                {
                    return false;
                }

                var aIResponse = await _mediator.Send(new SubmitAICommand
                {
                    SettingModel = aiConfig?.SettingModel,
                    SettingTemperature = aiConfig!.SettingTemperature,
                    SettingFrequecy = aiConfig!.SettingFrequecy,
                    SettingWordMaxLength = aiConfig!.SettingWordMaxLength,
                    SettingPresence = aiConfig!.SettingPresence,
                    SettingTopP = aiConfig!.SettingTopP,
                    SystemRoleAlConfig = aiConfig!.SystemRoleAlConfig,
                    UserAIConfig = userAiConfig,
                }, cancellationToken).ConfigureAwait(false);

                resultDictionary[item.Type] = aIResponse!;
            }

            var gradingAiFeedBackResult = new
            {
                TaskResponse = resultDictionary[EnumMockTestAIType.TaskResponse],
                Coherence = resultDictionary[EnumMockTestAIType.Coherence],
                LexicalResource = resultDictionary[EnumMockTestAIType.LexicalResource],
                GrammaticalRange = resultDictionary[EnumMockTestAIType.GrammaticalRange]
            };

            string? gradingAiFeedBack = ConvertHelper.Serialize(gradingAiFeedBackResult);

            if (mockTestAnswer != null)
            {
                mockTestAnswer.GradingAlFeedback = gradingAiFeedBack;

                _mockTestAnswerRepository.Update(mockTestAnswer);
                await _mockTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = gradingAiFeedBack,
            }, cancellationToken);

            return true;
        }
    }
}
