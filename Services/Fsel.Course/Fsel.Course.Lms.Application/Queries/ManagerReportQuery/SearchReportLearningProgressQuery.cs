// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
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
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public SearchReportLearningProgressQueryHandler(
            IMediator mediator,
            IMapper mapper,
            ICourseResultRepository courseResultRepository,
            ManagerProgressHelper managerProgressHelper,
            ICourseRepository courseRepository,
            IUnitResultRepository unitResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IUnitLessonRepository unitLessonRepository,
            ILessonResultRepository lessonResultRepository)
        {
            _mediator = mediator;
            _mapper = mapper;
            _courseResultRepository = courseResultRepository;
            _managerProgressHelper = managerProgressHelper;
            _courseRepository = courseRepository;
            _unitResultRepository = unitResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _unitLessonRepository = unitLessonRepository;
            _lessonResultRepository = lessonResultRepository;
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
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                EndDate = request.EndDate,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
            }, cancellationToken);
            var reportLearningProgress = _mapper.Map<SearchReportLearningProgressModel>(dataOverallResult.Result);

            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                EndDate = request.EndDate,
                PageSize = request.PageSize,
                Filters = request.Filters,
                SortBy = request.SortBy,
                IncludePaths = request.IncludePaths,
                Keyword = request.Keyword,
                Page = request.Page,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
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

            var courseCompletes = await _managerProgressHelper.GetProgressCompleteModuleAsync(lists, request.EndDate);
            var datas = new List<LearningProgressModel>();

            foreach (var item in students)
            {
                var courseComplete = courseCompletes.FirstOrDefault(x => x.StudentId == item.Id);
                var learningProgress = new LearningProgressModel
                {
                    StudentId = item.Id,
                    Email = item.Email,
                    FullName = item.FullName,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    SchoolName = item.School,
                    Status = item.ExpiredDate > DateTime.UtcNow ? EnumLearningStatus.InProgress : EnumLearningStatus.Expired,
                    CourseLevel = item.CourseLevel,
                    ContentProgress = $"{courseComplete?.CountComplete} / {courseComplete?.TotalComplete}",
                    UnitName = $"{nameof(Domain.Entities.Unit)} {courseComplete?.UnitDisplayOrder}",
                    LessonName = $"{nameof(Lesson)} {courseComplete?.LessonDisplayOrder}",
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
