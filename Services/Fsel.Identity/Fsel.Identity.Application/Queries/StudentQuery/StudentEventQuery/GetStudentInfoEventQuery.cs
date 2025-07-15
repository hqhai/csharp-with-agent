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
        private readonly IHumanRepository _humanRepository;
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;

        public GetStudentInfoEventQueryHandler(
            IHumanRepository humanRepository,
            AuthContext authContext,
            UserManager<User> userManager,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository)
        {
            _humanRepository = humanRepository;
            _authContext = authContext;
            _userManager = userManager;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
        }

        public async Task<MethodResult<StudentInfoEventModel>> Handle(GetStudentInfoEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentInfoEventModel>();
            var human = await _humanRepository.Queryable.Include(x => x.Student).Include(x => x.User)
                                                        .FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken);
            var user = human?.User;
            var student = human?.Student;
            if (user == null || human == null || student == null)
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
                Birthday = human.Birthday,
                Email = human.Email,
                FullName = human.FullName,
                ParentEmail = human.Student?.ParentEmail,
                ParentPhoneNumber = human.Student?.ParentPhoneNumber,
                PhoneNumber = human.PhoneNumber,
                School = human.Student?.School,
                SchoolClass = human.Student?.SchoolClass,
                SchoolGrade = human.Student?.SchoolGrade,
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
