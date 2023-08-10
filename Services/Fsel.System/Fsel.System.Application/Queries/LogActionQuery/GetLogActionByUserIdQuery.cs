// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LogActionQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLogActionByUserIdQuery : IRequest<MethodResult<IList<LogActionModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetLogActionByUserIdQueryHandler : IRequestHandler<GetLogActionByUserIdQuery, MethodResult<IList<LogActionModel>>>
    {
        private readonly ILogActionRepository _logActionRepository;
        private readonly IMapper _mapper;

        public GetLogActionByUserIdQueryHandler(ILogActionRepository logActionRepository, IMapper mapper)
        {
            _logActionRepository = logActionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LogActionModel>>> Handle(GetLogActionByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LogActionModel>>();
            var logActions = await _logActionRepository.Queryable.Where(x => x.CreatedUserId == request.Id).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<LogActionModel>>(logActions);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
