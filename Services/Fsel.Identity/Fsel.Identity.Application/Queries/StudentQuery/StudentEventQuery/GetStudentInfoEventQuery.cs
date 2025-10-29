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
        private readonly IStudentRepository _studentRepository;
        private const string EventCode = "EVHoChiMinh";
        private const string Parent = "Phụ Huynh";

        public GetStudentInfoEventQueryHandler(
            IHumanRepository humanRepository,
            AuthContext authContext,
            UserManager<User> userManager,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
            IStudentRepository studentRepository)
        {
            _humanRepository = humanRepository;
            _authContext = authContext;
            _userManager = userManager;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _studentRepository = studentRepository;
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
                                                                                   .Where(x => x.StudentId == student.Id).OrderByDescending(p => p.CreatedDate)
                                                                                   .FirstOrDefaultAsync(cancellationToken);

            var competitionEvent = studentCompetitionEvent?.CompetitionEvents;

            var isByPassEmailConfirm = competitionEvent?.EventContent?.IsByPassEmailComfirm ?? default;

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
                IsStudentVerifiedForEvent = isByPassEmailConfirm,
                AllowParentInfoUpdate = competitionEvent == null || string.IsNullOrEmpty(competitionEvent.EventCode) || !competitionEvent.EventCode.Contains(EventCode, StringComparison.InvariantCultureIgnoreCase),
            };

            var schoolId = human.Student?.SchoolId ?? default;
            var competitionEventId = competitionEvent?.Id ?? default;

            var query = await (from u in _userManager.Users
                               join h in _humanRepository.Queryable on u.Id equals h.UserId
                               join s in _studentRepository.Queryable on h.Id equals s.HumanId
                               join sce in _studentCompetitionEventsRepository.Queryable on s.Id equals sce.StudentId
                               where s.SchoolId == schoolId && u.Id != _authContext.CurrentUserId && sce.CompetitionEventId == competitionEventId
                               select new
                               {
                                   User = u,
                                   Human = h,
                                   Student = s
                               }).ToListAsync(cancellationToken);

            var companion = query.FirstOrDefault(p => !string.IsNullOrEmpty(p.Student.ParentPhoneNumber) && p.Student.ParentPhoneNumber == studentInfoEvent.PhoneNumber);
            if (companion != null)
            {
                studentInfoEvent.IsParent = true;
                studentInfoEvent.CompanionInfo = new CompanionInfoEventModel()
                {
                    FullName = companion.User.FullName,
                    Email = companion.User.Email,
                    Birthday = companion.Human.Birthday,
                    School = companion.Student.School,
                    SchoolGrade = companion.Student.SchoolGrade,
                    SchoolClass = companion.Student.SchoolClass,
                    PhoneNumber = companion.User.PhoneNumber
                };
            }
            else if (!string.IsNullOrEmpty(studentInfoEvent.ParentPhoneNumber))
            {
                var parent = query.FirstOrDefault(p => !string.IsNullOrEmpty(p.User.PhoneNumber) && p.User.PhoneNumber == studentInfoEvent.ParentPhoneNumber && p.Student.SchoolGrade == Parent);

                if (parent != null)
                {
                    studentInfoEvent.CompanionInfo = new CompanionInfoEventModel()
                    {
                        FullName = parent.User.FullName,
                        Email = parent.User.Email,
                        Birthday = parent.Human.Birthday,
                        School = parent.Student.School,
                        SchoolGrade = parent.Student.SchoolGrade,
                        SchoolClass = parent.Student.SchoolClass,
                        PhoneNumber = parent.User.PhoneNumber
                    };
                }
            }

            methodResult.Result = studentInfoEvent;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
