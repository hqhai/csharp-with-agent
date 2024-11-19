// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetReportLearningResultQuery : IRequest<MethodResult<DashBoardLearningResultModel>>
    {
        public DateTime? EndDate { get; set; }
    }

    public class GetReportLearningResultQueryHandler : IRequestHandler<GetReportLearningResultQuery, MethodResult<DashBoardLearningResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;

        public GetReportLearningResultQueryHandler(IMapper mapper,
                                                   IUserService userService,
                                                   IUnitResultRepository unitResultRepository,
                                                   ICourseResultRepository courseResultRepository,
                                                   ICourseRepository courseRepository)
        {
            _mapper = mapper;
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<DashBoardLearningResultModel>> Handle(GetReportLearningResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DashBoardLearningResultModel>();
            var unitOveralls = await (from baseQ in _courseResultRepository.Queryable
                                      join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                                      join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId } equals new { ur.StudentId, ur.CourseId }
                                      where baseQ.WorkingStatus == EnumWorkingStatus.Active && ur.Status == EnumResultStatus.Done &&
                                      (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date == request.EndDate.Value.Date)

                                      group new { baseQ, ur, c } by new { baseQ.CourseId, baseQ.StudentId } into g
                                      select new
                                      {
                                          StudentId = g.Key.StudentId,
                                          CourseLevel = g.Select(x => x.c).Select(x => x.CourseLevel).FirstOrDefault(),
                                          Percent = g.Select(x => x.ur).Average(x => x.Percent)
                                      }).ToListAsync(cancellationToken);
            DashBoardLearningResultModel reportLearningResult = new DashBoardLearningResultModel
            {
                Percent = (int)NumberHelper.ConvertRound(unitOveralls.Average(x => x.Percent)),
                //LearningResultChart = new LearningResultChartModel
                //{
                //    Type = EnumChartType.PieChart,
                //    LearningResultDatas = ConvertHelper.EnumToList<Enum>().Select(x =>
                //    {
                //        var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(x);
                //        return new LearningResultDataChartModel
                //        {
                //            Label = nameof(x),
                //            TotalStudentAca =
                //        };
                //    }).ToList()
                //}
                //LearningResultChart = new LearningResultChartModel
                //{
                //    Type = EnumChartType.PieChart,
                //    LearningResultDatas = ConvertHelper.EnumToList<EnumCourseType>().Select(x =>
                //    {
                //        var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(x);
                //        return new LearningResultDataChartModel
                //        {
                //            Label = nameof(x),
                //            TotalStudentAca =
                //        };
                //    }).ToList()
                //}
            };

            methodResult.Result = reportLearningResult;
            return methodResult;
        }
    }
}
