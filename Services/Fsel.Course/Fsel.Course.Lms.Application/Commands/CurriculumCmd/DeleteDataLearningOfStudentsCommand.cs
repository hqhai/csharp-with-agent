// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public class DeleteDataLearningOfStudentsCommand : IRequest<MethodResult<bool>>
    {
        public Guid CourseId { get; set; }
        public IList<Guid>? UserIds { get; set; }
    }

    public class DeleteDataLearningOfStudentsCommandHandler : IRequestHandler<DeleteDataLearningOfStudentsCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;

        public DeleteDataLearningOfStudentsCommandHandler(IMediator mediator, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<bool>> Handle(DeleteDataLearningOfStudentsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                return methodResult;
            }

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
            await Parallel.ForEachAsync(request.UserIds, parallelOptions, async (userId, token) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(new ResetCurriculumByStudentCommand
                {
                    UserId = userId,
                    CourseId = request.CourseId
                }, token);
            });

            return methodResult;
        }
    }
}
