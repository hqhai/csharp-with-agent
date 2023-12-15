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

    public class SubmitAIResponseCommand : IRequest<bool>
    {
        public string? GradingAlFeedback { get; set; }

        public ClassForumResult? ClassForumResult { get; set; }

    }

    public class SubmitAIResponseCommandHandler : IRequestHandler<SubmitAIResponseCommand, bool>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        public SubmitAIResponseCommandHandler(IClassForumResultRepository classForumResultRepository, SubmitAIResponsePublisher submitAIResponsePublisher)
        {
            _classForumResultRepository = classForumResultRepository;
            _submitAIResponsePublisher = submitAIResponsePublisher;
        }

        public async Task<bool> Handle(SubmitAIResponseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            request.ClassForumResult!.GradingAlFeedback = request.GradingAlFeedback;
            _classForumResultRepository.Add(request.ClassForumResult!);

            if (request.ClassForumResult!.Status == EnumClassForumResultStatus.Draft)
            {
                await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = request.GradingAlFeedback,
                ClassForumResultId = request.ClassForumResult!.Id,
            }, cancellationToken);

            return true;


        }
    }
}
