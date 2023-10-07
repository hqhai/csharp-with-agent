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

    public class ChooseCharacterGenderCommand : ChooseCharacterGenderCommandModel, IRequest<MethodResult<StudentGameInfoModel>>
    {
    }

    public class ChooseCharacterGenderCommandHandler : IRequestHandler<ChooseCharacterGenderCommand, MethodResult<StudentGameInfoModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public ChooseCharacterGenderCommandHandler(IMapper mapper, IStudentGameInfoRepository studentGameInfoRepository, IUserService userService, AuthContext authContext)
        {
            _mapper = mapper;
            _studentGameInfoRepository = studentGameInfoRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(ChooseCharacterGenderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameInfoModel> methodResult = new MethodResult<StudentGameInfoModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            StudentGameInfo chooseCharacter = _mapper.Map<StudentGameInfo>(request);
            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                chooseCharacter.StudentId = student!.Id;
                chooseCharacter = _studentGameInfoRepository.Add(chooseCharacter);
                await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentGameInfoModel>(chooseCharacter);
                return methodResult;
            });

            return methodResult;
        }
    }
}
