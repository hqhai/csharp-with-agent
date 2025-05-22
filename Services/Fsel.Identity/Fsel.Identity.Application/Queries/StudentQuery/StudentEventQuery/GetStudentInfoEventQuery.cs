// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery.StudentEventQuery
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentInfoEventQuery : IRequest<MethodResult<StudentInfoEventModel>>
    {
    }

    public class GetStudentInfoEventQueryHandler : IRequestHandler<GetStudentInfoEventQuery, MethodResult<StudentInfoEventModel>>
    {
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;

        public GetStudentInfoEventQueryHandler(
            AuthContext authContext,
            UserManager<User> userManager,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository)
        {
            _authContext = authContext;
            _userManager = userManager;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
        }

        public async Task<MethodResult<StudentInfoEventModel>> Handle(GetStudentInfoEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentInfoEventModel>();
            var user = await _userManager.Users.Include(x => x.Student)
                                                        .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            var student = user?.Student;
            if (user == null || student == null)
            {
                return methodResult;
            }
            var studentCompetitionEvent = await _studentCompetitionEventsRepository.Queryable.Include(x => x.CompetitionEvents)
                                                                                   .Where(x => x.StudentId == student.Id)
                                                                                   .FirstOrDefaultAsync(cancellationToken);

            var isByPassEmailComfirm = studentCompetitionEvent?.CompetitionEvents?.EventContent?.IsByPassEmailComfirm ?? default;
            var isChangePassword = await _userManager.CheckPasswordAsync(user, user.DefaultPassword ?? string.Empty);
            var studentInfoEvent = new StudentInfoEventModel
            {
                Birthday = user.Birthday,
                Email = user.Email,
                FullName = user.FullName,
                ParentEmail = user.Student?.ParentEmail,
                ParentPhoneNumber = user.Student?.ParentPhoneNumber,
                PhoneNumber = user.PhoneNumber,
                School = user.Student?.School,
                SchoolClass = user.Student?.SchoolClass,
                SchoolGrade = user.Student?.SchoolGrade,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                IsChangePassword = !isChangePassword,
                IsStudentVerifiedForEvent = isByPassEmailComfirm
            };
            methodResult.Result = studentInfoEvent;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
