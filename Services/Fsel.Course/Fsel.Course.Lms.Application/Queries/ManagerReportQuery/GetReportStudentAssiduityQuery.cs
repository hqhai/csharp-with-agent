// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReportStudentAssiduityQuery : SearchReportAssiduityQueryModel, IRequest<MethodResult<IList<StudentAssiduityModel>>>
    {
    }

    public class GetReportStudentAssiduityQueryHandler : IRequestHandler<GetReportStudentAssiduityQuery, MethodResult<IList<StudentAssiduityModel>>>
    {
        private readonly IMediator _mediator;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;

        public GetReportStudentAssiduityQueryHandler(
            IMediator mediator,
            ICourseResultRepository courseResultRepository,
            ISystemService systemService)
        {
            _mediator = mediator;
            _courseResultRepository = courseResultRepository;
            _systemService = systemService;
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
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                ManagerReportType = EnumManagerReportType.ReportStudentAssiduity
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
            var courseResults = await _courseResultRepository.Queryable.Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active).ToListAsync(cancellationToken);

            var featureAccessTimesQuery = new FeatureAccessTimesQueryModel
            {
                EndDate = request.EndDate,
                StartDate = request.StartDate,
                FeatureAccessTimes = students.SelectMany(item =>
                    {
                        var userId = item.UserId ?? default;
                        var query = new List<FeatureAccessTimeQueryModel> {
                        new FeatureAccessTimeQueryModel
                        {
                            UserId = userId,
                            CourseId =  item.CourseId,
                            EnumFeature = EnumFeature.ClassForum,
                        },
                        new FeatureAccessTimeQueryModel
                        {
                            UserId = userId,
                            CourseId = item.CourseId,
                            EnumFeature = EnumFeature.HomeWork,
                        },
                        new FeatureAccessTimeQueryModel
                        {
                            UserId = userId,
                            CourseId = item.CourseId,
                            EnumFeature = EnumFeature.VideoLesson,
                        },
                        new FeatureAccessTimeQueryModel
                        {
                            UserId = userId,
                            CourseId = item.CourseId
                        }
                    };
                        return query;
                    }).ToList()
            };
            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeToModulesAsync(featureAccessTimesQuery);
            if (!featureAccessTimeResult.IsSuccessStatusCode)
            {
                methodResult.AddError(featureAccessTimeResult.Error);
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResult.Content?.Result;
            var datas = students.Select(student =>
            {
                var userId = student.UserId ?? default;
                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == student.Id);
                var studentAssiduity = new StudentAssiduityModel
                {
                    FullName = student.FullName,
                    Email = student.Email,
                    SchoolName = student.School,
                    SchoolClass = student.SchoolClass,
                    SchoolGrade = student.SchoolGrade,
                    CourseLevel = student.CourseLevel,
                    ExpiredDate = student.ExpiredDate,
                    ProcessDate = courseResult?.ProcessDate
                };
                GetFeatureAccessTime(studentAssiduity, featureAccessTimes, userId);
                return studentAssiduity;
            }).ToList();
            methodResult.Result = datas;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static void GetFeatureAccessTime(StudentAssiduityModel studentAssiduity, IList<FeatureAccessTimeModel>? featureAccessTimes, Guid userId)
        {
            var query = featureAccessTimes?.Where(x => x.UserId == userId);
            studentAssiduity.TotalTimeVideo = query?.FirstOrDefault(x => x.EnumFeature == EnumFeature.VideoLesson)?.AccessTime ?? default;
            studentAssiduity.TotalTimeClassForum = query?.FirstOrDefault(x => x.EnumFeature == EnumFeature.ClassForum)?.AccessTime ?? default;
            studentAssiduity.TotalTimeHomeWork = query?.FirstOrDefault(x => x.EnumFeature == EnumFeature.HomeWork)?.AccessTime ?? default;
            studentAssiduity.TotalTime = query?.FirstOrDefault(x => x.CourseId.HasValue)?.AccessTime ?? default;
            studentAssiduity.TotalVisit = query?.FirstOrDefault(x => x.CourseId.HasValue && !x.EnumFeature.HasValue)?.Visit ?? default;
            studentAssiduity.CurrentDate = query?.FirstOrDefault(x => x.CourseId.HasValue && !x.EnumFeature.HasValue)?.LastVisited ?? null;
        }
    }
}
