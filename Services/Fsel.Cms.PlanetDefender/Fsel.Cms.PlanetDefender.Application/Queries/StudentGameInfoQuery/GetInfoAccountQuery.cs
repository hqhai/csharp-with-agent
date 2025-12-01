// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetInfoAccountQuery : IRequest<MethodResult<AccountModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetInfoAccountQueryHandler : IRequestHandler<GetInfoAccountQuery, MethodResult<AccountModel>>
    {
        private readonly IUserService _userService;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;

        public GetInfoAccountQueryHandler(IUserService userService, IStudentGameInfoRepository studentGameInfoRepository)
        {
            _userService = userService;
            _studentGameInfoRepository = studentGameInfoRepository;
        }

        public async Task<MethodResult<AccountModel>> Handle(GetInfoAccountQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AccountModel>();

            var userResult = await _userService.GetInfoStudentOrGuest(request.StudentId);
            if (!userResult.IsSuccessStatusCode)
            {
                methodResult.AddError(userResult.Error);
                return methodResult;
            }
            var user = userResult.Content?.Result;

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Include(p => p.GameHistories).Include(n => n.StudentSpaceShips).FirstOrDefaultAsync(x => x.StudentId == request.StudentId, cancellationToken);
            if (studentGameInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var accountModel = new AccountModel()
            {
                Id = user.Id,
                AvatarPath = user.User?.AvatarPath,
                Name = user.User?.FullName,
                Code = user.User?.Code,
                UserName = string.IsNullOrEmpty(user.User?.Email) ? user.User?.FullName : user.User?.Email,
                AccountType = user.User?.Role,
                GameLevel = studentGameInfo.CourseLevel,
                Level = studentGameInfo.Level,
                CurrentRank = 100,
                Achievement = 1,
                TotalGamePlay = studentGameInfo.GameHistories.Count,
                CharacterOwned = 1,
                SpaceshipOwned = studentGameInfo.StudentSpaceShips.Count,
                Exp = 100,
                TotalExp = 1000,
                FSELCoin = 100000,
                GameHistory = studentGameInfo.GameHistories.Select(x => new GameHistoryModel
                {
                    RoundNumber = x.RoundNumber,
                    Score = x.Score,
                    CreatedDate = x.CreatedDate,
                    DestroyNumber = x.DestroyNumber,
                    ImpactNumber = x.ImpactNumber
                }).ToList(),
            };

            methodResult.Result = accountModel;
            return methodResult;
        }
    }
}
