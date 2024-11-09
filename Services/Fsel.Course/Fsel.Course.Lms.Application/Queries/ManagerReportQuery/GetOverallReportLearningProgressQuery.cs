// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallReportLearningProgressQuery : SearchReportLearningProgressQueryModel, IRequest<MethodResult<OverallReportLearningProgressModel>>
    {
    }

    public class GetOverallReportLearningProgressQueryHandler : IRequestHandler<GetOverallReportLearningProgressQuery, MethodResult<OverallReportLearningProgressModel>>
    {
        private readonly IMediator _mediator;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;

        public GetOverallReportLearningProgressQueryHandler(IMediator mediator, ICourseResultRepository courseResultRepository, ManagerProgressHelper managerProgressHelper)
        {
            _mediator = mediator;
            _courseResultRepository = courseResultRepository;
            _managerProgressHelper = managerProgressHelper;
        }

        public async Task<MethodResult<OverallReportLearningProgressModel>> Handle(GetOverallReportLearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportLearningProgressModel>();

            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
                ManagerReportType = EnumManagerReportType.ReportLearningProgress,
            }, cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            var studentIds = students?.Select(x => x.Id).ToList();
            var overallReport = new OverallReportLearningProgressModel
            {
                TotalStudent = studentIds?.Count ?? default
            };
            GetTotalCourseLevel(overallReport, request.CourseType, students);
            await SetAverageProgress(overallReport, request, studentIds);
            methodResult.Result = overallReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetAverageProgress(OverallReportLearningProgressModel overallReport, GetOverallReportLearningProgressQuery request, IList<Guid>? studentIds)
        {
            if (studentIds == null)
            {
                return;
            }
            var averageProgress = new List<(double, double)>();
            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active).ToListAsync();
            foreach (var courseResult in courseResults)
            {
                averageProgress.Add(await _managerProgressHelper.GetCompleteCourseAsync(new CourseResultModel
                {
                    CourseType = courseResult.Course?.CourseType,
                    CourseId = courseResult.CourseId,
                    StudentId = courseResult.StudentId
                }, request.EndDate));
            }
            var countProgress = averageProgress.Any() ? NumberHelper.ConvertRound(averageProgress.Average(x => x.Item1)) : default;
            var totalProgress = averageProgress.Any() ? NumberHelper.ConvertRound(averageProgress.Average(x => x.Item2)) : request.CourseType.GetTotalProgress();
            overallReport.ContentAverageProgress = string.Format("{0}/{1}", countProgress, totalProgress);
        }

        private static void GetTotalCourseLevel(OverallReportLearningProgressModel overallReportLearningProgress, EnumCourseType courseType, IList<StudentDtoModel>? studentDtos)
        {
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(courseType);
            overallReportLearningProgress.CourseLevelProgresses = new List<CourseLevelProgressModel>();
            foreach (var item in courseLevels)
            {
                overallReportLearningProgress.CourseLevelProgresses.Add(new CourseLevelProgressModel
                {
                    CourseLevel = item,
                    TotalCount = studentDtos?.Where(x => x.CourseLevel == item).Count() ?? default
                });
            }
        }
    }
}
