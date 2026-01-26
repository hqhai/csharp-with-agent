// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteChatbotConfigCommand : IRequest<MethodResult<bool>>
    {
        public Guid UnitId { get; set; }
    }

    public class DeleteChatbotConfigCommandHandler : IRequestHandler<DeleteChatbotConfigCommand, MethodResult<bool>>
    {
        private readonly IChatbotConfigRepository _chatbotConfigRepository;

        public DeleteChatbotConfigCommandHandler(IChatbotConfigRepository chatbotConfigRepository)
        {
            _chatbotConfigRepository = chatbotConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteChatbotConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var chatbotConfig = await _chatbotConfigRepository.Queryable.Include(x => x.ChatbotSkillConfigs)
                                                              .Include(x => x.ChatbotTokenConfigs)
                                                              .Where(x => x.UnitId == request.UnitId)
                                                              .FirstOrDefaultAsync(cancellationToken);
            if (chatbotConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(chatbotConfig));
                return methodResult;
            }

            await _chatbotConfigRepository.ExecuteTransactionAsync(async () =>
            {
                await _chatbotConfigRepository.DeleteAsync(chatbotConfig);
                await _chatbotConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
