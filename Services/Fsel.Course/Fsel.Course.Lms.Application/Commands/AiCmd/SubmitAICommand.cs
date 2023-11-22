// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Ais;
    using Fsel.Course.Lms.Application.Services.AiService;
    using Fsel.Course.Lms.Application.Services.AiService.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SubmitAICommand : SubmitAiCommandModel, IRequest<MethodResult<AIResponseModel>>
    {
        public Guid Id { get; set; }
    }

    public class SubmitAICommandHandler : IRequestHandler<SubmitAICommand, MethodResult<AIResponseModel>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IOpenAIService _aiService;

        public SubmitAICommandHandler(IClassForumResultRepository classForumResultRepository, IOpenAIService aiService)
        {
            _classForumResultRepository = classForumResultRepository;
            _aiService = aiService;
        }

        async Task<MethodResult<AIResponseModel>> IRequestHandler<SubmitAICommand, MethodResult<AIResponseModel>>.Handle(SubmitAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AIResponseModel> methodResult = new MethodResult<AIResponseModel>();

            var classForumResult = await _classForumResultRepository.Queryable.Where(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken);

            /* if (classForumResult != null)
             {
                 methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                 return methodResult;
             }*/

            _classForumResultRepository.Add(classForumResult!);
            await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
