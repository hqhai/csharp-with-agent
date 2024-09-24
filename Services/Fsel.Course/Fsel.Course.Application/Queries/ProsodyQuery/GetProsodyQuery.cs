// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ProsodyQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetProsodyQuery : IRequest<MethodResult<IList<ProsodyScoreModel>>>
    {

    }
    public class GetProsodyQueryHandler : IRequestHandler<GetProsodyQuery, MethodResult<IList<ProsodyScoreModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IProsodyScoreRepository _prosodyScoreRepository;
        public GetProsodyQueryHandler(IMapper mapper, IProsodyScoreRepository prosodyScoreRepository)
        {
            _mapper = mapper;
            _prosodyScoreRepository = prosodyScoreRepository;
        }

        public async Task<MethodResult<IList<ProsodyScoreModel>>> Handle(GetProsodyQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ProsodyScoreModel>> methodResult = new MethodResult<IList<ProsodyScoreModel>>();

            var prosodyScores = _prosodyScoreRepository.Queryable.OrderByDescending(x => x.BandScore).ToList();

            methodResult.Result = _mapper.Map<IList<ProsodyScoreModel>>(prosodyScores);

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

    }
}
