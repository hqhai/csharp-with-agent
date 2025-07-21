namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.StudentEditHistoryCmd;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateExpiredDateStudentHasValueCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
        public DateTime ExpiredDate { get; set; }
    }

    public class UpdateExpiredDateStudentHasValueCommandHandler : IRequestHandler<UpdateExpiredDateStudentHasValueCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentEditHistoryRepository _studentEditHistoryRepository;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private const string? Description = "Chỉnh sửa ngày hết hạn";

        public UpdateExpiredDateStudentHasValueCommandHandler(IStudentRepository studentRepository, IStudentEditHistoryRepository studentEditHistoryRepository, AuthContext authContext, IMediator mediator)
        {
            _studentRepository = studentRepository;
            _studentEditHistoryRepository = studentEditHistoryRepository;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(UpdateExpiredDateStudentHasValueCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.StudentId), request.StudentId);
                return methodResult;
            }
            if (!student.ExpiredDate.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.StudentHasNotExpiredDate));
                return methodResult;
            }

            var role = _authContext.Roles?.FirstOrDefault();
            if (role != EnumRole.Admin.ToString())
            {
                var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                if (request.ExpiredDate.Date < student.ExpiredDate.Value.Date)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumStudentEditHistoryErrorCode.ExpiredDateInPast));
                    return methodResult;
                }
                else if (request.ExpiredDate.Date > student.ExpiredDate.Value.AddDays(7).Date)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumStudentEditHistoryErrorCode.ExceedsAllowedEditWindow));
                    return methodResult;
                }
                else if (await _studentEditHistoryRepository.Queryable.AnyAsync(p => p.CreatedUserId == _authContext.CurrentUserId && p.StudentId == request.StudentId, cancellationToken))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumStudentEditHistoryErrorCode.AlreadyEdited));
                    return methodResult;
                }
            }

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                var oldExpiredDate = student.ExpiredDate;
                student.ExpiredDate = request.ExpiredDate;
                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                var result = await _mediator.Send(new CreateStudentEditHistoryCommand()
                {
                    Type = EnumStudentEditHistoryType.EditExpiredDate,
                    StudentId = request.StudentId,
                    EditDetail = new StudentEditHistoryDetailModel
                    {
                        OldExpiredDate = oldExpiredDate,
                        NewExpiredDate = student.ExpiredDate
                    },
                    Description = Description
                });
                if (!result.IsOK)
                {
                    methodResult.AddError(result.ErrorMessages);
                    return methodResult;
                }
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
