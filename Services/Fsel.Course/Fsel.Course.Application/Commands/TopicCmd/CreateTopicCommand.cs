// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TopicCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Topics;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateTopicCommand : CreateTopicCommandModel, IRequest<MethodResult<TopicModel>>
    {
    }

    public class CreateTopicCommandHandler : IRequestHandler<CreateTopicCommand, MethodResult<TopicModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITopicRepository _topicRepository;

        public CreateTopicCommandHandler(IMapper mapper, ITopicRepository topicRepository)
        {
            _mapper = mapper;
            _topicRepository = topicRepository;
        }

        public async Task<MethodResult<TopicModel>> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TopicModel> methodResult = new MethodResult<TopicModel>();

            if (await _topicRepository.Queryable.AnyAsync(x => x.Code == request.Code, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            var topic = _mapper.Map<Topic>(request);
            if (!topic.IsValid())
            {
                methodResult.AddErrorBadRequest(topic.ErrorMessages);
                return methodResult;
            }

            await _topicRepository.ExecuteTransactionAsync(async () =>
            {
                _topicRepository.Add(topic);
                await _topicRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TopicModel>(topic);
                return methodResult;
            });
            return methodResult;
        }
    }
}
