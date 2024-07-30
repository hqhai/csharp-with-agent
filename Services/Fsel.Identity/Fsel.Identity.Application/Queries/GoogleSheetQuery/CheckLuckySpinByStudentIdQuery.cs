// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.GoogleSheetQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckLuckySpinByStudentIdQuery : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
    }

    public class CheckLuckySpinByStudentIdQueryHandler : IRequestHandler<CheckLuckySpinByStudentIdQuery, MethodResult<bool>>
    {
        private readonly IStudentRankingEventsRepository _studentRankingEventsRepository;

        public CheckLuckySpinByStudentIdQueryHandler(IStudentRankingEventsRepository studentRankingEventsRepository)
        {
            _studentRankingEventsRepository = studentRankingEventsRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckLuckySpinByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentRankingEvents = await _studentRankingEventsRepository.Queryable.Include(x => x.CompetitionEvents).Where(p => p.StudentId == request.StudentId).ToListAsync(cancellationToken);
            if (studentRankingEvents == null)
            {
                methodResult.Result = false;
                return methodResult;
            }
            methodResult.Result = studentRankingEvents.Any(p => p.CompetitionEvents != null && p.CompetitionEvents.EventContent != null && p.CompetitionEvents.EventContent.LuckySpin);
            return methodResult;
        }
    }
}
