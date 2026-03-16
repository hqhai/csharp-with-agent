// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.System.Domain.Models.QueryModels;
    using Fsel.System.Domain.Models.QueryModels.ManagerReports;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallReportStudentAssiduityQuery : SearchStudentReportQueryModel, IRequest<MethodResult<OverallReportStudentAssiduityModel>>
    {
    }

    public class GetOverallReportStudentAssiduityQueryHandler : IRequestHandler<GetOverallReportStudentAssiduityQuery, MethodResult<OverallReportStudentAssiduityModel>>
    {
        private readonly IMediator _mediator;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetOverallReportStudentAssiduityQueryHandler(IMediator mediator, IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mediator = mediator;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<OverallReportStudentAssiduityModel>> Handle(GetOverallReportStudentAssiduityQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportStudentAssiduityModel>();

            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListLearningStatus = request.ListLearningStatus,
                ListCompletionStatus = request.ListCompletionStatus,
                IsLearning = request.IsLearning,

                EndDate = request.EndDate,
                StartDate = request.StartDate,
                Keyword = request.Keyword,
            }, cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null || !students.Any())
            {
                methodResult.Result = new OverallReportStudentAssiduityModel();
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var featureAccessTimesQuery = new GetFeatureAccessTimesQueryModel
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                FeatureAccessTimes = students.Select(item =>
                {
                    return new GetFeatureAccessTimeQueryModel
                    {
                        UserId = item.UserId ?? default,
                        CourseId = item.CourseId
                    };
                }).ToList()
            };
            var orverall = await GetOverallFeatureAccessTimes(featureAccessTimesQuery);
            var overallReport = new OverallReportStudentAssiduityModel
            {
                TotalStudent = studentIds?.Count ?? default,
                TotalAvgProgressTime = NumberHelper.ConvertRound(orverall.Item1),
                TotalAvgVisit = NumberHelper.ConvertRound(orverall.Item2),
            };
            methodResult.Result = overallReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<(double, double)> GetOverallFeatureAccessTimes(GetFeatureAccessTimesQueryModel queryModel)
        {
            var courseIds = queryModel.FeatureAccessTimes.Where(x => x.CourseId.HasValue).Select(x => x.CourseId).Distinct().ToList();
            var userIds = queryModel.FeatureAccessTimes.Select(x => x.UserId).ToList();
            var datas = queryModel.FeatureAccessTimes.Where(x => x.CourseId.HasValue).Select(x => new { CourseId = x.CourseId, UserId = x.UserId }).ToList();

            var query = _featureAccessTimeRepository.Queryable.Where(x => x.CourseId.HasValue && courseIds.Contains(x.CourseId.Value) && userIds.Contains(x.CreatedUserId))
                                                              .Where(x => x.EnumFeature != Shared.Enums.EnumFeature.Other);
            if (queryModel.StartDate.HasValue)
            {
                query = query.Where(x => queryModel.StartDate.Value.Date <= (x.UpdatedDate ?? x.CreatedDate).Date);
            }
            if (queryModel.EndDate.HasValue)
            {
                query = query.Where(x => queryModel.EndDate.Value.Date >= (x.UpdatedDate ?? x.CreatedDate).Date);
            }
            var featureAccessTimes = await query.Select(x => new
            {
                CourseId = x.CourseId,
                UserId = x.CreatedUserId,
                AccessTime = x.AccessTime,
                Visit = x.Visit,
            }).ToListAsync();

            var overall = featureAccessTimes.Join(
                                     datas,
                                     featureAccessTime => new { featureAccessTime.CourseId, featureAccessTime.UserId },
                                     student => new { student.CourseId, student.UserId },
                                     (featureAccessTime, student) => featureAccessTime)
                                     .GroupBy(x => new { x.CourseId, x.UserId })
                                     .Select(x => new
                                     {
                                         AccessTime = x.Sum(x => x.AccessTime),
                                         Visit = x.Sum(x => x.Visit),
                                     }).ToList();
            if (!overall.Any())
            {
                return (default, default);
            }
            return (NumberHelper.ConvertRound((double)overall.Sum(x => x.AccessTime) / userIds.Count), NumberHelper.ConvertRound((double)overall.Sum(x => x.Visit) / userIds.Count));
        }
    }
}
