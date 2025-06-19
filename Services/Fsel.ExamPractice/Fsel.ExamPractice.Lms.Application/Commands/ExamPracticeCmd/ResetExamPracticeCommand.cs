// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ResetExamPracticeCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ResetExamPracticeCommandHandler : IRequestHandler<ResetExamPracticeCommand, MethodResult<bool>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeRetryRepository _examPracticeRetryRepository;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;

        public ResetExamPracticeCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            AuthContext authContext,
            IUserService userService,
            IExamPracticeRetryRepository examPracticeRetryRepository,
            IExamPracticeResultRepository examPracticeResultRepository)
        {
            _examPracticeRepository = examPracticeRepository;
            _authContext = authContext;
            _userService = userService;
            _examPracticeRetryRepository = examPracticeRetryRepository;
            _examPracticeResultRepository = examPracticeResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(ResetExamPracticeCommand request, CancellationToken cancellationToken)
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
            var examPractice = await _examPracticeRepository.GetByIdAsync(request.Id);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), request.Id);
                return methodResult;
            }
            // Note 5
            if (examPractice.Status != EnumExamPracticeStatus.Active)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.TestStatusUpdated), nameof(examPractice.Status), examPractice.Status);
                return methodResult;
            }
            var examPracticeRetry = await _examPracticeRetryRepository.Queryable.FirstOrDefaultAsync(x => x.ExamPracticeId == examPractice.Id && x.StudentId == student.Id, cancellationToken);
            if (examPracticeRetry == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeRetry));
                return methodResult;
            }
            if (examPracticeRetry.RetryCount == default)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.RetryLimitExceeded), nameof(examPracticeRetry.RetryCount));
                return methodResult;
            }
            var examPracticeResult = await _examPracticeResultRepository.Queryable
                                             .Where(x => x.ExamPracticeRetryId == examPracticeRetry.Id && x.WorkingStatus == EnumWorkingStatus.Active)
                                             .FirstOrDefaultAsync(cancellationToken);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult));
                return methodResult;
            }

            await _examPracticeRetryRepository.ExecuteTransactionAsync(async () =>
            {
                examPracticeRetry.RetryCount--;
                _examPracticeRetryRepository.Update(examPracticeRetry);
                await _examPracticeRetryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                examPracticeResult.WorkingStatus = EnumWorkingStatus.NotWorking;
                _examPracticeResultRepository.Update(examPracticeResult);
                await _examPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
