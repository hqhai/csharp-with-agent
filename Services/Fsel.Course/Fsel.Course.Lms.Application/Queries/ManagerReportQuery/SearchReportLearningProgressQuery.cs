// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchReportLearningProgressQuery : SearchReportLearningProgressQueryModel, IRequest<MethodResult<SearchReportLearningProgressModel>>
    {
        public SearchReportLearningProgressQuery()
        {
            ManagerReportType = EnumManagerReportType.ReportLearningProgress;
        }
    }

    public class SearchReportLearningProgressQueryHandler : IRequestHandler<SearchReportLearningProgressQuery, MethodResult<SearchReportLearningProgressModel>>
    {
        private const int MaxPageSize = 100;

        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ManagerProgressHelper _managerProgressHelper;

        public SearchReportLearningProgressQueryHandler(
            IMediator mediator,
            IMapper mapper,
            ManagerProgressHelper managerProgressHelper)
        {
            _mediator = mediator;
            _mapper = mapper;
            _managerProgressHelper = managerProgressHelper;
        }

        public async Task<MethodResult<SearchReportLearningProgressModel>> Handle(
            SearchReportLearningProgressQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<SearchReportLearningProgressModel>();

            if (request.PageSize > MaxPageSize)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var overallReport = await GetOverallReportAsync(request, cancellationToken);
            var studentResult = await _mediator.Send(new GetStudentReportQuery(request), cancellationToken);

            if (!studentResult.IsOK)
            {
                methodResult.AddErrorBadRequest(studentResult.ErrorMessages);
                return methodResult;
            }

            if (studentResult.Result == null || !studentResult.Result.Any())
            {
                methodResult.Result = overallReport;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var learningProgresses = await BuildLearningProgressAsync(
                studentResult.Result,
                request.EndDate);

            overallReport.PagingItems = new PagingItemsModel<LearningProgressModel>(
                learningProgresses,
                request,
                overallReport.TotalStudent);

            methodResult.Result = overallReport;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }

        private async Task<SearchReportLearningProgressModel> GetOverallReportAsync(
            SearchReportLearningProgressQuery request,
            CancellationToken cancellationToken)
        {
            var overallResult = await _mediator.Send(new GetOverallReportLearningProgressQuery(request), cancellationToken);
            return _mapper.Map<SearchReportLearningProgressModel>(overallResult.Result);
        }

        private async Task<List<LearningProgressModel>> BuildLearningProgressAsync(
            IEnumerable<StudentDtoModel> students,
            DateTime? endDate)
        {
            var courseRequests = students
                .Select(student => new CourseResultModel
                {
                    CourseId = student.CourseId.GetValueOrDefault(),
                    StudentId = student.Id
                })
                .ToList();

            var courseCompletes = await _managerProgressHelper
                .GetProgressCompleteLessonAsync(courseRequests, endDate);

            var courseCompleteLookup = courseCompletes
                .ToDictionary(x => x.StudentId);

            var nowUtc = DateTime.UtcNow;

            var result = new List<LearningProgressModel>();

            foreach (var student in students)
            {
                courseCompleteLookup.TryGetValue(student.Id, out var progress);

                result.Add(new LearningProgressModel
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    UserName = student.UserName,
                    PhoneNumber = student.PhoneNumber,
                    Email = student.Email,
                    SchoolGrade = student.SchoolGrade,
                    SchoolClass = student.SchoolClass,
                    SchoolName = student.School,
                    CourseLevel = student.CourseLevel,
                    CourseType = student.CourseLevel.GetEnumCourseType(),
                    ContentProgress = $"{progress?.TotalLessonDone} / {progress?.TotalLesson}",
                    UnitName = $"{nameof(Domain.Entities.Unit)} {progress?.UnitDisplayOrder}",
                    LessonName = $"{nameof(Lesson)} {progress?.LessonDisplayOrder}",
                    Status = student.ExpiredDate > nowUtc
                        ? EnumLearningStatus.InProgress
                        : EnumLearningStatus.Expired
                });
            }

            return result;
        }
    }
}
