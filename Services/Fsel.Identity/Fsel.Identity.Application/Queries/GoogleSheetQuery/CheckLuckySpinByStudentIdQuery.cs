// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.GoogleSheetQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckLuckySpinByStudentIdQuery : IRequest<MethodResult<bool>>
    {
    }

    public class CheckLuckySpinByStudentIdQueryHandler : IRequestHandler<CheckLuckySpinByStudentIdQuery, MethodResult<bool>>
    {
        private readonly IStudentRankingEventsRepository _studentRankingEventsRepository;
        private readonly AuthContext _authContext;
        private readonly IStudentRepository _studentRepository;

        public CheckLuckySpinByStudentIdQueryHandler(IStudentRankingEventsRepository studentRankingEventsRepository, AuthContext authContext, IStudentRepository studentRepository)
        {
            _studentRankingEventsRepository = studentRankingEventsRepository;
            _authContext = authContext;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckLuckySpinByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var student = await _studentRepository.Queryable
                                        .Include(i => i.Human)
                                        .FirstOrDefaultAsync(i => i.Human != null && i.Human.UserId == _authContext.CurrentUserId, cancellationToken);
            if (student == null)
            {
                methodResult.Result = false;
                return methodResult;
            }

            var studentRankingEvents = await _studentRankingEventsRepository.Queryable.Include(x => x.CompetitionEvents).Where(p => p.StudentId == student.Id).ToListAsync(cancellationToken);
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
