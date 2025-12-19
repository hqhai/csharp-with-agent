// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TopicCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteTopicCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
        public Guid TopicId { get; set; }
    }

    public class DeleteTopicCommandHandler : IRequestHandler<DeleteTopicCommand, MethodResult<bool>>
    {
        private readonly ITopicRepository _topicRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public DeleteTopicCommandHandler(ITopicRepository topicRepository, IHomeWorkRepository homeWorkRepository)
        {
            _topicRepository = topicRepository;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteTopicCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (request.TopicId != request.Id)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.TopicId));
                return methodResult;
            }

            var topic = await _topicRepository.GetByIdAsync(request.Id);
            if (topic == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(topic), request.Id);
                return methodResult;
            }

            var topicAssigned = await _topicRepository.GetByIdAsync(request.TopicId);
            if (topicAssigned == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(topicAssigned), request.TopicId);
                return methodResult;
            }
            var homeWorks = await _homeWorkRepository.Queryable.Where(x => x.TopicId == topic.Id).ToListAsync(cancellationToken);
            foreach (var item in homeWorks)
            {
                item.TopicId = topicAssigned.Id;
            }
            await _topicRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _topicRepository.DeleteAsync(topic);
                await _topicRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _homeWorkRepository.UpdateList(homeWorks);
                await _homeWorkRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }
    }
}
