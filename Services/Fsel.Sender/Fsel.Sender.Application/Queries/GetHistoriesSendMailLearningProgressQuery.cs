// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Queries
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetHistoriesSendMailLearningProgressQuery : IRequest<MethodResult<IList<HistorySendMailLearningProgressModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetHistoriesSendMailLearningProgressQueryHandler : IRequestHandler<GetHistoriesSendMailLearningProgressQuery, MethodResult<IList<HistorySendMailLearningProgressModel>>>
    {
        private readonly IMessageHistoryRepository _messageHistoryRepository;
        private readonly IMapper _mapper;

        public GetHistoriesSendMailLearningProgressQueryHandler(IMessageHistoryRepository messageHistoryRepository, IMapper mapper)
        {
            _messageHistoryRepository = messageHistoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<HistorySendMailLearningProgressModel>>> Handle(GetHistoriesSendMailLearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<HistorySendMailLearningProgressModel>>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                return methodResult;
            }

            var messageHistoryTypes = new List<EnumSenderTemplate>()
            {
                EnumSenderTemplate.WeeklyProgressReport1,
                EnumSenderTemplate.WeeklyProgressReport2,
                EnumSenderTemplate.WeeklyProgressReport3,
                EnumSenderTemplate.WeeklyProgressReport4,
                EnumSenderTemplate.WeeklyProgressReport5,
                EnumSenderTemplate.WeeklyProgressReport6,
                EnumSenderTemplate.WeeklyProgressReport7,
                EnumSenderTemplate.WeeklyProgressReport8,
                EnumSenderTemplate.WeeklyProgressReport9,
                EnumSenderTemplate.LearningProgressWarning
            };

            var messageHistories = await _messageHistoryRepository.Queryable.WhereBulkContains(request.UserIds, p => p.ReceiverId).ToListAsync(cancellationToken);

            messageHistories = messageHistories.Where(m => m.Template.HasValue && messageHistoryTypes.Contains(m.Template.Value)).OrderByDescending(p => p.CreatedDate).ToList();

            methodResult.Result = _mapper.Map<IList<HistorySendMailLearningProgressModel>>(messageHistories);
            return methodResult;
        }
    }
}
