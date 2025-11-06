// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendEmailCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Sender.Domain.IRepositories;
    using Fsel.Sender.Domain.Models.Commands;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SaveMessageHistoryByTypeEmailCommand : SaveMessageHistoryByTypeEmailCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SaveMessageHistoryByTypeEmailCommandHandler : IRequestHandler<SaveMessageHistoryByTypeEmailCommand, MethodResult<bool>>
    {
        private readonly IMessageHistoryRepository _messageHistoryRepository;

        public SaveMessageHistoryByTypeEmailCommandHandler(IMessageHistoryRepository messageHistoryRepository)
        {
            _messageHistoryRepository = messageHistoryRepository;
        }

        public async Task<MethodResult<bool>> Handle(SaveMessageHistoryByTypeEmailCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            List<MessageHistory> messageHistories = new List<MessageHistory>();

            if (request.ToEmails != null && request.ToEmails.Any())
            {
                foreach (var toEmail in request.ToEmails)
                {
                    var receiver = request.Receivers.FirstOrDefault(p => p.Email == toEmail);

                    MessageHistory messageHistory = new MessageHistory
                    {
                        To = toEmail,
                        Type = EnumMessageHistoryType.Email,
                        Status = request.Status,
                        RequestBody = request.Content,
                        Template = request.Template,
                        ReceiverId = receiver?.ReceiverId
                    };

                    messageHistories.Add(messageHistory);
                }
            }

            if (request.CcEmails != null && request.CcEmails.Any())
            {
                foreach (var ccEmail in request.CcEmails)
                {
                    MessageHistory messageHistory = new MessageHistory
                    {
                        CC = ccEmail,
                        Type = EnumMessageHistoryType.Email,
                        Status = request.Status,
                        RequestBody = request.Content,
                        Template = request.Template
                    };

                    messageHistories.Add(messageHistory);
                }
            }

            if (request.BccEmails != null && request.BccEmails.Any())
            {
                foreach (var bCCEmail in request.BccEmails)
                {
                    MessageHistory messageHistory = new MessageHistory
                    {
                        BCC = bCCEmail,
                        Type = EnumMessageHistoryType.Email,
                        Status = request.Status,
                        RequestBody = request.Content,
                        Template = request.Template
                    };

                    messageHistories.Add(messageHistory);
                }
            }

            await _messageHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                await _messageHistoryRepository.AddList(messageHistories);
                await _messageHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
