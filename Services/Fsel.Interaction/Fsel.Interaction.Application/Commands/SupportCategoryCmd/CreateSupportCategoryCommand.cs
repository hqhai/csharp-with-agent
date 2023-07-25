// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportCategoryCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportCategorys;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateSupportCategoryCommand : CreateSupportCategoryCommandModel, IRequest<MethodResult<SupportCategoryModel>>
    {
    }

    public class CreateSupportCategoryCommandHandler : IRequestHandler<CreateSupportCategoryCommand, MethodResult<SupportCategoryModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportCategoryRepository _supportCategoryRepository;

        public CreateSupportCategoryCommandHandler(IMapper mapper, ISupportCategoryRepository supportCategoryRepository)
        {
            _mapper = mapper;
            _supportCategoryRepository = supportCategoryRepository;
        }

        public async Task<MethodResult<SupportCategoryModel>> Handle(CreateSupportCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportCategoryModel> methodResult = new MethodResult<SupportCategoryModel>();

            SupportCategory supportCategory = _mapper.Map<SupportCategory>(request);
            await _supportCategoryRepository.ExecuteTransactionAsync(async () =>
            {
                supportCategory.IsActive = true;
                supportCategory = _supportCategoryRepository.Add(supportCategory);
                await _supportCategoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<SupportCategoryModel>(supportCategory);
                return methodResult;
            });

            return methodResult;
        }
    }
}
