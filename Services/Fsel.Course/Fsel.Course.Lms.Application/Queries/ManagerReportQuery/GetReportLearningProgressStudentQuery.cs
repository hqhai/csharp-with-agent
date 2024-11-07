// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReportLearningProgressStudentQuery : SearchReportLearningProgressQueryModel, IRequest<MethodResult<IList<LearningProgressModel>>>
    {
    }

    public class GetReportLearningProgressStudentQueryHandler : IRequestHandler<GetReportLearningProgressStudentQuery, MethodResult<IList<LearningProgressModel>>>
    {
        private readonly IMediator _mediator;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public GetReportLearningProgressStudentQueryHandler(IMediator mediator,
            ManagerProgressHelper managerProgressHelper,
            ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository)
        {
            _mediator = mediator;
            _managerProgressHelper = managerProgressHelper;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<IList<LearningProgressModel>>> Handle(GetReportLearningProgressStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LearningProgressModel>>();
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                ManagerReportType = EnumManagerReportType.ReportLearningProgress,
            }, cancellationToken);

            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null)
            {
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var query = _courseResultRepository.Queryable.Include(x => x.Course)
              .Where(x => studentIds.Contains(x.StudentId))
              .Where(x => !x.IsDeleted && x.WorkingStatus == EnumWorkingStatus.Active)
              .GroupBy(r => new { r.StudentId, r.CourseId })
              .Select(group => new CourseResultModel
              {
                  StudentId = group.Key.StudentId,
                  CourseId = group.Key.CourseId,
                  CourseType = group.Select(x => x.Course).FirstOrDefault(c => c!.Id == group.Key.CourseId)!.CourseType,
                  CourseLevel = group.Select(x => x.Course).FirstOrDefault(c => c!.Id == group.Key.CourseId)!.CourseLevel,
                  CreatedDate = group.Max(r => r.CreatedDate),
                  UpdatedDate = group.Max(r => r.UpdatedDate),
              });

            var lists = await query.ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var datas = new List<LearningProgressModel>();
            foreach (var item in students)
            {
                var learningProgress = new LearningProgressModel
                {
                    Email = item.Email,
                    FullName = item.FullName,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    SchoolName = item.School,
                    Status = item.ExpiredDate > DateTime.UtcNow ? EnumLearningStatus.InProgress : EnumLearningStatus.Expired,
                    CourseLevel = item.CourseLevel,
                };
                var courseResult = lists.FirstOrDefault(x => x.StudentId == item.Id);
                if (courseResult != null)
                {
                    var (currentProgress, progress) = await _managerProgressHelper.GetCompleteCourseAsync(courseResult, request.EndDate);
                    var (displayOrderUnit, displayOrderLesson) = await _courseRepository.GetDisplayOrder(courseResult, request.EndDate);
                    learningProgress.ContentProgress = $"{currentProgress} / {progress}";
                    learningProgress.UnitName = $"Unit {displayOrderUnit}";
                    learningProgress.LessonName = $"Lesson {displayOrderLesson}";
                }
                else
                {
                    learningProgress.ContentProgress = $"{ValueSettings.ValueDefault} / {request.CourseType.GetTotalProgress()}";
                }

                datas.Add(learningProgress);
            }
            methodResult.Result = datas;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
