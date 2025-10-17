using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Services.SystemService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    public class DeductCoinOfStudentCommand : DeductCoinOfStudentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class DeductCoinOfStudentCommandHandler : IRequestHandler<DeductCoinOfStudentCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;

        public DeductCoinOfStudentCommandHandler(UserManager<User> userManager, IHumanRepository humanRepository, IStudentRepository studentRepository, AuthContext authContext, ISystemService systemService)
        {
            _userManager = userManager;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _authContext = authContext;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(DeductCoinOfStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var student = await (from u in _userManager.Users
                                 join h in _humanRepository.Queryable on u.Id equals h.UserId
                                 join s in _studentRepository.Queryable on h.Id equals s.HumanId
                                 where u.Id == userId
                                 select s).FirstOrDefaultAsync(cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UserId));
                return methodResult;
            }

            if (student.NumberOfToken < request.NumberOfCoinsDeducted)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.UserId));
                return methodResult;
            }

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                long initialToken = student.NumberOfToken;

                student.NumberOfToken = initialToken - request.NumberOfCoinsDeducted;

                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var result = await _systemService.CreateHistoryDeductCoinOfStudent(new CreateHistoryDeductCoinOfStudentCommandModel()
                {
                    UserId = userId,
                    InitialToken = initialToken,
                    VolatileToken = request.NumberOfCoinsDeducted,
                    RemainToken = student.NumberOfToken,
                    Feature = request.Feature,
                    Mission = request.Mission,
                    ObjectId = request.ObjectId,
                    Config = request.Config,
                    Translations = request.Translations
                });

                if (!result.IsSuccessStatusCode)
                {
                    methodResult.AddError(result.Error);
                    return methodResult;
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
