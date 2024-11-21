// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateBannerStudentInDayCommand : IRequest<MethodResult<bool>>
    {
        public Guid BannerId { get; set; }
    }

    public class CreateBannerStudentInDayCommandHandler : IRequestHandler<CreateBannerStudentInDayCommand, MethodResult<bool>>
    {
        private readonly IBannerStudentRepository _bannerStudentRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public CreateBannerStudentInDayCommandHandler(IBannerStudentRepository bannerStudentRepository,
                                                 AuthContext authContext,
                                                 IUserService userService)
        {
            _bannerStudentRepository = bannerStudentRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(CreateBannerStudentInDayCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

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

            if (await _bannerStudentRepository.Queryable.AnyAsync(x => x.StudentId == student.Id && x.BannerId == request.BannerId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.ThisStudentViewedThisBanner), nameof(request));
                return methodResult;
            }

            var newBannerStudent = new BannerStudent
            {
                BannerId = request.BannerId,
                StudentId = student.Id
            };

            await _bannerStudentRepository.ExecuteTransactionAsync(async () =>
            {
                _bannerStudentRepository.Add(newBannerStudent);
                await _bannerStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
