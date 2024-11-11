// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.PopupCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.MaintainConfigs;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SetTimePopupMaintainCommand : SetTimePopupMaintainCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SetTimePopupMaintainCommandHandler : IRequestHandler<SetTimePopupMaintainCommand, MethodResult<bool>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IBannerStudentRepository _bannerStudentRepository;
        private readonly IMapper _mapper;

        public SetTimePopupMaintainCommandHandler(IBannerRepository bannerRepository, IBannerStudentRepository bannerStudentRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _bannerStudentRepository = bannerStudentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(SetTimePopupMaintainCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var bannerUpate = await _bannerRepository.GetByIdAsync(request.BannerId);
            if (request.StartDate > DateTime.UtcNow && request.StartDate < request.EndDate && bannerUpate != null)
            {
                List<BannerStudent> deleteSendingUser = await _bannerStudentRepository.Queryable.Where(x => x.BannerId == request.BannerId).ToListAsync(cancellationToken);
                if (deleteSendingUser != null)
                {
                    await _bannerStudentRepository.DeleteListAsync(deleteSendingUser);
                    await _bannerStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                }
                bannerUpate.StartDate = request.StartDate;
                bannerUpate.EndDate = request.EndDate;
                bannerUpate.DisplayStartDate = request.StartDate.AddHours(-request.NotificationTime);
                await _bannerRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                methodResult.StatusCode = StatusCodes.Status406NotAcceptable;
                methodResult.Result = false;
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }


}
