// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.SpaceShipCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using MediatR;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
    using Microsoft.AspNetCore.Http;
    using Fsel.Common.Enums.ErrorCodes;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Cms.PlanetDefender.Domain.Entities;

    public class BuySpaceShipCommand : IRequest<MethodResult<bool>>
    {
        public Guid SpaceShip { get; set; }
    }
    public class BuySpaceShipCommandHandler : IRequestHandler<BuySpaceShipCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly ISpaceShipRepository _spaceShipRepository;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly AuthContext _authContext;

        public BuySpaceShipCommandHandler(IUserService userService, ISpaceShipRepository spaceShipRepository, IStudentGameInfoRepository studentGameInfoRepository, AuthContext authContext)
        {
            _userService = userService;
            _spaceShipRepository = spaceShipRepository;
            _studentGameInfoRepository = studentGameInfoRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(BuySpaceShipCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;


            var spaceShip = await _spaceShipRepository.GetByIdAsync(request.SpaceShip);
            if (spaceShip == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (spaceShip.Price > student?.NumberOfToken)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Min));
                return methodResult;
            }

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Include(p => p.StudentSpaceShips).FirstOrDefaultAsync(x => x.StudentId == student!.Id, cancellationToken);

            if (studentGameInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (studentGameInfo.StudentSpaceShips.Any(p => p.SpaceShipId == spaceShip.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }
            studentGameInfo.StudentSpaceShips.Add(new StudentSpaceShip
            {
                SpaceShipId = spaceShip.Id,
                IsActive = false,
                Level = 1,
            });
            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                var updateTokenResult = await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel
                {
                    StudentId = student!.Id,
                    NumberOfToken = -spaceShip.Price
                });

                if (!updateTokenResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(updateTokenResult.Error);
                    return methodResult;
                }

                studentGameInfo = _studentGameInfoRepository.Update(studentGameInfo);
                await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
