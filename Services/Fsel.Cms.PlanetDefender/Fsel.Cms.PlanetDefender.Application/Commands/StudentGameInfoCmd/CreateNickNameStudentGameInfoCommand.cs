// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.StudentGameInfoCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameInfos;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateNickNameStudentGameInfoCommand : CreateNickNameStudentGameInfoCommandModel, IRequest<MethodResult<StudentGameInfoModel>>
    {
    }

    public class CreateNickNameStudentGameInfoCommandHandler : IRequestHandler<CreateNickNameStudentGameInfoCommand, MethodResult<StudentGameInfoModel>>
    {
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IMapper _mapper;

        public CreateNickNameStudentGameInfoCommandHandler(IStudentGameInfoRepository studentGameInfoRepository, IMapper mapper)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(CreateNickNameStudentGameInfoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameInfoModel> methodResult = new MethodResult<StudentGameInfoModel>();
            var studentGameInfo = await _studentGameInfoRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == request.StudentId, cancellationToken);

            var nickNameIsExist = await _studentGameInfoRepository.Queryable.AnyAsync(x => x.NickName == request.NickName && x.StudentId != request.StudentId, cancellationToken);
            if (nickNameIsExist)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(studentGameInfo));
                return methodResult;
            }

            #region Validation

            if (studentGameInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentGameInfo));
                return methodResult;
            }
            _mapper.Map(request, studentGameInfo);

            #endregion Validation

            await _studentGameInfoRepository.ExecuteTransactionAsync(async () =>
            {
                studentGameInfo = _studentGameInfoRepository.Update(studentGameInfo);

                await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentGameInfoModel>(studentGameInfo);
                return methodResult;
            });

            return methodResult;
        }
    }
}
