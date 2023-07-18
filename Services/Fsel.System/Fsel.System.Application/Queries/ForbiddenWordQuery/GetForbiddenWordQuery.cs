// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Querys.ForbiddenWordQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetForbiddenWordQuery : IRequest<MethodResult<ForbiddenWordModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetForbiddenWordQueryHandler : IRequestHandler<GetForbiddenWordQuery, MethodResult<ForbiddenWordModel>>
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;
        private readonly IMapper _mapper;

        public GetForbiddenWordQueryHandler(IForbiddenWordRepository forbiddenWordRepository, IMapper mapper)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ForbiddenWordModel>> Handle(GetForbiddenWordQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ForbiddenWordModel> methodResult = new MethodResult<ForbiddenWordModel>();

            var course = await _forbiddenWordRepository.GetIncludeByIdAsync(request.Id);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumForbiddenWordErrorCode.ForbiddenWordsNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<ForbiddenWordModel>(course);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
