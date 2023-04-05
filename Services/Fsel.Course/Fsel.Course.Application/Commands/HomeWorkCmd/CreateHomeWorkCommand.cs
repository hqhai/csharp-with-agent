// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.HomeWorkCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
    using Fsel.Course.Domain.Models.EntityModels;

    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateHomeWorkCommand : CreateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class CreateHomeWorkCommandHandler : IRequestHandler<CreateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IQuestionRepository _questionRepository;

        public CreateHomeWorkCommandHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , IQuestionRepository questionRepository)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(CreateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            HomeWork homeWork = _mapper.Map<HomeWork>(request);
            if (!homeWork.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(homeWork.ErrorMessages);
                return methodResult;
            }
            if (request.QuestionIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QusetionIdNotCorrect), nameof(request.QuestionIds), request.QuestionIds);
                return methodResult;
            }
            if (_questionRepository.IsIdsInValid(request.QuestionIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotCorrect), nameof(request.QuestionIds), request.QuestionIds);
                return methodResult;
            }
            await _homeWorkRepository.ExecuteTransactionAsync(async () =>
            {
                homeWork.HomeWorkQuestions = request.QuestionIds.Select(x => new HomeWorkQuestion
                {
                    QuestionId = x,
                }).ToList();
                homeWork = _homeWorkRepository.Add(homeWork);
                await _homeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<HomeWorkModel>(homeWork);
                return methodResult;
            });

            return methodResult;
        }
    }
}
