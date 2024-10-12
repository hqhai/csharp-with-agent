// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.System.Application.Commands.BannerStudentCmd;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetBannerToStudentQuery : IRequest<MethodResult<BannerModel>>
    {
    }

    public class GetBannerToStudentQueryHandler : IRequestHandler<GetBannerToStudentQuery, MethodResult<BannerModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IBannerStudentRepository _bannerStudentRepository;
        private readonly IMediator _mediator;

        public GetBannerToStudentQueryHandler(IBannerRepository bannerRepository,
                                              IMapper mapper,
                                              AuthContext authContext,
                                              IUserService userService,
                                              IBannerStudentRepository bannerStudentRepository,
                                              IMediator mediator)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _bannerStudentRepository = bannerStudentRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<BannerModel>> Handle(GetBannerToStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<BannerModel> methodResult = new MethodResult<BannerModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var banner = await _bannerRepository.Queryable.FirstOrDefaultAsync(x => x.StartDate <= DateTime.UtcNow && x.EndDate >= DateTime.UtcNow, cancellationToken);
            if (banner == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            if (await _bannerStudentRepository.Queryable.AnyAsync(x => x.StudentId == student.Id && x.BannerId == banner.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.ThisStudentViewedThisBanner), nameof(request));
                return methodResult;
            }

            var sendBanner = await _mediator.Send(new CreateBannerStudentCommand { BannerId = banner.Id, StudentId = student.Id }, cancellationToken);
            if (!sendBanner.IsOK)
            {
                methodResult.AddErrorBadRequest(sendBanner.ErrorMessages);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<BannerModel>(banner);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
