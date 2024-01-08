// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.HarmfulContentQuery
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Application.Services.HarmfulContentService;
    using Fsel.Interaction.Application.Services.HarmfulContentService.Models;
    using Fsel.Interaction.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CheckHarmfulContentWordsQuery : IRequest<MethodResult<bool>>
    {
        public string? Content { get; set; }
    }

    public class CheckHarmfulContentWordsQueryHandler : IRequestHandler<CheckHarmfulContentWordsQuery, MethodResult<bool>>
    {
        private readonly IHarmfulContentService _harmfulContentService;
        private readonly AppSetting _appSetting;

        public CheckHarmfulContentWordsQueryHandler(IHarmfulContentService harmfulContentService, AppSetting appSetting)
        {
            _harmfulContentService = harmfulContentService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(CheckHarmfulContentWordsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.Content))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            bool isHarmfulContent;
            try
            {
                var checkHarmfulContentResult = await _harmfulContentService.CheckHarmfulContentWords(new CheckHarmfulContentWordsModel
                {
                    Text = request.Content,
                    Categories = HarmfulSetting.Categories,
                    OutputType = HarmfulSetting.OutputType
                }, _appSetting.HarmfulContentConfig?.Version);

                if (checkHarmfulContentResult.StatusCode != HttpStatusCode.OK)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                    return methodResult;
                }

                var harmfulContent = checkHarmfulContentResult.Content?.CategoriesAnalysis?.ToList();

                isHarmfulContent = HarmfulContentHelper.CheckHarmfulContent(harmfulContent, EnumHarmfulContentType.Word);
            }
            catch
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            methodResult.Result = isHarmfulContent;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
