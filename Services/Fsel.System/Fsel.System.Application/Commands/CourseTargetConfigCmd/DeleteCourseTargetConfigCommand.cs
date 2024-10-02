// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseTargetConfigCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteCourseTargetConfigCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCourseTargetConfigCommandHandler : IRequestHandler<DeleteCourseTargetConfigCommand, MethodResult<bool>>
    {
        private readonly ICourseTargetConfigRepository _courseTargetConfigRepository;

        public DeleteCourseTargetConfigCommandHandler(ICourseTargetConfigRepository courseTargetConfigRepository)
        {
            _courseTargetConfigRepository = courseTargetConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCourseTargetConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var courseTargetConfig = await _courseTargetConfigRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (courseTargetConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), nameof(request.Id));
                return methodResult;
            }

            await _courseTargetConfigRepository.ExecuteTransactionAsync(async () =>
            {
                await _courseTargetConfigRepository.DeleteAsync(courseTargetConfig);
                await _courseTargetConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
