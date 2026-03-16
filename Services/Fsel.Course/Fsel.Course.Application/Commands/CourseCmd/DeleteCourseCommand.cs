// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class DeleteCourseCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, MethodResult<bool>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public DeleteCourseCommandHandler(ICourseRepository courseRepository, ICourseResultRepository courseResultRepository)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var course = await _courseRepository.Queryable.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            if (course.Status != EnumCourseStatus.InActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotInActive), nameof(course.Status), course.Status);
                return methodResult;
            }

            if (await _courseResultRepository.Queryable.AnyAsync(p => p.CourseId == course.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIsUsed), nameof(course), course.Id);
                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _courseRepository.DeleteAsync(course);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
