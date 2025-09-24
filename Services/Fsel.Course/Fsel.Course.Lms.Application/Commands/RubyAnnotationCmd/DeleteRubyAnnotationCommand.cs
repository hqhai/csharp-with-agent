// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.RubyAnnotationCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteRubyAnnotationCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteRubyAnnotationCommandHandler : IRequestHandler<DeleteRubyAnnotationCommand, MethodResult<bool>>
    {
        private readonly IRubyAnnotationRepository _rubyAnnotationRepository;

        public DeleteRubyAnnotationCommandHandler(IRubyAnnotationRepository rubyAnnotationRepository)
        {
            _rubyAnnotationRepository = rubyAnnotationRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteRubyAnnotationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var rubyText = await _rubyAnnotationRepository.GetByIdAsync(request.Id);

            if (rubyText == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(rubyText));
                return methodResult;
            }

            await _rubyAnnotationRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _rubyAnnotationRepository.DeleteAsync(rubyText);
                await _rubyAnnotationRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
