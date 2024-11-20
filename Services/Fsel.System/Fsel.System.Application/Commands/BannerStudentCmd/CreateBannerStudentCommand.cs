// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.BannerStudents;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateBannerStudentCommand : CreateBannerStudentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateBannerStudentCommandHandler : IRequestHandler<CreateBannerStudentCommand, MethodResult<bool>>
    {
        private readonly IBannerStudentRepository _bannerStudentRepository;

        public CreateBannerStudentCommandHandler(IBannerStudentRepository bannerStudentRepository)
        {
            _bannerStudentRepository = bannerStudentRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateBannerStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (await _bannerStudentRepository.Queryable.AnyAsync(x => x.StudentId == request.StudentId && x.BannerId == request.BannerId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.ThisStudentViewedThisBanner), nameof(request));
                return methodResult;
            }

            var newBannerStudent = new BannerStudent
            {
                BannerId = request.BannerId,
                StudentId = request.StudentId
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
