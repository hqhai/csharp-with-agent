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
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchReportLearningProgressQuery : SearchReportLearningProgressQueryModel, IRequest<MethodResult<SearchReportLearningProgressModel>>
    {
    }

    public class SearchReportLearningProgressQueryHandler : IRequestHandler<SearchReportLearningProgressQuery, MethodResult<SearchReportLearningProgressModel>>
    {
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

        public async Task<MethodResult<SearchReportLearningProgressModel>> Handle(SearchReportLearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SearchReportLearningProgressModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var dataOverallResult = await _mediator.Send(new GetOverallReportLearningProgressQuery
            {
                Keyword = request.Keyword,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListCourseLevel = request.ListCourseLevel,
                ListLearningStatus = request.ListLearningStatus,
                ListCompletionStatus = request.ListCompletionStatus,
                ListCurrentLevel = request.ListCurrentLevel,
                ListOverallScore = request.ListOverallScore,
                IsLearning = request.IsLearning,

                EndDate = request.EndDate,
                CourseType = request.CourseType,
            }, cancellationToken);
            var reportLearningProgress = _mapper.Map<SearchReportLearningProgressModel>(dataOverallResult.Result);

            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                Keyword = request.Keyword,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListCourseLevel = request.ListCourseLevel,
                ListLearningStatus = request.ListLearningStatus,
                ListCompletionStatus = request.ListCompletionStatus,
                IsLearning = request.IsLearning,
                ListCurrentLevel = request.ListCurrentLevel,
                ListOverallScore = request.ListOverallScore,

                EndDate = request.EndDate,
                PageSize = request.PageSize,
                Filters = request.Filters,
                SortBy = request.SortBy,
                IncludePaths = request.IncludePaths,
                Page = request.Page,
                CourseType = request.CourseType,
                ManagerReportType = EnumManagerReportType.ReportLearningProgress,
                IsSearchReport = true
            }, cancellationToken);

            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }

            var students = userResults?.Result;
            if (students == null || !students.Any())
            {
                methodResult.Result = reportLearningProgress;
                return methodResult;
            }

            var lists = students.Select(x => new CourseResultModel { CourseId = x.CourseId.GetValueOrDefault(), StudentId = x.Id }).ToList();
            var courseCompletes = await _managerProgressHelper.GetProgressCompleteLessonAsync(lists, request.EndDate);

            var datas = new List<LearningProgressModel>();
            var courseCompleteDict = courseCompletes.ToDictionary(x => x.StudentId);
            var dateTimeUTC = DateTime.UtcNow;

            foreach (var item in students)
            {
                courseCompleteDict.TryGetValue(item.Id, out var courseComplete);
                var learningProgress = new LearningProgressModel
                {
                    StudentId = item.Id,
                    FullName = item.FullName,
                    UserName = item.UserName,
                    PhoneNumber = item.PhoneNumber,
                    Email = item.Email,
                    SchoolGrade = item.SchoolGrade,
                    SchoolClass = item.SchoolClass,
                    CourseType = item.CourseLevel.GetEnumCourseType(),
                    CourseLevel = item.CourseLevel,
                    ContentProgress = $"{courseComplete?.TotalLessonDone} / {courseComplete?.TotalLesson}",
                    UnitName = $"{nameof(Domain.Entities.Unit)} {courseComplete?.UnitDisplayOrder}",
                    LessonName = $"{nameof(Lesson)} {courseComplete?.LessonDisplayOrder}",
                    SchoolName = item.School,
                    Status = item.ExpiredDate > dateTimeUTC ? EnumLearningStatus.InProgress : EnumLearningStatus.Expired,
                };
                datas.Add(learningProgress);
            }

            reportLearningProgress.PagingItems = new PagingItemsModel<LearningProgressModel>(datas, request, reportLearningProgress.TotalStudent);
            methodResult.Result = reportLearningProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
