// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CurriculumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentCurriculumOfStudentQuery : IRequest<MethodResult<CurriculumModel>>
    {
        public Guid? UserId { get; set; }
    }

    public class GetCurrentCurriculumOfStudentQueryHandler : IRequestHandler<GetCurrentCurriculumOfStudentQuery, MethodResult<CurriculumModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;

        public GetCurrentCurriculumOfStudentQueryHandler(AuthContext authContext, IUserService userService, ICurriculumStudentRepository curriculumStudentRepository, ICurriculumRepository curriculumRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _curriculumStudentRepository = curriculumStudentRepository;
            _curriculumRepository = curriculumRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<CurriculumModel>> Handle(GetCurrentCurriculumOfStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CurriculumModel>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var studentResult = await _userService.GetStudentByUserIdAsync(userId);
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }

            if (!student.CourseId.HasValue)
            {
                return methodResult;
            }

            var curriculum = await (from baseQuery in _curriculumRepository.Queryable
                                    join cs in _curriculumStudentRepository.Queryable on baseQuery.Id equals cs.CurriculumId
                                    join c in _courseRepository.Queryable on baseQuery.CourseId equals c.Id
                                    join cc in _courseRepository.Queryable on baseQuery.CourseCloneId equals cc.Id
                                    where cc.Id == student.CourseId
                                    select new
                                    {
                                        Curriculum = baseQuery,
                                        Course = c,
                                        CourseClone = cc,
                                        CurriculumStudent = cs,
                                    }).FirstOrDefaultAsync(cancellationToken);

            if (curriculum == null)
            {
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == student.CourseId && p.StudentId == student.Id, cancellationToken);

            var curriculumModel = new CurriculumModel()
            {
                Id = curriculum.Curriculum.Id,
                CurriculumName = curriculum.Curriculum.CurriculumName,
                CourseId = curriculum.Curriculum.CourseId,
                CourseCloneId = curriculum.Curriculum.CourseCloneId,
                CreatedDate = curriculum.CurriculumStudent.CreatedDate,
                CourseName = curriculum.Course.Name,
                StartDate = curriculum.Curriculum.StartDate,
                EndDate = curriculum.Curriculum.EndDate,
                CourseLevel = curriculum.Course.CourseLevel,
                CourseType = curriculum.Course.CourseType,
                Subject = "Tiếng Anh",
                IsDone = courseResult != null && courseResult.Status == EnumResultStatus.Done
            };

            methodResult.Result = curriculumModel;
            return methodResult;
        }
    }
}
