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
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateHomeWorkCommand : CreateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class CreateHomeWorkCommandHandler : IRequestHandler<CreateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly QuestionTypeValidation _questionTypeValidation;
        private readonly QuestionTypeCountConverter _questionTypeCountConverter;

        public CreateHomeWorkCommandHandler(IMapper mapper
            , QuestionTypeValidation questionTypeValidation
            , QuestionTypeCountConverter questionTypeCountConverter
            , IHomeWorkRepository homeWorkRepository)
        {
            _mapper = mapper;
            _questionTypeValidation = questionTypeValidation;
            _questionTypeCountConverter = questionTypeCountConverter;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(CreateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            HomeWork homeWork = _mapper.Map<HomeWork>(request);

            if (request.Questions == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QusetionIdNotCorrect), nameof(request.Questions), request.Questions);
                return methodResult;
            }

            request.Questions.ForEach(q =>
            {
                if (q == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotCorrect), nameof(request.Questions));
                }
                else
                {
                    Question question = _mapper.Map<Question>(q);
                    var ischeck = _questionTypeValidation.TryParseQuestionType(question.Config, question.QuestionType);
                    if (!ischeck)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config));
                    }
                    question.CorrectTotal = question.Ungraded ? default : _questionTypeCountConverter.GetTotalCorrectByQuestionType(question.Config, question.QuestionType) ?? default;

                    homeWork.HomeWorkQuestions.Add(new HomeWorkQuestion
                    {
                        Question = question
                    });

                    if (!question.IsValid())
                    {
                        methodResult.AddErrorBadRequest(question.ErrorMessages);
                    }
                }
            });

            if (!homeWork.IsValid())
            {
                methodResult.AddErrorBadRequest(homeWork.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
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
