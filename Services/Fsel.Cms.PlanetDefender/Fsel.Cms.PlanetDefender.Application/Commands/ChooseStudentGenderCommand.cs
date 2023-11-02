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

    public class ChooseStudentGenderCommand : ChooseStudentGenderCommandModel, IRequest<MethodResult<StudentGameInfoModel>>
    {
    }

    public class ChooseStudentGenderCommandHandler : IRequestHandler<ChooseStudentGenderCommand, MethodResult<StudentGameInfoModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public ChooseStudentGenderCommandHandler(IMapper mapper, IStudentGameInfoRepository studentGameInfoRepository, IUserService userService, AuthContext authContext)
        {
            _mapper = mapper;
            _studentGameInfoRepository = studentGameInfoRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(ChooseStudentGenderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameInfoModel> methodResult = new MethodResult<StudentGameInfoModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;
            StudentGameInfo studentGameInfo = _mapper.Map<StudentGameInfo>(request);

            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                studentGameInfo.StudentGameAvatars = new List<StudentGameAvatar>()
                {
                    new StudentGameAvatar
                    {
                        IsActive = true,
                        AvatarImageId = studentGameInfo.StudentGameAvatars.Select(x => x.AvatarImage).OrderByDescending(x => x.Level).Select(x => x.Id).FirstOrDefault(),
                        StudentGameInfoId = studentGameInfo.Id
                    }
                };

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
