// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsProgressCoursesQuery : IRequest<MethodResult<IList<CompetitionStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }

        public IList<Guid>? StudentIds { get; set; }
    }

    public class GetStudentsProgressCoursesQueryHandler : IRequestHandler<GetStudentsProgressCoursesQuery, MethodResult<IList<CompetitionStudentProgressModel>>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetStudentsProgressCoursesQueryHandler(IUserService userService, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<IList<CompetitionStudentProgressModel>>> Handle(GetStudentsProgressCoursesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CompetitionStudentProgressModel>> methodResult = new MethodResult<IList<CompetitionStudentProgressModel>>();
            IList<CompetitionStudentProgressModel> courseProgress = new List<CompetitionStudentProgressModel>();

            var @classResults = await _trainingService.GetListClassByStudentIdAsync(request.StudentId);
            var @classes = @classResults.Content?.Result;
            if (@classes != null && @classes.Any())
            {
                var courseIds = @classes.OrderBy(x => x.CreatedDate).Select(x => x.CourseId).ToList();
                var courses = await _courseRepository.GetByIdsAsync(courseIds);
                if (courses == null || !courses.Any())
                {
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }




                foreach (var item in courses)
                {
                    var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course).FirstOrDefaultAsync(x => x.StudentId == request.StudentId && x.CourseId == item.Id, cancellationToken);
                    CompetitionStudentProgressModel courseStudentProgress = new CompetitionStudentProgressModel();
                    if (courseResult != null)
                    {
                        var courseResultModel = new CourseResultModel
                        {
                            CourseType = courseResult.Course?.CourseType,
                            CourseId = courseResult.CourseId,
                            StudentId = courseResult.StudentId
                        };
                        var (currentProgress, progress) = await _courseRepository.GetContentComplete(courseResultModel);
                        double progressPercentage = ((float)currentProgress / progress) * 100;
                        courseStudentProgress.ContentCompleted = Math.Round(progressPercentage, 2);
                    }
                    courseStudentProgress.CourseName = item.Code;
                    courseStudentProgress.CourseId = item.Id;
                    var @class = @classes.FirstOrDefault(x => x.CourseId == item.Id);
                    courseProgress.Add(courseStudentProgress);
                }
            }
            methodResult.Result = courseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
