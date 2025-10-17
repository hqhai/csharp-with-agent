// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TopicCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Topics;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateTopicCommand : UpdateTopicCommandModel, IRequest<MethodResult<TopicModel>>
    {
    }

    public class UpdateTopicCommandHandler : IRequestHandler<UpdateTopicCommand, MethodResult<TopicModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITopicRepository _topicRepository;

        public UpdateTopicCommandHandler(IMapper mapper, ITopicRepository topicRepository)
        {
            _mapper = mapper;
            _topicRepository = topicRepository;
        }

        public async Task<MethodResult<TopicModel>> Handle(UpdateTopicCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TopicModel> methodResult = new MethodResult<TopicModel>();

            var topic = await _topicRepository.GetByIdAsync(request.Id);
            if (topic == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(topic), request.Id);
                return methodResult;
            }

            if (await _topicRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            _mapper.Map(request, topic);
            if (!topic.IsValid())
            {
                methodResult.AddErrorBadRequest(topic.ErrorMessages);
                return methodResult;
            }

            await _topicRepository.ExecuteTransactionAsync(async () =>
            {
                _topicRepository.Update(topic);
                await _topicRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TopicModel>(topic);
                return methodResult;
            });
            return methodResult;
        }
    }
}
