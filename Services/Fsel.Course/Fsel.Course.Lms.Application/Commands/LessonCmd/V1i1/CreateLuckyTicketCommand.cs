// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonCmd.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateLuckyTicketCommand : IRequest<MethodResult<VoidMethodResult>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class CreateLuckyTicketCommandHandler : IRequestHandler<CreateLuckyTicketCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly CreateLuckyTicketPublisher _createLuckyTicketPublisher;

        public CreateLuckyTicketCommandHandler(IVideoResultRepository videoResultRepository, CreateLuckyTicketPublisher createLuckyTicketPublisher)
        {
            _videoResultRepository = videoResultRepository;
            _createLuckyTicketPublisher = createLuckyTicketPublisher;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(CreateLuckyTicketCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(p => p.LessonResultId == request.LessonResultId && p.Status == EnumResultStatus.Done, cancellationToken);

            if (videoResult == null || videoResult.Percent < 65)
            {
                return methodResult;
            }

            await _createLuckyTicketPublisher.Publish(new LuckyTicketQueueModel()
            {
                LessonResultId = request.LessonResultId,
            }, cancellationToken).ConfigureAwait(false);

            return methodResult;
        }
    }
}
