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
        private readonly ILessonResultRepository _lessonResultRepository;

        public DeleteLessonCommandHandler(ILessonRepository lessonRepository, ILessonResultRepository lessonResultRepository)
        {
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
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

            if (await _lessonResultRepository.ReadQueryable.AnyAsync(p => p.LessonId == lesson.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonUsed), nameof(lesson), lesson.Id);
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
