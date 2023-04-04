// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.LessonCmd
{
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

            #region Validation

            var lesson = await _lessonRepository.GetIncludeByIdAsync(request.Id);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist), nameof(request.Id), request?.Id);
                return methodResult;
            }

            if (!lesson.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(lesson.ErrorMessages);
                return methodResult;
            }

            var isLessonUsed = await _lessonRepository.IsLessonUsed(request.Id);
            if (isLessonUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            #endregion Validation

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _lessonRepository.DeleteAsync(lesson);
                await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
