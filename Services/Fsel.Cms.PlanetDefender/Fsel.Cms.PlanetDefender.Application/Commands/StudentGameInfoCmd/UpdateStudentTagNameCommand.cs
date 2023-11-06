// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.StudentGameInfoCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameInfos;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Infrastructure.Repositories;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentTagNameCommand : UpdateStudentTagNameCommandModel, IRequest<MethodResult<StudentGameInfoModel>>
    {
    }

    public class UpdateStudentTagNameCommandHandler : IRequestHandler<UpdateStudentTagNameCommand, MethodResult<StudentGameInfoModel>>
    {
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IStudentTagNameRepository _studentTagRepository;

        public UpdateStudentTagNameCommandHandler(IStudentGameInfoRepository studentGameInfoRepository, IMapper mapper, IUserService userService, AuthContext authContext, IStudentTagNameRepository studentTagRepository)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
            _studentTagRepository = studentTagRepository;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(UpdateStudentTagNameCommand request, CancellationToken cancellationToken)
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
            if (!await _studentTagRepository.AnyAsync(request.TagNameId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.TagNameId));
                return methodResult;
            }

            var studentTagName = await _studentGameInfoRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id, cancellationToken);
            if (studentTagName == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentTagName));
                return methodResult;
            }

            _mapper.Map(request, studentTagName);

            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                studentTagName = _studentGameInfoRepository.Update(studentTagName);

                await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentGameInfoModel>(studentTagName);
                return methodResult;
            });

            return methodResult;
        }
    }
}
