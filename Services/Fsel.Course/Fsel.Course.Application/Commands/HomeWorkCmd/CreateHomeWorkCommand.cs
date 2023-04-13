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
        private readonly QuestionTypeConverter _questionTypeConverter;

        public CreateHomeWorkCommandHandler(IMapper mapper
            , QuestionTypeConverter questionTypeConverter
            , IHomeWorkRepository homeWorkRepository)
        {
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(CreateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            HomeWork homeWork = _mapper.Map<HomeWork>(request);

            if (request.Questions == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNull), nameof(request.Questions), request.Questions);
                return methodResult;
            }

            request.Questions.ForEach(q =>
            {
                if (q == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(request.Questions), q);
                }
                else
                {
                    Question question = _mapper.Map<Question>(q);
                    var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                    if (config == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                    }
                    question.CorrectTotal = correctTotal;
                    if (!question.IsValid())
                    {
                        methodResult.AddErrorBadRequest(question.ErrorMessages);
                    }

                    homeWork.HomeWorkQuestions.Add(new HomeWorkQuestion
                    {
                        Question = question
                    });
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
