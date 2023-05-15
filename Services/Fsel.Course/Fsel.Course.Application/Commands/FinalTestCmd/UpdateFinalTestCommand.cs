// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.FinalTestCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    public class UpdateFinalTestCommand : UpdateFinalTestCommandModel, IRequest<MethodResult<FinalTestModel>>
    {
    }

    public class UpdateFinalTestCommandHandler : IRequestHandler<UpdateFinalTestCommand, MethodResult<FinalTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExerciseRepository _exerciseRepository;

        public UpdateFinalTestCommandHandler(IMapper mapper
            , QuestionTypeConverter questionTypeConverter
            , IFinalTestRepository finalTestRepository
            , IQuestionRepository questionRepository
            , IExerciseRepository exerciseRepository)
        {
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
            _finalTestRepository = finalTestRepository;
            _questionRepository = questionRepository;
            _exerciseRepository = exerciseRepository;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(UpdateFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();

            #region Validation

            var finalTest = await _finalTestRepository.GetIncludeByIdAsync(request.Id);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestsNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (finalTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestInActiveState), nameof(finalTest.IsActive), finalTest.IsActive);
                return methodResult;
            }

            if (request.Exercises == null || request.Exercises.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull), nameof(request.Exercises));
                return methodResult;
            }
            List<Exercise> exercises = finalTest.FinalTestExercises.Select(x => x.Exercise!).ToList();
            List<Question>? questions = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question!).ToList();
            _mapper.Map(request, finalTest);
            finalTest.FinalTestExercises = new List<FinalTestExercise>();
            foreach (var exercise in request.Exercises)
            {
                if (exercise == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExerciseNull), nameof(exercise), exercise);
                }
                var newExercise = _mapper.Map<Exercise>(exercise);
                newExercise.ExerciseQuestions = new List<ExerciseQuestion>();
                if (exercise!.Questions == null || exercise.Questions.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(exercise.Questions), exercise.Questions);
                }
                foreach (var question in exercise.Questions!)
                {
                    if (question == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(question), question);
                    }
                    var newQuestion = _mapper.Map<Question>(question);
                    newExercise.ExerciseQuestions.Add(new ExerciseQuestion
                    {
                        Question = newQuestion
                    });
                    var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question!.Config, question.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                    if (config == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                    }
                    newQuestion.CorrectTotal = correctTotal;
                    if (!newQuestion.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                    }
                }
                finalTest.FinalTestExercises.Add(new FinalTestExercise
                {
                    Exercise = newExercise
                });
                if (!newExercise.IsValid())
                {
                    methodResult.AddErrorBadRequest(newExercise.ErrorMessages);
                }
            }
            if (!finalTest.IsValid())
            {
                methodResult.AddErrorBadRequest(finalTest.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _finalTestRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in exercises)
                {
                    await _exerciseRepository.DeleteAsync(item);
                }
                await _exerciseRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in questions)
                {
                    await _questionRepository.DeleteAsync(item);
                }
                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                finalTest = _finalTestRepository.Update(finalTest);
                await _finalTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<FinalTestModel>(finalTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
