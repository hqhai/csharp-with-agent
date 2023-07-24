// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SupportCategoryQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListSupportCategoryQuery : IRequest<MethodResult<IList<SupportCategoryModel>>>
    {
    }

    public class GetListSupportCategoryQueryHandler : IRequestHandler<GetListSupportCategoryQuery, MethodResult<IList<SupportCategoryModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportCategoryRepository _supportCategoryRepository;

        public GetListSupportCategoryQueryHandler(IMapper mapper, ISupportCategoryRepository supportCategoryRepository)
        {
            _mapper = mapper;
            _supportCategoryRepository = supportCategoryRepository;
        }

        public async Task<MethodResult<IList<SupportCategoryModel>>> Handle(GetListSupportCategoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SupportCategoryModel>> methodResult = new MethodResult<IList<SupportCategoryModel>>();

            var supportCategory = await _supportCategoryRepository.Queryable.ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<SupportCategoryModel>>(supportCategory);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
