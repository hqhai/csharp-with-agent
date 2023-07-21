// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SupportQuestionQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Queries.SupportCategoryQuery;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetSupportQuestionQuery : IRequest<MethodResult<SupportQuestionModel>>
    {
        public Guid Id { get; set; }
    }
    public class GetSupportQuestionQueryHandler : IRequestHandler<GetSupportQuestionQuery, MethodResult<SupportQuestionModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportQuestionRepository _supportQuestionRepository;

        public GetSupportQuestionQueryHandler(IMapper mapper, ISupportQuestionRepository supportQuestionRepository)
        {
            _mapper = mapper;
            _supportQuestionRepository = supportQuestionRepository;
        }

        public async Task<MethodResult<SupportQuestionModel>> Handle(GetSupportQuestionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportQuestionModel> methodResult = new MethodResult<SupportQuestionModel>();

            var supportQuestion = await _supportQuestionRepository.GetIncludeByIdAsync(request.Id);

            if (supportQuestion == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<SupportQuestionModel>(supportQuestion);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
