// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.QuestionExplanationErrorCmd
{
    using System.Linq;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.QuestionExplanationErrors;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateQuestionExplanationErrorCommand : CreateQuestionExplanationErrorCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateQuestionExplanationErrorCommandHandler : IRequestHandler<CreateQuestionExplanationErrorCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuestionExplanationErrorRepository _questionExplanationErrorRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ErrorExplainPublisher _errorExplainPublisher;

        public CreateQuestionExplanationErrorCommandHandler(IMapper mapper,
            IQuestionRepository questionRepository,
            IQuestionExplanationErrorRepository questionExplanationErrorRepository,
            AuthContext authContext,
            IUserService userService,
            IVideoResultRepository videoResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ErrorExplainPublisher errorExplainPublisher)
        {
            _mapper = mapper;
            _questionRepository = questionRepository;
            _questionExplanationErrorRepository = questionExplanationErrorRepository;
            _authContext = authContext;
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _errorExplainPublisher = errorExplainPublisher;
        }

        public async Task<MethodResult<bool>> Handle(CreateQuestionExplanationErrorCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var voidMethodResult = await ValidateObject(request);
            if (!voidMethodResult.IsOK)
            {
                methodResult.AddErrorBadRequest(voidMethodResult.ErrorMessages);
                return methodResult;
            }
            var question = await _questionRepository.GetByIdAsync(request.QuestionId);
            if (question == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                return methodResult;
            }

            var questionExplanationError = await _questionExplanationErrorRepository.Queryable.Where(x => x.QuestionId == question.Id && x.ObjectResultId == request.ObjectResultId && x.Status == EnumProcessedStatus.NotProcessed).FirstOrDefaultAsync(cancellationToken);
            if (questionExplanationError != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(questionExplanationError));
                return methodResult;
            }
            questionExplanationError = _mapper.Map<QuestionExplanationError>(request);
            questionExplanationError.StudentId = student.Id;
            if (!questionExplanationError.IsValid())
            {
                methodResult.AddErrorBadRequest(questionExplanationError.ErrorMessages);
                return methodResult;
            }
            await _questionExplanationErrorRepository.ExecuteTransactionAsync(async () =>
            {
                questionExplanationError = _questionExplanationErrorRepository.Add(questionExplanationError);
                await _questionExplanationErrorRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            await _errorExplainPublisher.Publish(request, cancellationToken);
            return methodResult;
        }

        private async Task<VoidMethodResult> ValidateObject(CreateQuestionExplanationErrorCommand request)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (request.ExplanationType == EnumFeatureExplanationType.Video)
            {
                var videoResult = await _videoResultRepository.GetByIdAsync(request.ObjectResultId);
                if (videoResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                    return methodResult;
                }
            }
            else if (request.ExplanationType == EnumFeatureExplanationType.HomeWork)
            {
                var homeWorkResult = await _homeWorkResultRepository.GetByIdAsync(request.ObjectResultId);
                if (homeWorkResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                    return methodResult;
                }
            }
            else if (request.ExplanationType == EnumFeatureExplanationType.FinalTest)
            {
                var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.ObjectResultId);
                if (finalTestResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                    return methodResult;
                }
            }
            else if (request.ExplanationType == EnumFeatureExplanationType.MockTest)
            {
                var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.ObjectResultId);
                if (mockTestResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                    return methodResult;
                }
            }
            else
            {
                var placementTestResult = await _placementTestResultRepository.GetByIdAsync(request.ObjectResultId);
                if (placementTestResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestResult));
                    return methodResult;
                }
            }
            return methodResult;
        }
    }
}
