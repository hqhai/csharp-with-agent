// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;

    public class GetReportLearningResultStackQuery : IRequest<MethodResult<StackBarChartsModel>>
    {
        public DateTime? EndDate { get; set; }
    }

    public class GetReportLearningResultStackQueryHandler : IRequestHandler<GetReportLearningResultStackQuery, MethodResult<StackBarChartsModel>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;

        public GetReportLearningResultStackQueryHandler(IMapper mapper, IUserService userService, IUnitResultRepository unitResultRepository)
        {
            _mapper = mapper;
            _userService = userService;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<StackBarChartsModel>> Handle(GetReportLearningResultStackQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StackBarChartsModel>();

            StackBarChartsModel reportLearningResult = new StackBarChartsModel();
            methodResult.Result = reportLearningResult;
            return methodResult;
        }
    }
}
