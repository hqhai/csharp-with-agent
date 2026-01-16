// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.WeeklyReportCommand
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public class AggregateDataWeeklyReportsCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? StudentIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AggregateDataWeeklyReportsCommandHandler : IRequestHandler<AggregateDataWeeklyReportsCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;

        public AggregateDataWeeklyReportsCommandHandler(IMediator mediator, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<bool>> Handle(AggregateDataWeeklyReportsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                methodResult.Result = true;
                return methodResult;
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 20
            };

            await Parallel.ForEachAsync(request.StudentIds, parallelOptions, async (studentId, token) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(new AggregateDataWeeklyReportCommand
                {
                    StudentIds = new List<Guid> { studentId },
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                }, token);
            });

            methodResult.Result = true;
            return methodResult;
        }
    }
}
