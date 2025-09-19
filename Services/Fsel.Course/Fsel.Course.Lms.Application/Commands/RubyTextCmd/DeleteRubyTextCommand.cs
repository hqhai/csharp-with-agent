// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.RubyTextCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteRubyTextCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteRubyTextCommandHandler : IRequestHandler<DeleteRubyTextCommand, MethodResult<bool>>
    {
        private readonly IRubyTextRepository _rubyTextRepository;

        public DeleteRubyTextCommandHandler(IRubyTextRepository rubyTextRepository)
        {
            _rubyTextRepository = rubyTextRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteRubyTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var rubyText = await _rubyTextRepository.GetByIdAsync(request.Id);

            if (rubyText == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(rubyText));
                return methodResult;
            }

            await _rubyTextRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _rubyTextRepository.DeleteAsync(rubyText);
                await _rubyTextRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
