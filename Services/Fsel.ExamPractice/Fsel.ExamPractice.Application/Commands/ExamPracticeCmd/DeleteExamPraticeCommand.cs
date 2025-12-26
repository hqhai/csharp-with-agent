// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Commands.ExamPracticeCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteExamPraticeCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteExamPraticeCommandHandler : IRequestHandler<DeleteExamPraticeCommand, MethodResult<bool>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;

        public DeleteExamPraticeCommandHandler(IExamPracticeRepository examPracticeRepository)
        {
            _examPracticeRepository = examPracticeRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteExamPraticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var examPractice = await _examPracticeRepository.GetByIdAsync(request.Id);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), request.Id);
                return methodResult;
            }

            var isUsingByClient = await _examPracticeRepository.IsUsingByClient(examPractice.Id);
            if (isUsingByClient)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.AlreadyExistsLearningData), nameof(examPractice), request.Id);
                return methodResult;
            }

            await _examPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                await _examPracticeRepository.DeleteAsync(examPractice);
                await _examPracticeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
