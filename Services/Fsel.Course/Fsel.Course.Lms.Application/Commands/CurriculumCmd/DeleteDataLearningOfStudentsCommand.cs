// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using MediatR;

    public class DeleteDataLearningOfStudentsCommand : IRequest<MethodResult<bool>>
    {
        public Guid CourseId { get; set; }
        public IList<Guid>? UserIds { get; set; }
    }

    public class DeleteDataLearningOfStudentsCommandHandler : IRequestHandler<DeleteDataLearningOfStudentsCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;

        public DeleteDataLearningOfStudentsCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(DeleteDataLearningOfStudentsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                return methodResult;
            }

            var tasks = request.UserIds.Select(user =>
                                    _mediator.Send(new ResetCurriculumByStudentCommand
                                    {
                                        UserId = user,
                                        CourseId = request.CourseId
                                    }, cancellationToken)
                                );

            await Task.WhenAll(tasks);

            return methodResult;
        }
    }
}
