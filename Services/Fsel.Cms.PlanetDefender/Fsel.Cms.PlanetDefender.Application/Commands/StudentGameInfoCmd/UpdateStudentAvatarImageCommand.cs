// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.StudentGameInfoCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameInfos;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentAvatarImageCommand : UpdateStudentAvatarImageCommandModel, IRequest<MethodResult<StudentGameInfoModel>>
    {
    }
    public class UpdateStudentAvatarImageCommandHandler : IRequestHandler<UpdateStudentAvatarImageCommand, MethodResult<StudentGameInfoModel>>
    {
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IAvatarImageRepository _avatarImageRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public UpdateStudentAvatarImageCommandHandler(IStudentGameInfoRepository studentGameInfoRepository, IAvatarImageRepository avatarImageRepository, IMapper mapper, IUserService userService, AuthContext authContext)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _avatarImageRepository = avatarImageRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(UpdateStudentAvatarImageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameInfoModel> methodResult = new MethodResult<StudentGameInfoModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            if (!await _avatarImageRepository.AnyAsync(request.AvatarImageId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.AvatarImageId));
                return methodResult;
            }

            var studentAvatarImage = await _studentGameInfoRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id, cancellationToken);
            if (studentAvatarImage == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentAvatarImage));
                return methodResult;
            }

            _mapper.Map(request, studentAvatarImage);

            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                studentAvatarImage = _studentGameInfoRepository.Update(studentAvatarImage);

                await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentGameInfoModel>(studentAvatarImage);
                return methodResult;
            });

            return methodResult;
        }
    }
}
