// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;

    public class GetLevelsForChangeQuery : IRequest<MethodResult<SubjectModel>>
    {
    }

    public class GetLevelsForChangeQueryHandler : IRequestHandler<GetLevelsForChangeQuery, MethodResult<SubjectModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IChangeCourseService _changeCourseService;
        private readonly ICourseCachingService _courseCachingService;

        public GetLevelsForChangeQueryHandler(AuthContext authContext,
            IUserService userService,
            IChangeCourseService changeCourseService,
            ICourseCachingService courseCachingService)
        {
            _authContext = authContext;
            _userService = userService;
            _changeCourseService = changeCourseService;
            _courseCachingService = courseCachingService;
        }

        public async Task<MethodResult<SubjectModel>> Handle(GetLevelsForChangeQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<SubjectModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var subjectAggregate = await _changeCourseService.GetChangeSubjectAggreate(student, cancellationToken);

            var subjectChangeCourse = subjectAggregate.RootSubjects.FirstOrDefault(x => x.GetComponentsByType<LevelChangeCourse>().Any(x => x.IsCurrentLearningLevel));
            var subjectModel = subjectChangeCourse?.GetSubjectTree(isIncludeLevel: true);

            if (subjectModel != null)
            {
                var availableCourses = await _courseCachingService.GetAllAvailableCoursesAsync();
                await CheckAvailableCourse(subjectModel, availableCourses);
            }

            return new MethodResult<SubjectModel>() { Result = subjectModel, StatusCode = 200 };
        }

        private static async Task CheckAvailableCourse(SubjectModel subjectModel, IList<Course> courses)
        {
            if (subjectModel?.Levels != null)
            {
                foreach (var level in subjectModel.Levels.OfType<SelectionLevelModel>())
                {
                    level.IsAvailableCourse = courses.Any(c => c.LevelId == level.Id);
                }
            }

            if (subjectModel?.ChildSubjects != null)
            {
                foreach (var childSubject in subjectModel.ChildSubjects)
                {
                    await CheckAvailableCourse(childSubject, courses);
                }
            }
        }
    }
}
