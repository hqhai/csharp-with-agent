// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.LessonCmd.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteLessonCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, MethodResult<bool>>
    {
        private readonly ILessonRepository _lessonRepository;

        public DeleteLessonCommandHandler(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var lesson = await _lessonRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }

            if (lesson.Status != Shared.Enums.EnumStatus.InActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.StatusLessonNotInActive), nameof(lesson.Status), lesson.Status);
                return methodResult;
            }

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                await _lessonRepository.DeleteAsync(lesson);
                await _lessonRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
