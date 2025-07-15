namespace Fsel.Identity.Application.Commands.PermissionGroupCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Permissions;
    using Fsel.Identity.Domain.Models.EntityModels.Permissions;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SavePermissionGroupCommand : SavePermissionGroupCommandModel, IRequest<MethodResult<PermissionGroupModel>>
    {
    }

    public class SavePermissionGroupCommandHandler : IRequestHandler<SavePermissionGroupCommand, MethodResult<PermissionGroupModel>>
    {
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public SavePermissionGroupCommandHandler(IPermissionGroupRepository permissionGroupRepository, IMapper mapper, IMediator mediator)
        {
            _permissionGroupRepository = permissionGroupRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MethodResult<PermissionGroupModel>> Handle(SavePermissionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PermissionGroupModel>();

            if (string.IsNullOrEmpty(request.ClaimType) || string.IsNullOrEmpty(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (!StringHelper.ContainsWhitespaceOrSpecialChars(request.ClaimType))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (!request.Id.HasValue)
            {
                await Create(request, methodResult, cancellationToken);
            }
            else
            {
                await Update(request, methodResult, cancellationToken);
            }

            return methodResult;
        }

        private async Task Create(SavePermissionGroupCommandModel request, MethodResult<PermissionGroupModel> methodResult, CancellationToken cancellationToken)
        {
            if (await _permissionGroupRepository.Queryable.AnyAsync(p => p.ClaimType == request.ClaimType, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return;
            }
            //if (request.MenuId.HasValue)
            //{
            //    if (await _permissionGroupRepository.Queryable.AnyAsync(p => p.MenuId == request.MenuId, cancellationToken))
            //    {
            //        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
            //        return;
            //    }
            //}

            var permissionGroup = _mapper.Map<PermissionGroup>(request);
            if (!permissionGroup.IsValid())
            {
                methodResult.AddError(permissionGroup.ErrorMessages);
                return;
            }

            await _permissionGroupRepository.ExecuteTransactionAsync(async () =>
            {
                permissionGroup = _permissionGroupRepository.Add(permissionGroup);
                await _permissionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PermissionGroupModel>(permissionGroup);
                return methodResult;
            });
        }

        private async Task Update(SavePermissionGroupCommandModel request, MethodResult<PermissionGroupModel> methodResult, CancellationToken cancellationToken)
        {
            if (await _permissionGroupRepository.Queryable.AnyAsync(p => p.ClaimType == request.ClaimType && p.Id != request.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return;
            }
            //if (request.MenuId.HasValue)
            //{
            //    if (await _permissionGroupRepository.Queryable.AnyAsync(p => p.MenuId == request.MenuId && p.Id != request.Id, cancellationToken))
            //    {
            //        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
            //        return;
            //    }
            //}

            var permissionGroup = await _permissionGroupRepository.GetByIdAsync(request.Id!.Value);
            if (permissionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return;
            }

            await _permissionGroupRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Status != permissionGroup.Status)
                {
                    var result = await _mediator.Send(new ChangeStatusPermissionGroupCommand() { Id = permissionGroup.Id }, cancellationToken);
                    if (!result.IsOK)
                    {
                        methodResult.AddError(result.ErrorMessages);
                        return methodResult;
                    }
                }
                _mapper.Map(request, permissionGroup);
                if (!permissionGroup.IsValid())
                {
                    methodResult.AddError(permissionGroup.ErrorMessages);
                    return methodResult;
                }
                permissionGroup = _permissionGroupRepository.Update(permissionGroup);
                await _permissionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<PermissionGroupModel>(permissionGroup);
                return methodResult;
            });
        }
    }
}