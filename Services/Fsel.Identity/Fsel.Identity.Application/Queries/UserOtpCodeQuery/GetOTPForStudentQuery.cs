namespace Fsel.Identity.Application.Queries.UserOtpCodeQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetOTPForStudentQuery : IRequest<MethodResult<OTPModel>>
    {
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class GetOTPForStudentQueryHandler : IRequestHandler<GetOTPForStudentQuery, MethodResult<OTPModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;

        public GetOTPForStudentQueryHandler(UserManager<User> userManager, IUserOtpCodeRepository userOtpCodeRepository)
        {
            _userManager = userManager;
            _userOtpCodeRepository = userOtpCodeRepository;
        }

        public async Task<MethodResult<OTPModel>> Handle(GetOTPForStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OTPModel>();

            if (!request.UserId.HasValue && string.IsNullOrEmpty(request.UserName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            User? user = null;

            if (request.UserId.HasValue)
            {
                user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);
            }
            else
            {
                user = await _userManager.Users.FirstOrDefaultAsync(p => p.UserName == request.UserName, cancellationToken);
            }

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var otpCodes = await _userOtpCodeRepository.Queryable.Where(p => p.UserId == user.Id).ToListAsync(cancellationToken);
            otpCodes = otpCodes.OrderByDescending(p => p.CreatedDate).ToList();

            var otpEmail = otpCodes.FirstOrDefault(p => p.Type == EnumUserOtpCodeType.Email);
            var otpPhoneNumber = otpCodes.FirstOrDefault(p => p.Type == EnumUserOtpCodeType.SMS);

            methodResult.Result = new OTPModel()
            {
                OTPEmail = otpEmail == null ? null : otpEmail.OTPCode,
                IsConfirmOTPEmail = otpEmail == null ? null : (otpEmail.Status == EnumOtpCodeStatus.Verified),
                OTPPhoneNumber = otpPhoneNumber == null ? null : otpPhoneNumber.OTPCode,
                IsConfirmOTPPhoneNumber = otpPhoneNumber == null ? null : (otpPhoneNumber.Status == EnumOtpCodeStatus.Verified)
            };
            return methodResult;
        }
    }
}
