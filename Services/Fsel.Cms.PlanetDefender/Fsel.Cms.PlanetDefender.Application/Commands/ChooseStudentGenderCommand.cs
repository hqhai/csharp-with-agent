// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.StudentGameInfos;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ChooseStudentGenderCommand : ChooseStudentGenderCommandModel, IRequest<MethodResult<StudentGameInfoModel>>
    {
    }

    public class ChooseStudentGenderCommandHandler : IRequestHandler<ChooseStudentGenderCommand, MethodResult<StudentGameInfoModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IAvatarImageRepository _avatarImageRepository;
        private readonly IStudentTagNameRepository _studentTagNameRepository;
        private readonly IStudentSpaceShipRepository _studentSpaceShipRepository;
        private readonly ISpaceShipRepository _spaceShipRepository;

        public ChooseStudentGenderCommandHandler(IMapper mapper, IStudentGameInfoRepository studentGameInfoRepository, IUserService userService, AuthContext authContext, IAvatarImageRepository avatarImageRepository, IStudentTagNameRepository studentTagNameRepository, IStudentSpaceShipRepository studentSpaceShipRepository, ISpaceShipRepository spaceShipRepository)
        {
            _mapper = mapper;
            _studentGameInfoRepository = studentGameInfoRepository;
            _userService = userService;
            _authContext = authContext;
            _avatarImageRepository = avatarImageRepository;
            _studentTagNameRepository = studentTagNameRepository;
            _studentSpaceShipRepository = studentSpaceShipRepository;
            _spaceShipRepository = spaceShipRepository;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(ChooseStudentGenderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameInfoModel> methodResult = new MethodResult<StudentGameInfoModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;
            StudentGameInfo studentGameInfo = _mapper.Map<StudentGameInfo>(request);

            var avatarId = await _avatarImageRepository.Queryable.OrderBy(x => x.Level).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            var studentTagNameId = await _studentTagNameRepository.Queryable.OrderBy(x => x.Level).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            var spaceShip = await _spaceShipRepository.Queryable.Where(x => x.IsDefault).FirstOrDefaultAsync(cancellationToken);

            await _studentSpaceShipRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                studentGameInfo.StudentSpaceShips = new List<StudentSpaceShip>()
                {
                    new StudentSpaceShip
                    {
                        IsActive = true,
                        StudentGameInfoId = studentGameInfo.Id,
                        SpaceShipId = spaceShip!.Id,
                    }
                };
                studentGameInfo.AvatarImageId = avatarId;
                studentGameInfo.TagNameId = studentTagNameId;
                studentGameInfo.StudentId = student!.Id;
                studentGameInfo = _studentGameInfoRepository.Add(studentGameInfo);
                await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentGameInfoModel>(studentGameInfo);
                return methodResult;
            });

            return methodResult;
        }
    }
}
