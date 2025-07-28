// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CourseCmd
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ActiveCourseCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ActiveCourseCommandHandler : IRequestHandler<ActiveCourseCommand, MethodResult<bool>>
    {
        private readonly ICourseRepository _courseRepository;

        public ActiveCourseCommandHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<bool>> Handle(ActiveCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var course = await _courseRepository.Queryable
                                                 .Where(e => e.Id == request.Id)
                                                 .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course.IsArchive = true;
                _courseRepository.Update(course);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
