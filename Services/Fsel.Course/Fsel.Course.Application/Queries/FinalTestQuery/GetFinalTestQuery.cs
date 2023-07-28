// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.FinalTestQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetFinalTestQuery : IRequest<MethodResult<FinalTestModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetFinalTestQueryHandler : IRequestHandler<GetFinalTestQuery, MethodResult<FinalTestModel>>
    {
        private readonly IFinalTestRepository _finalTestRepository;

        public GetFinalTestQueryHandler(IFinalTestRepository finalTestRepository)
        {
            _finalTestRepository = finalTestRepository;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(GetFinalTestQuery request, CancellationToken cancellationToken)
        {
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();
            ArgumentNullException.ThrowIfNull(request);
            var finalTest = await _finalTestRepository.GetIncludeAllAsync(request.Id);

            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }

            methodResult.Result = finalTest;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
