// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonNoteCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteLessonNoteCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteLessonNoteCommandHandler : IRequestHandler<DeleteLessonNoteCommand, MethodResult<bool>>
    {
        private readonly ILessonNoteRepository _lessonNoteRepository;

        public DeleteLessonNoteCommandHandler(ILessonNoteRepository lessonNoteRepository)
        {
            _lessonNoteRepository = lessonNoteRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteLessonNoteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var lessonNote = await _lessonNoteRepository.GetByIdAsync(request.Id);

            if (lessonNote == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }

            await _lessonNoteRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _lessonNoteRepository.DeleteAsync(lessonNote);
                await _lessonNoteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }
    }
}
