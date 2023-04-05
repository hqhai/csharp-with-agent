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
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateHomeWorkCommand : UpdateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class UpdateHomeWorkCommandHandler : IRequestHandler<UpdateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionTypeValidation _questionTypeValidation;
        private readonly QuestionTypeCountConverter _questionTypeCountConverter;

        public UpdateHomeWorkCommandHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , IQuestionRepository questionRepository
            , QuestionTypeValidation questionTypeValidation
            , QuestionTypeCountConverter questionTypeCountConverter)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _questionRepository = questionRepository;
            _questionTypeValidation = questionTypeValidation;
            _questionTypeCountConverter = questionTypeCountConverter;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(UpdateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();
            var homeWork = await _homeWorkRepository.GetByIdAsync(request.Id);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorksNull),
                                               nameof(request.Id), request.Id);
                return methodResult;
            }
            _mapper.Map(request, homeWork);

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
                homeWork = _homeWorkRepository.Update(homeWork);
                await _homeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<HomeWorkModel>(homeWork);
                return methodResult;
            });

            return methodResult;
        }
    }
}
