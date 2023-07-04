namespace Fsel.System.Application.Querys.LiveTimeFrameQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLiveTimeFrameQuery : IRequest<MethodResult<IList<LiveTimeFrameModel>>>
    {
    }

    public class GetListLiveTimeFrameQueryHandler : IRequestHandler<GetListLiveTimeFrameQuery, MethodResult<IList<LiveTimeFrameModel>>>
    {
        private readonly ILiveTimeFrameRepository _liveTimeFrameRepository;

        public GetListLiveTimeFrameQueryHandler(ILiveTimeFrameRepository liveTimeFrameRepository)
        {
            _liveTimeFrameRepository = liveTimeFrameRepository;
        }

        public async Task<MethodResult<IList<LiveTimeFrameModel>>> Handle(GetListLiveTimeFrameQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LiveTimeFrameModel>>();

            var liveTimeFrame = await _liveTimeFrameRepository.Queryable

                                    .Select(x => new LiveTimeFrameModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        EndTime = x.EndTime,
                                        StartTime = x.StartTime,
                                    }).ToListAsync(cancellationToken);
            methodResult.Result = liveTimeFrame;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
