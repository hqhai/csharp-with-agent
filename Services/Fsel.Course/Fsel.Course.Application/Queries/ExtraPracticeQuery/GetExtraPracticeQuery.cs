// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ExtraPracticeQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetExtraPracticeQuery : IRequest<MethodResult<ExtraPracticeModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetExtraPracticeQueryHandler : IRequestHandler<GetExtraPracticeQuery, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;

        public GetExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository, IMapper mapper)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(GetExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();
            ArgumentNullException.ThrowIfNull(request);
            var extraPractice = await _extraPracticeRepository.GetIncludeAllAsync(request.Id);

            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<ExtraPracticeModel>(extraPractice);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
