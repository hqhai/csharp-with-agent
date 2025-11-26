// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentAggregateQuery
{
    using Common.ActionResults;
    using Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Services.UserServices;

    public class GetClassCampusQuery : IRequest<MethodResult<IList<string>>>
    {

    }

    public class GetClassCampusQueryHandler : IRequestHandler<GetClassCampusQuery, MethodResult<IList<string>>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly  IUserService _userService;

        public GetClassCampusQueryHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,  IUserService userService)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<string>>> Handle(GetClassCampusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<string>>();

            var results = await _studentGoalAggregateRepository.ReadQueryable
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var studentIds = results.Select(l => l.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result?.Select(x => x.ClassCampusCode).Distinct().ToList();

            methodResult.Result = students;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
