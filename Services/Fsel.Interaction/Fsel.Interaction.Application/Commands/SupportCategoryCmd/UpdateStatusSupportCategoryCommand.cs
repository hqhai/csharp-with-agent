// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportCategoryCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportCategorys;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusSupportCategoryCommand : UpdateStatusSupportCategoryCommandModel, IRequest<MethodResult<SupportCategoryModel>>
    {
    }

    public class UpdateStatusSupportCategoryCommandHandler : IRequestHandler<UpdateStatusSupportCategoryCommand, MethodResult<SupportCategoryModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportCategoryRepository _supportCategoryRepository;

        public UpdateStatusSupportCategoryCommandHandler(IMapper mapper, ISupportCategoryRepository supportCategoryRepository)
        {
            _mapper = mapper;
            _supportCategoryRepository = supportCategoryRepository;
        }

        public async Task<MethodResult<SupportCategoryModel>> Handle(UpdateStatusSupportCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportCategoryModel> methodResult = new MethodResult<SupportCategoryModel>();
            var supportCategory = await _supportCategoryRepository.GetByIdAsync(request.Id);

            #region Validation

            if (supportCategory == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionNotExist));
                return methodResult;
            }
            _mapper.Map(request, supportCategory);

            #endregion Validation

            await _supportCategoryRepository.ExecuteTransactionAsync(async () =>
            {
                supportCategory = _supportCategoryRepository.Update(supportCategory);

                await _supportCategoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SupportCategoryModel>(supportCategory);
                return methodResult;
            });

            return methodResult;
        }
    }
}
