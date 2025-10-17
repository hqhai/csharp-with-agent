// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ManagerReportQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.System.Domain.Models.QueryModels.ManagerReports;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetReportStudentAssiduityQuery : SearchStudentReportQueryModel, IRequest<MethodResult<IList<StudentAssiduityModel>>>
    {
    }

    public class GetReportStudentAssiduityQueryHandler : IRequestHandler<GetReportStudentAssiduityQuery, MethodResult<IList<StudentAssiduityModel>>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetReportStudentAssiduityQueryHandler(
            IMediator mediator,
            IMapper mapper,
            IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mediator = mediator;
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<StudentAssiduityModel>>> Handle(GetReportStudentAssiduityQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentAssiduityModel>>();
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                Keyword = request.Keyword,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListSchool = request.ListSchool,
                ListCourseLevel = request.ListCourseLevel,
                ListCourseType = request.ListCourseType,

                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
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
            var featureAccessTimeReportQuerys = students.Where(x => x.CourseId.HasValue).Select(item =>
            {
                return new GetFeatureAccessTimeReportQueryModel
                {
                    UserId = item.UserId ?? default,
                    CourseId = item.CourseId.GetValueOrDefault()
                };
            }).ToList();

            var overallFeatureAccessTimes = await _featureAccessTimeRepository.GetOverallFeatureAccessTimesAsync(featureAccessTimeReportQuerys, request.StartDate, request.EndDate);
            var datas = students.Select(student =>
            {
                var userId = student.UserId ?? default;
                var overallFeatureAccessTime = overallFeatureAccessTimes.FirstOrDefault(x => x.UserId == userId && x.CourseId == student.CourseId);
                var studentAssiduity = new StudentAssiduityModel
                {
                    FullName = student.FullName,
                    Email = student.Email,
                    PhoneNumber = student.PhoneNumber,
                    SchoolName = student.School,
                    SchoolClass = student.SchoolClass,
                    SchoolGrade = student.SchoolGrade,
                    CourseLevel = student.CourseLevel,
                    UserName = student.UserName,
                    ExpiredDate = student.ExpiredDate,
                    ProcessDate = student.CreatedDate
                };
                _mapper.Map(overallFeatureAccessTime, studentAssiduity);
                return studentAssiduity;
            }).ToList();
            methodResult.Result = datas;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
