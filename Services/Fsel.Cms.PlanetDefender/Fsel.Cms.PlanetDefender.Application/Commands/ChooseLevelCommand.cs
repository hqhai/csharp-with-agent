// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.StudentGameInfos;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ChooseLevelCommand : ChooseLevelCommandModel, IRequest<MethodResult<StudentGameInfoModel>>
    {
    }

    public class ChooseLevelCommandHandler : IRequestHandler<ChooseLevelCommand, MethodResult<StudentGameInfoModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public ChooseLevelCommandHandler(IMapper mapper, IStudentGameInfoRepository studentGameInfoRepository, IUserService userService, AuthContext authContext)
        {
            _mapper = mapper;
            _studentGameInfoRepository = studentGameInfoRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(ChooseLevelCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameInfoModel> methodResult = new MethodResult<StudentGameInfoModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            var chooseLevel = await _studentGameInfoRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student!.Id, cancellationToken);

            if (chooseLevel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(chooseLevel));
                return methodResult;
            }
            _mapper.Map(request, chooseLevel);

            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                chooseLevel = _studentGameInfoRepository.Update(chooseLevel);

                await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentGameInfoModel>(chooseLevel);
                return methodResult;
            });

            return methodResult;
        }
    }
}
