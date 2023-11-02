// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.StudentGameAvatarCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameAvatars;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentGameAvatarCommand : UpdateStudentGameAvatarCommandModel, IRequest<MethodResult<StudentGameAvatarModel>>
    {
    }

    public class UpdateStudentGameAvatarCommandHandler : IRequestHandler<UpdateStudentGameAvatarCommand, MethodResult<StudentGameAvatarModel>>
    {
        private readonly IStudentGameAvatarRepository _studentGameAvatarRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public UpdateStudentGameAvatarCommandHandler(IStudentGameAvatarRepository studentGameAvatarRepository, IMapper mapper, IUserService userService, AuthContext authContext)
        {
            _studentGameAvatarRepository = studentGameAvatarRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentGameAvatarModel>> Handle(UpdateStudentGameAvatarCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameAvatarModel> methodResult = new MethodResult<StudentGameAvatarModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult.Content?.Result?.Id;

            var studentGameAvatar = await _studentGameAvatarRepository.Queryable.FirstOrDefaultAsync(x => x.StudentGameInfoId == studentId, cancellationToken);

            if (studentGameAvatar == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentGameAvatar));
                return methodResult;
            }
            _mapper.Map(request, studentGameAvatar);

            await _studentGameAvatarRepository.ExecuteTransactionAsync(async () =>
            {
                studentGameAvatar.IsActive = true;
                studentGameAvatar = _studentGameAvatarRepository.Update(studentGameAvatar);

                await _studentGameAvatarRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentGameAvatarModel>(studentGameAvatar);
                return methodResult;
            });

            return methodResult;
        }
    }
}
