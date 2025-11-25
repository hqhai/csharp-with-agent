// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MassTransit.Internals;
    using Microsoft.EntityFrameworkCore;

    public class UserOtpCodeRepository : BaseRepository<UserOtpCode>, IUserOtpCodeRepository
    {
        private readonly AppSetting _appSetting;

        public UserOtpCodeRepository(UserDbContext dbContext, AppSetting appSetting, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
            _appSetting = appSetting;
        }

        public async Task<UserOtpCode?> GetUserOtpCodeAsync(string? otpCode, string? email, string? phoneNumber, EnumUserOtpCodeType otpCodeType)
        {
            if (_appSetting.Otp != null && otpCode == _appSetting.Otp.ByPassOtpValue && _appSetting.Otp.IsByPassOtp)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    return await Queryable.FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && x.User != null && x.User.Email == email.Trim() && x.Type == otpCodeType);
                }

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    return await Queryable.FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && x.User != null && x.User.UserName == phoneNumber.Trim() && x.Type == otpCodeType);
                }
            }
            if (!string.IsNullOrEmpty(email))
            {
                return await Queryable.FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && x.User != null && x.User.Email == email.Trim() && x.OtpCode == otpCode && x.Type == EnumUserOtpCodeType.Email);
            }
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                return await Queryable.FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && x.User != null && x.User.UserName == phoneNumber.Trim() && x.OtpCode == otpCode && x.Type == EnumUserOtpCodeType.SMS);
            }
            return null;
        }

        public async Task<UserOtpCode?> GetUserOtpCodeAsync(string? otpCode, string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return null;
            }
            return await Queryable.FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && x.User != null && x.User.PhoneNumber == phoneNumber.Trim() && x.OtpCode == otpCode && x.Type == EnumUserOtpCodeType.SMS);
        }
    }
}
