// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRankingEvents

{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckStudentLuckySpinCmd : IRequest<MethodResult<bool>>
    {
    }

    public class CheckStudentLuckySpinCmdHandler : IRequestHandler<CheckStudentLuckySpinCmd, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly IStudentRankingEventsRepository _studentRankingEventsRepository;
        private readonly IStudentRepository _studentRepository;
        public CheckStudentLuckySpinCmdHandler(AuthContext authContext, IStudentRankingEventsRepository studentRankingEventsRepository, IStudentRepository studentRepository)
        {
            _authContext = authContext;
            _studentRankingEventsRepository = studentRankingEventsRepository;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckStudentLuckySpinCmd request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var student = _studentRepository.Queryable.FirstOrDefault(x => x.Human != null && x.Human.UserId == _authContext.CurrentUserId);
            var studentRankingEvents = _studentRankingEventsRepository.Queryable.Include(x => x.CompetitionEvents).Where(x => student != null && x.StudentId == student.Id).ToList();

            bool checkLuckySpin = studentRankingEvents.Any(x => x.CompetitionEvents != null && x.CompetitionEvents.EventContent != null && x.CompetitionEvents.EventContent.LuckySpin);

            methodResult.Result = checkLuckySpin;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
