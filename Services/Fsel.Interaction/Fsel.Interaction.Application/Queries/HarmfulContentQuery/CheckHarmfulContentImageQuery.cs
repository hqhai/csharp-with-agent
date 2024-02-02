// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.HarmfulContentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Application.Services.HarmfulContentService;
    using Fsel.Interaction.Application.Services.HarmfulContentService.Models;
    using Fsel.Interaction.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CheckHarmfulContentImageQuery : IRequest<MethodResult<bool>>
    {
        public string? FilePath { get; set; }
    }

    public class CheckHarmfulContentImageQueryHandler : IRequestHandler<CheckHarmfulContentImageQuery, MethodResult<bool>>
    {
        private readonly IHarmfulContentImageService _harmfulContentService;
        private readonly AppSetting _appSetting;

        public CheckHarmfulContentImageQueryHandler(IHarmfulContentImageService harmfulContentService, AppSetting appSetting)
        {
            _harmfulContentService = harmfulContentService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(CheckHarmfulContentImageQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.FilePath))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            bool isHarmfulContent;
            try
            {
                var checkHarmfulContentResult = await _harmfulContentService.CheckHarmfulContentImage(new CheckHarmfulContentImagesModel { DataRepresentation = "URL", Value = request.FilePath}, true);

                if (!checkHarmfulContentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                    return methodResult;
                }

                var harmfulContent = checkHarmfulContentResult.Content;

                isHarmfulContent = harmfulContent!.Result;
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
