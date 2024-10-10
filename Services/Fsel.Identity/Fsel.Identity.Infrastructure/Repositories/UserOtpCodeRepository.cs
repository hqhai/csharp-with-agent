// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Microsoft.EntityFrameworkCore;

    public class UserOtpCodeRepository : BaseRepository<UserOtpCode>, IUserOtpCodeRepository
    {
        private readonly AppSetting _appSetting;

        public UserOtpCodeRepository(UserDbContext dbContext, AppSetting appSetting, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
            _appSetting = appSetting;
        }

        public async Task<UserOtpCode?> GetUserOtpCodeAsync(string? otpCode, string? email)
        {
            if (!string.IsNullOrEmpty(email) && _appSetting.Otp != null && otpCode == _appSetting.Otp.ByPassOtpValue && _appSetting.Otp.IsByPassOtp)
            {
                return await Queryable.Include(x => x.User).FirstOrDefaultAsync(x => x.User != null && x.Status == EnumOtpCodeStatus.New && x.User.Email == email);
            }
            return await Queryable.FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && x.OTPCode == otpCode);
        }
    }
}
