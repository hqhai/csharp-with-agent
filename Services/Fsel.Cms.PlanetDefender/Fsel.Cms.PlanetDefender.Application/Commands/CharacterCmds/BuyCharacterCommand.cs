// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.CharacterCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class BuyCharacterCommand : IRequest<MethodResult<bool>>
    {
        public Guid CharacterId { get; set; }
    }
    public class BuyCharacterCommandHandler : IRequestHandler<BuyCharacterCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly AuthContext _authContext;
        public BuyCharacterCommandHandler(IUserService userService, ICharacterRepository characterRepository, IStudentGameInfoRepository studentGameInfoRepository, AuthContext authContext)
        {
            _userService = userService;
            _characterRepository = characterRepository;
            _studentGameInfoRepository = studentGameInfoRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(BuyCharacterCommand request, CancellationToken cancellationToken)
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


            var character = await _characterRepository.GetByIdAsync(request.CharacterId);
            if (character == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (character.Price > student?.NumberOfToken)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Min));
                return methodResult;
            }

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Include(p => p.StudentCharacters).FirstOrDefaultAsync(x => x.StudentId == student!.Id, cancellationToken);

            if (studentGameInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (studentGameInfo.StudentCharacters.Any(p => p.CharacterId == character.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }
            studentGameInfo.StudentCharacters.Add(new StudentCharacter
            {
                CharacterId = character.Id,
                IsActive = false
            });
            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                var updateTokenResult = await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel
                {
                    StudentId = student!.Id,
                    NumberOfToken = -character.Price
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
