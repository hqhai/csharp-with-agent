// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentAggregateQuery
{
    using Common.ActionResults;
    using Core.Base;
    using Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Services.UserServices;
    using Shared.Enums;

    public class GetClassCampusQuery : IRequest<MethodResult<IList<string>>>
    {

    }

    public class GetClassCampusQueryHandler : IRequestHandler<GetClassCampusQuery, MethodResult<IList<string>>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetClassCampusQueryHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,  IUserService userService, AuthContext authContext)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<string>>> Handle(GetClassCampusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<string>>();
            Guid? schoolId = null;

            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };

            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                schoolId = (await _userService.GetSchoolIdAsync()).Content?.Result;
            }

            var query = _studentGoalAggregateRepository.Queryable.Where(x => x.IsActive);
            if (schoolId.HasValue)
            {
                query = query.Where(x => x.SchoolId == schoolId);
            }

            var studentIds = query.Select(l => l.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result?.Select(x => x.ClassCampusCode).Distinct().ToList();

            methodResult.Result = students;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
