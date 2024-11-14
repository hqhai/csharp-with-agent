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

        public SearchReportLearningProgressQueryHandler(
            IMediator mediator,
            IMapper mapper,
            ICourseResultRepository courseResultRepository,
            ManagerProgressHelper managerProgressHelper,
            ICourseRepository courseRepository
            )
        {
            _mediator = mediator;
            _mapper = mapper;
            _courseResultRepository = courseResultRepository;
            _managerProgressHelper = managerProgressHelper;
            _courseRepository = courseRepository;
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
                EndDate = request.EndDate,
                PageSize = request.PageSize,
                Filters = request.Filters,
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
            if (students == null)
            {
                methodResult.Result = reportLearningProgress;
                return methodResult;
            }
            var lists = students.Select(x => new CourseResultModel { CourseId = x.CourseId.GetValueOrDefault(), StudentId = x.Id }).ToList();
            var datas = new List<LearningProgressModel>();

            var unitCurrentPositions = await _courseRepository.GetDisplayOrder(lists, request.EndDate);
            var courseCompletes = await _managerProgressHelper.GetProgressCompleteModuleAsync(lists, request.EndDate);
            foreach (var item in students)
            {
                var unitCurrentPosition = unitCurrentPositions.FirstOrDefault(x => x.StudentId == item.Id);
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
                var courseResult = lists.FirstOrDefault(x => x.StudentId == item.Id) ?? new CourseResultModel
                {
                    StudentId = item.Id,
                    CourseId = item.CourseId.GetValueOrDefault()
                };
                var courseComplete = courseCompletes.FirstOrDefault(x => x.StudentId == item.Id);
                learningProgress.ContentProgress = $"{courseComplete?.CountComplete} / {courseComplete?.TotalComplete}";
                learningProgress.UnitName = $"{nameof(Domain.Entities.Unit)} {unitCurrentPosition?.DisplayUnit}";
                learningProgress.LessonName = $"{nameof(Lesson)} {unitCurrentPosition?.DisplayLesson}";
                datas.Add(learningProgress);
            }

            reportLearningProgress.PagingItems = new PagingItemsModel<LearningProgressModel>(datas, request, reportLearningProgress.TotalStudent);
            methodResult.Result = reportLearningProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
