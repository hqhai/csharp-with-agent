// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.QuestionExplanationErrorCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.QuestionExplanationErrors;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IQuestionExplanationErrorRepository _questionExplanationErrorRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public CreateQuestionExplanationErrorCommandHandler(IMapper mapper,
            IQuestionRepository questionRepository,
            IVideoResultRepository videoResultRepository,
            IQuestionExplanationErrorRepository questionExplanationErrorRepository,
            AuthContext authContext,
            IUserService userService)
        {
            _mapper = mapper;
            _questionRepository = questionRepository;
            _videoResultRepository = videoResultRepository;
            _questionExplanationErrorRepository = questionExplanationErrorRepository;
            _authContext = authContext;
            _userService = userService;
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

            var question = await _questionRepository.GetByIdAsync(request.QuestionId);
            if (question == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                return methodResult;
            }

            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var questionExplanationError = _mapper.Map<QuestionExplanationError>(request);
            questionExplanationError.StudentId = student.Id;
            if (!questionExplanationError.IsValid())
            {
                methodResult.AddErrorBadRequest(questionExplanationError.ErrorMessages);
                return methodResult;
            }
            await _questionExplanationErrorRepository.ExecuteTransactionAsync(async () =>
            {
                questionExplanationError = _questionExplanationErrorRepository.Add(questionExplanationError);
                await _questionExplanationErrorRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
