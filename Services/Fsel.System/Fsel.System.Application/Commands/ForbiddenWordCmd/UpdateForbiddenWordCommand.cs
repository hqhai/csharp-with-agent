// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ForbiddenWordCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ForbiddenWords;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Globalization;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateForbiddenWordCommand : UpdateForbiddenWordCommandModel, IRequest<MethodResult<ForbiddenWordModel>>
    {
    }

    public class UpdateForbiddenWordCommandHandler : IRequestHandler<UpdateForbiddenWordCommand, MethodResult<ForbiddenWordModel>>
    {
        private readonly IMapper _mapper;
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public UpdateForbiddenWordCommandHandler(IMapper mapper, IForbiddenWordRepository forbiddenWordRepository)
        {
            _mapper = mapper;
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<ForbiddenWordModel>> Handle(UpdateForbiddenWordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ForbiddenWordModel> methodResult = new MethodResult<ForbiddenWordModel>();
            var forbiddenWord = await _forbiddenWordRepository.GetByIdAsync(request.Id);

            #region Validation
            string compareWord = request!.Word!.ToLower(CultureInfo.InvariantCulture);

            var forbiddenWordName = await _forbiddenWordRepository.Queryable.AnyAsync(x => x!.Word!.ToLower() == compareWord, cancellationToken);

            if (forbiddenWordName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(forbiddenWordName));
                return methodResult;
            }
            if (forbiddenWord == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(forbiddenWord));
                return methodResult;
            }
            _mapper.Map(request, forbiddenWord);

            #endregion Validation

            await _forbiddenWordRepository.ExecuteTransactionAsync(async () =>
            {
                forbiddenWord = _forbiddenWordRepository.Update(forbiddenWord);

                await _forbiddenWordRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ForbiddenWordModel>(forbiddenWord);
                return methodResult;
            });

            return methodResult;
        }
    }
}
