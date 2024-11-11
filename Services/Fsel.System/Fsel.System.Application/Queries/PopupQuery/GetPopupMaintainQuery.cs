// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.BannerStudentCmd;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPopupMaintainQuery : IRequest<MethodResult<GetPopUpMaintainQueryModel>>
    {
    }

    public class GetPopUpMaintainHandler : IRequestHandler<GetPopupMaintainQuery, MethodResult<GetPopUpMaintainQueryModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IBannerStudentRepository _bannerStudentRepository;
        private readonly IMediator _mediator;

        public GetPopUpMaintainHandler(IBannerRepository bannerRepository,
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

        public async Task<MethodResult<GetPopUpMaintainQueryModel>> Handle(GetPopupMaintainQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GetPopUpMaintainQueryModel> methodResult = new MethodResult<GetPopUpMaintainQueryModel>();

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

            var banner = await _bannerRepository.Queryable.FirstOrDefaultAsync(x => x.DisplayStartDate <= DateTime.UtcNow && x.EndDate >= DateTime.UtcNow && x.Name == ValueSettings.PopupName.MaintainPopup, cancellationToken);
            if (banner == null || await _bannerStudentRepository.Queryable.AnyAsync(x => x.StudentId == student.Id && x.BannerId == banner.Id, cancellationToken))
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var sendBanner = await _mediator.Send(new CreateBannerStudentCommand { BannerId = banner.Id, StudentId = student.Id }, cancellationToken);
            if (!sendBanner.IsOK)
            {
                methodResult.AddErrorBadRequest(sendBanner.ErrorMessages);
                return methodResult;
            }

            var contentWithDates = banner.Content
                                        .Replace("{Start_time}", banner.StartDate.ToString("HH:mm"))
                                        .Replace("{End_time}", banner.EndDate.ToString("HH:mm"))
                                        .Replace("{Date}", banner.EndDate.ToString("dd/MM/yyyy"));

            IList<string> contentList = contentWithDates.Split(new[] { "\n" }, StringSplitOptions.None).Select(line => line.Trim()).ToList();

            methodResult.Result = new GetPopUpMaintainQueryModel { Content = contentList };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }


    }
}
