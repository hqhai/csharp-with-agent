// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.HomeWorkCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkCommand : CreateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class CreateHomeWorkCommandHandler : IRequestHandler<CreateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public CreateHomeWorkCommandHandler(IMapper mapper
            , QuestionConverter questionConverter
            , IHomeWorkRepository homeWorkRepository)
        {
            _mapper = mapper;
            _questionConverter = questionConverter;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(CreateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();
            var isExistCode = await _homeWorkRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.Type == request.Type, cancellationToken);
            if (isExistCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            HomeWork homeWork = _mapper.Map<HomeWork>(request);
            if (request.Questions == null || request.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Questions));
                return methodResult;
            }
            foreach (var question in request.Questions)
            {
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Questions));
                    return methodResult;
                }
                else
                {
                    var newQuestion = _mapper.Map<Question>(question);
                    var method = _questionConverter.HandleQuestion(newQuestion, true);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    homeWork.HomeWorkQuestions.Add(new HomeWorkQuestion
                    {
                        Question = method.Result
                    });
                }
            }
            if (!homeWork.IsValid())
            {
                methodResult.AddErrorBadRequest(homeWork.ErrorMessages);
                return methodResult;
            }

            await _homeWorkRepository.ExecuteTransactionAsync(async () =>
            {
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
