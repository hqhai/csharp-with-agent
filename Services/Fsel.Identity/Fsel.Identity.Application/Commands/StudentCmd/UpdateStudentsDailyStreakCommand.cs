// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStudentsDailyStreakCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
        public long NumberOfToken { get; set; }
    }

    public class UpdateStudentsDailyStreakCommandHandler : IRequestHandler<UpdateStudentsDailyStreakCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public UpdateStudentsDailyStreakCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStudentsDailyStreakCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            student.NumberOfToken += request.NumberOfToken;
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
