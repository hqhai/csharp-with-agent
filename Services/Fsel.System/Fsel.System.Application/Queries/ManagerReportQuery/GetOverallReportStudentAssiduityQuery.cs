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

    public class GetOverallReportStudentAssiduityQuery : SearchReportAssiduityQueryModel, IRequest<MethodResult<OverallReportStudentAssiduityModel>>
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
                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType
            }, cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null)
            {
                methodResult.Result = new OverallReportStudentAssiduityModel();
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var featureAccessTimesQuery = new GetFeatureAccessTimesQueryModel
            {
                EndDate = request.EndDate,
                StartDate = request.StartDate,
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
            var courseIds = queryModel.FeatureAccessTimes.Select(x => x.CourseId).ToList();
            var userIds = queryModel.FeatureAccessTimes.Select(x => x.UserId).ToList();
            var query = _featureAccessTimeRepository.Queryable.Where(x => courseIds.Contains(x.CourseId) && userIds.Contains(x.CreatedUserId));
            if (queryModel.StartDate.HasValue)
            {
                query = query.Where(x => queryModel.StartDate.Value.Date <= (x.UpdatedDate ?? x.CreatedDate).Date);
            }
            if (queryModel.EndDate.HasValue)
            {
                query = query.Where(x => queryModel.EndDate.Value.Date >= (x.UpdatedDate ?? x.CreatedDate).Date);
            }
            var overall = await query.GroupBy(x => new { x.CourseId, x.CreatedUserId })
                                     .Select(x => new
                                     {
                                         AccessTime = x.Sum(x => x.AccessTime),
                                         Visit = x.Sum(x => x.Visit),
                                     }).ToListAsync();
            if (!overall.Any())
            {
                return (default, default);
            }
            return (NumberHelper.ConvertRound((double)overall.Sum(x => x.AccessTime) / userIds.Count), NumberHelper.ConvertRound((double)overall.Sum(x => x.Visit) / userIds.Count));
        }
    }
}
