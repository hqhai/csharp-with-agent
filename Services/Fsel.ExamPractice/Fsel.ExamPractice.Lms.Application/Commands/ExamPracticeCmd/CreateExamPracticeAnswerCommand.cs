// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAnswers;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;

    public class CreateExamPracticeAnswerCommand : CreateExamPracticeAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateExamPracticeAnswerCommandHandler : IRequestHandler<CreateExamPracticeAnswerCommand, MethodResult<bool>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;

        public CreateExamPracticeAnswerCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            AuthContext authContext,
            IUserService userService,
            IExamPracticeResultRepository examPracticeResultRepository)
        {
            _examPracticeRepository = examPracticeRepository;
            _authContext = authContext;
            _userService = userService;
            _examPracticeResultRepository = examPracticeResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateExamPracticeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(request.ExamPracticeResultId);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult), request.ExamPracticeResultId);
                return methodResult;
            }
            if (examPracticeResult.Status == Domain.Enums.EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(examPracticeResult.Status), examPracticeResult.Status);
                return methodResult;
            }
            var examPractice = await _examPracticeRepository.GetByIdAsync(examPracticeResult.ExamPracticeId);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), examPracticeResult.ExamPracticeId);
                return methodResult;
            }
            return methodResult;
        }
    }
}
