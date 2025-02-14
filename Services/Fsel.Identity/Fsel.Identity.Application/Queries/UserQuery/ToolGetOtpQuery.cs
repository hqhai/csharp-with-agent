// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ToolGetOtpQuery : IRequest<MethodResult<UserOtpCodeModel>>
    {
        public string? EmailOrNumberphone { get; set; }
        public EnumUserOtpCodeType Type { get; set; }
    }

    public class ToolGetOtpQueryHandler : IRequestHandler<ToolGetOtpQuery, MethodResult<UserOtpCodeModel>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IMapper _mapper;
        public ToolGetOtpQueryHandler(UserManager<User> userManager, IHumanRepository humanRepository, IUserOtpCodeRepository userOtpCodeRepository, IMapper mapper, IUserRepository userRepository)
        {
            _humanRepository = humanRepository;
            _userOtpCodeRepository = userOtpCodeRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<UserOtpCodeModel>> Handle(ToolGetOtpQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserOtpCodeModel>();

            request.EmailOrNumberphone = request.EmailOrNumberphone?.Trim() ?? string.Empty;
            var emailQuery = _userManager.Users.Where(x => x.Email != null && x.UserName == request.EmailOrNumberphone).Select(x => x.Id);
            var phoneQuery = _userManager.Users.Where(x => x.PhoneNumber != null && x.PhoneNumber == request.EmailOrNumberphone && x.UserName == request.EmailOrNumberphone).Select(x => x.Id);
            Guid? userId = emailQuery.Union(phoneQuery).FirstOrDefault();

            var userOTP = _userOtpCodeRepository.Queryable.Where(x => x.UserId == userId && x.Type == request.Type)
                                                          .OrderByDescending(x => x.CreatedDate)
                                                          .FirstOrDefault();

            UserOtpCodeModel userOtpResult = _mapper.Map<UserOtpCodeModel>(userOTP);
            methodResult.Result = userOtpResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
