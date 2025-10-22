// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptManagerQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAiCriteriaByIdAipromptQuery : IRequest<MethodResult<IList<AICriteriaConfigsModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetAiCriteriaByIdAipromptQueryHandler : IRequestHandler<GetAiCriteriaByIdAipromptQuery, MethodResult<IList<AICriteriaConfigsModel>>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;

        public GetAiCriteriaByIdAipromptQueryHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
        }

        public async Task<MethodResult<IList<AICriteriaConfigsModel>>> Handle(GetAiCriteriaByIdAipromptQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<AICriteriaConfigsModel>> methodResult = new MethodResult<IList<AICriteriaConfigsModel>>();

            var result = await _aiCriteriaConfigRepository.Queryable
                .Where(x => x.AiPromptManagerId == request.Id)
                .AsNoTracking()
                .Select(x=> new AICriteriaConfigsModel
                {
                    Id = x.Id,
                    UserRole = x.UserRole,
                    SettingAiConfig = x.SettingAiConfig,
                    SettingAiJson = x.SettingAiJson,
                    AiPromptManagerId = x.AiPromptManagerId,
                    SettingFrequency = x.SettingFrequency,
                    SettingPresence = x.SettingPresence,
                    SettingTemperature = x.SettingTemperature,
                    SettingTopP = x.SettingTopP,
                    SettingWordMaxLength = x.SettingWordMaxLength,
                    MaximumNumber = x.MaximumNumber,
                    MaximumToken = x.MaximumToken,
                })
                .ToListAsync(cancellationToken);

            if (result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
