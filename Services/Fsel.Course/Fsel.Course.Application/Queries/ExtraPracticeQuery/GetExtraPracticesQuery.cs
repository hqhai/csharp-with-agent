// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ExtraPracticeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetExtraPracticesQuery : IRequest<MethodResult<IList<ExtraPracticeModel>>>
    {
    }

    public class GetExtraPracticesQueryHandler : IRequestHandler<GetExtraPracticesQuery, MethodResult<IList<ExtraPracticeModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;

        public GetExtraPracticesQueryHandler(IExtraPracticeRepository extraPracticeRepository, IMapper mapper)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<ExtraPracticeModel>>> Handle(GetExtraPracticesQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<ExtraPracticeModel>> methodResult = new MethodResult<IList<ExtraPracticeModel>>();
            ArgumentNullException.ThrowIfNull(request);
            var extraPractices = await _extraPracticeRepository.Queryable.Where(x => x.IsActive).ToListAsync(cancellationToken);
            if (extraPractices == null || extraPractices.Count == 0)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<ExtraPracticeModel>>(extraPractices);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
