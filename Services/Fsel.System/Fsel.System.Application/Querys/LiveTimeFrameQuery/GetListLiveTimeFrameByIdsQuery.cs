// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Querys.LiveTimeFrameQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLiveTimeFrameByIdsQuery : IRequest<MethodResult<IList<LiveTimeFrameModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetListLiveTimeFrameByIdsQueryHandler : IRequestHandler<GetListLiveTimeFrameByIdsQuery, MethodResult<IList<LiveTimeFrameModel>>>
    {
        private readonly ILiveTimeFrameRepository _liveTimeFrameRepository;

        public GetListLiveTimeFrameByIdsQueryHandler(ILiveTimeFrameRepository liveTimeFrameRepository)
        {
            _liveTimeFrameRepository = liveTimeFrameRepository;
        }

        public async Task<MethodResult<IList<LiveTimeFrameModel>>> Handle(GetListLiveTimeFrameByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LiveTimeFrameModel>>();

            var liveTimeFrame = await _liveTimeFrameRepository.Queryable
                                    .Where(x => request.Ids!.Contains(x.Id))
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
