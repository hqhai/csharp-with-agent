// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.LessonCmd.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStatusLessonCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }

        public EnumStatus Status { get; set; }
    }

    public class UpdateStatusLessonCommandHandler : IRequestHandler<UpdateStatusLessonCommand, MethodResult<bool>>
    {
        private readonly ILessonRepository _lessonRepository;

        public UpdateStatusLessonCommandHandler(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var lesson = await _lessonRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }

            if (request.Status == EnumStatus.Active)
            {
                var checkCode = await _lessonRepository.Queryable.AnyAsync(x => x.Status == EnumStatus.Active && x.Id != request.Id && x.Name == lesson.Name, cancellationToken).ConfigureAwait(false);
                if (checkCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.CodeAlreadyExist), nameof(checkCode), nameof(lesson.Name));
                    return methodResult;
                }
            }

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                lesson.Status = request.Status;
                _lessonRepository.Update(lesson);
                await _lessonRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
