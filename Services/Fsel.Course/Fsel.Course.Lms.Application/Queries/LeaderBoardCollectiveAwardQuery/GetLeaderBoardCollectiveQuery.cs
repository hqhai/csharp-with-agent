// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LeaderBoardCollectiveAwardQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.QueryModels.LeaderBoardRewards;
    using MediatR;

    public class GetLeaderBoardCollectiveQuery : IRequest<MethodResult<IList<LeaderBoardCollectiveAwardModel>>>
    {
        public string? EventCode { get; set; }
    }

    public class GetLeaderBoardCollectiveQueryHandler : IRequestHandler<GetLeaderBoardCollectiveQuery, MethodResult<IList<LeaderBoardCollectiveAwardModel>>>
    {


        public GetLeaderBoardCollectiveQueryHandler(
           )
        {

        }

        public async Task<MethodResult<IList<LeaderBoardCollectiveAwardModel>>> Handle(GetLeaderBoardCollectiveQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<LeaderBoardCollectiveAwardModel>>();
            if (request.EventCode != "EVHanoiTest")
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.EventCode), request.EventCode);
                return methodResult;
            }

            var result = new List<LeaderBoardCollectiveAwardModel>
            {
                new LeaderBoardCollectiveAwardModel
                {
                    SchoolName = "THCS Đống Đa",
                    AmountFinishPT=20,
                    AmountFinishUnitOne=9,
                    AmountFinishUnitTwo=2,
                    AmountFinishUnitThree=1,
                    AmountFinishUnitFour=2
                },
                new LeaderBoardCollectiveAwardModel
                {
                    SchoolName = "THCS Dịch Vọng Hậu",
                    AmountFinishPT=20,
                    AmountFinishUnitOne=2,
                    AmountFinishUnitTwo=1,
                    AmountFinishUnitThree=1,
                    AmountFinishUnitFour=1
                },
                new LeaderBoardCollectiveAwardModel
                {
                    SchoolName = "THCS Nguyễn Đình Chiểu",
                    AmountFinishPT=30,
                    AmountFinishUnitOne=15,
                    AmountFinishUnitTwo=9,
                    AmountFinishUnitThree=2,
                    AmountFinishUnitFour=1
                },
                new LeaderBoardCollectiveAwardModel
                {
                    SchoolName = "Tiểu Học Tây Hồ",
                    AmountFinishPT=15,
                    AmountFinishUnitOne=9,
                    AmountFinishUnitTwo=9,
                    AmountFinishUnitThree=4,
                    AmountFinishUnitFour=3
                },
            };


            methodResult.Result = result;
            return methodResult;
        }
    }
}
