// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.QuestionFormCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteQuestionFormCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteQuestionFormCommandHandler : IRequestHandler<DeleteQuestionFormCommand, MethodResult<bool>>
    {
        private readonly IQuestionFormRepository _questionFormRepository;

        public DeleteQuestionFormCommandHandler(IQuestionFormRepository questionFormRepository)
        {
            _questionFormRepository = questionFormRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteQuestionFormCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var questionForm = await _questionFormRepository.GetByIdAsync(request.Id);
            if (questionForm == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questionForm));
                return methodResult;
            }

            #endregion Validation

            await _questionFormRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _questionFormRepository.DeleteAsync(questionForm);
                await _questionFormRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
