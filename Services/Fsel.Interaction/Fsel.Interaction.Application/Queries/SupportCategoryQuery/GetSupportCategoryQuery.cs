// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SupportCategoryQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetSupportCategoryQuery : IRequest<MethodResult<SupportCategoryModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetSupportCategoryQueryHandler : IRequestHandler<GetSupportCategoryQuery, MethodResult<SupportCategoryModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportCategoryRepository _supportCategoryRepository;

        public GetSupportCategoryQueryHandler(IMapper mapper, ISupportCategoryRepository supportCategoryRepository)
        {
            _mapper = mapper;
            _supportCategoryRepository = supportCategoryRepository;
        }

        public async Task<MethodResult<SupportCategoryModel>> Handle(GetSupportCategoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportCategoryModel> methodResult = new MethodResult<SupportCategoryModel>();

            var supportCategory = await _supportCategoryRepository.GetIncludeByIdAsync(request.Id);

            if (supportCategory == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<SupportCategoryModel>(supportCategory);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
