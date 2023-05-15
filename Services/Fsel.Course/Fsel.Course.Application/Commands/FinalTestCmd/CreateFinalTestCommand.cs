// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.FinalTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.FinalTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateFinalTestCommand : CreateFinalTestCommandModel, IRequest<MethodResult<FinalTestModel>>
    {
    }

    public class CreateFinalTestCommandHandler : IRequestHandler<CreateFinalTestCommand, MethodResult<FinalTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IFinalTestRepository _finalTestRepository;

        public CreateFinalTestCommandHandler(IMapper mapper, QuestionTypeConverter questionTypeConverter, IFinalTestRepository finalTestRepository)
        {
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
            _finalTestRepository = finalTestRepository;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(CreateFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();
            if (request.Exercises == null || request.Exercises.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull), nameof(request.Exercises));
                return methodResult;
            }
            FinalTest finalTest = _mapper.Map<FinalTest>(request);

            request.Exercises.ForEach(x =>
            {
                if (x == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull), nameof(request.Exercises), x);
                }
                else
                {
                    Exercise excercise = _mapper.Map<Exercise>(x);
                    finalTest.FinalTestExercises.Add(new FinalTestExercise
                    {
                        Exercise = excercise
                    });
                    if (x.Questions == null || x.Questions.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(x.Questions), x.Questions);
                    }
                    x.Questions.ForEach(q =>
                    {
                        if (q == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(x.Questions), q);
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

                            excercise.ExerciseQuestions.Add(new ExerciseQuestion
                            {
                                Question = question
                            });

                            if (!question.IsValid())
                            {
                                methodResult.AddErrorBadRequest(question.ErrorMessages);
                            }
                        }
                    });
                    if (!excercise.IsValid())
                    {
                        methodResult.AddErrorBadRequest(excercise.ErrorMessages);
                    }
                }
            });
            if (!finalTest.IsValid())
            {
                methodResult.AddErrorBadRequest(finalTest.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }
            await _finalTestRepository.ExecuteTransactionAsync(async () =>
            {
                finalTest = _finalTestRepository.Add(finalTest);

                await _finalTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FinalTestModel>(finalTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
