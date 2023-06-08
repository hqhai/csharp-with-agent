// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ExtraPracticeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractices;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateExtraPracticeCommand : UpdateExtraPracticeCommandModel, IRequest<MethodResult<ExtraPracticeModel>>
    {
    }

    public class UpdateExtraPracticeCommandHandler : IRequestHandler<UpdateExtraPracticeCommand, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly ExtraPracticeConverter _extraPracticeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExerciseRepository _exerciseRepository;

        public UpdateExtraPracticeCommandHandler(IExtraPracticeRepository extraPracticeRepository,
            IMapper mapper,
            ExtraPracticeConverter extraPracticeConverter,
            IQuestionRepository questionRepository,
            IExerciseRepository exerciseRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _extraPracticeConverter = extraPracticeConverter;
            _questionRepository = questionRepository;
            _exerciseRepository = exerciseRepository;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(UpdateExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();

            var extraPractice = await _extraPracticeRepository.GetIncludeByIdAsync(request.Id);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (extraPractice.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeInActiveState), nameof(extraPractice.IsActive), extraPractice.IsActive);
                return methodResult;
            }

            List<Exercise> exercises = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise ?? new Exercise()).ToList();
            List<Question> questions = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question ?? new Question()).ToList();

            _mapper.Map(request, extraPractice);
            var method = await _extraPracticeConverter.UpdateExtraPractice(extraPractice, request);
            if (!method.IsOK)
            {
                methodResult.AddError(method.ErrorMessages);
                return methodResult;
            }

            await _extraPracticeRepository.ExecuteTransactionAsync(async () =>
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

                extraPractice = _extraPracticeRepository.Update(extraPractice);

                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ExtraPracticeModel>(extraPractice);
                return methodResult;
            });
            return methodResult;
        }
    }
}
