// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ManagerReportQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.System.Domain.Models.QueryModels.ManagerReports;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchReportAssiduityQuery : SearchStudentReportQueryModel, IRequest<MethodResult<SearchReportStudentAssiduityModel>>
    {
    }

    public class SearchReportAssiduityQueryHandler : IRequestHandler<SearchReportAssiduityQuery, MethodResult<SearchReportStudentAssiduityModel>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public SearchReportAssiduityQueryHandler(
            IMediator mediator,
            IMapper mapper,
            IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mediator = mediator;
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<SearchReportStudentAssiduityModel>> Handle(SearchReportAssiduityQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SearchReportStudentAssiduityModel>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListCourseLevel = request.ListCourseLevel,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                IsLearning = request.IsLearning,
                ListLearningStatus = request.ListLearningStatus,
                ListCompletionStatus = request.ListCompletionStatus,

                PageSize = request.PageSize,
                Filters = request.Filters,
                IncludePaths = request.IncludePaths,
                Keyword = request.Keyword,
                Page = request.Page,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CourseType = request.CourseType,
                IsSearchReport = true,
            }, cancellationToken);

            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null || !students.Any())
            {
                methodResult.Result = new SearchReportStudentAssiduityModel();
                return methodResult;
            }
            var featureAccessTimeReportQuerys = students.Where(x => x.CourseId.HasValue).Select(item =>
            {
                return new GetFeatureAccessTimeReportQueryModel
                {
                    UserId = item.UserId ?? default,
                    CourseId = item.CourseId.GetValueOrDefault()
                };
            }).ToList();

            var overallFeatureAccessTimes = await _featureAccessTimeRepository.GetOverallFeatureAccessTimesAsync(featureAccessTimeReportQuerys, request.StartDate, request.EndDate);
            var dataOverallResult = await _mediator.Send(new GetOverallReportStudentAssiduityQuery
            {
                Keyword = request.Keyword,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                IsLearning = request.IsLearning,
                ListCompletionStatus = request.ListCompletionStatus,
                ListCourseLevel = request.ListCourseLevel,
                ListLearningStatus = request.ListLearningStatus,

                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CourseType = request.CourseType,
            }, cancellationToken);
            var reportStudentAssiduity = _mapper.Map<SearchReportStudentAssiduityModel>(dataOverallResult.Result);
            var studentIds = students.Select(x => x.Id).ToList();
            var datas = new List<StudentAssiduityModel>();
            students.ForEach(student =>
            {
                var userId = student.UserId ?? default;
                var overallFeatureAccessTime = overallFeatureAccessTimes.FirstOrDefault(x => x.UserId == userId && x.CourseId == student.CourseId);
                var studentAssiduity = new StudentAssiduityModel
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    PhoneNumber = student.PhoneNumber,
                    Email = student.Email,
                    SchoolName = student.School,
                    SchoolClass = student.SchoolClass,
                    SchoolGrade = student.SchoolGrade,
                    CourseLevel = student.CourseLevel,
                    UserName = student.UserName,
                    ExpiredDate = student.ExpiredDate,
                    ProcessDate = student.CreatedDate
                };
                _mapper.Map(overallFeatureAccessTime, studentAssiduity);
                datas.Add(studentAssiduity);
            });
            reportStudentAssiduity.PagingItems = new PagingItemsModel<StudentAssiduityModel>(datas, request, reportStudentAssiduity.TotalStudent);
            methodResult.Result = reportStudentAssiduity;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
