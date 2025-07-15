namespace Fsel.Identity.Application.Commands.PermissionCmd
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

    public class SavePermissionCommand : SavePermissionCommandModel, IRequest<MethodResult<PermissionModel>>
    {
    }

    public class SavePermissionCommandHandler : IRequestHandler<SavePermissionCommand, MethodResult<PermissionModel>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IMapper _mapper;
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly IMediator _mediator;

        public SavePermissionCommandHandler(IPermissionRepository permissionRepository, IMapper mapper, IPermissionGroupRepository permissionGroupRepository, IMediator mediator)
        {
            _permissionRepository = permissionRepository;
            _mapper = mapper;
            _permissionGroupRepository = permissionGroupRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<PermissionModel>> Handle(SavePermissionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PermissionModel>();

            if (string.IsNullOrEmpty(request.ClaimValue) || string.IsNullOrEmpty(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
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

        private async Task Create(SavePermissionCommandModel request, MethodResult<PermissionModel> methodResult, CancellationToken cancellationToken)
        {
            if (!StringHelper.ContainsWhitespaceOrSpecialChars(request.ClaimValue ?? string.Empty))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return;
            }

            var permissionGroup = await _permissionGroupRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.PermissionGroupId, cancellationToken);
            if (permissionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var claimValue = $"{permissionGroup.ClaimType}.{request.ClaimValue}";

            if (await _permissionRepository.Queryable.AnyAsync(p => p.ClaimValue == claimValue, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return;
            }

            var permission = _mapper.Map<Permission>(request);
            permission.ClaimValue = claimValue;
            if (!permission.IsValid())
            {
                methodResult.AddError(permission.ErrorMessages);
                return;
            }

            await _permissionRepository.ExecuteTransactionAsync(async () =>
            {
                permission = _permissionRepository.Add(permission);
                await _permissionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PermissionModel>(permission);
                return methodResult;
            });
        }

        private async Task Update(SavePermissionCommandModel request, MethodResult<PermissionModel> methodResult, CancellationToken cancellationToken)
        {
            if (await _permissionRepository.Queryable.AnyAsync(p => p.ClaimValue == request.ClaimValue && p.Id != request.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return;
            }

            var permission = await _permissionRepository.GetByIdAsync(request.Id!.Value);
            if (permission == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return;
            }

            await _permissionRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Status != permission.Status)
                {
                    var result = await _mediator.Send(new ChangeStatusPermissionCommand() { Id = permission.Id }, cancellationToken);
                    if (!result.IsOK)
                    {
                        methodResult.AddError(result.ErrorMessages);
                        return methodResult;
                    }
                }

                permission.Name = request.Name;
                permission.Description = request.Description;
                permission.Status = request.Status;

                if (!permission.IsValid())
                {
                    methodResult.AddError(permission.ErrorMessages);
                    return methodResult;
                }

                permission = _permissionRepository.Update(permission);
                await _permissionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<PermissionModel>(permission);
                return methodResult;
            });
        }
    }
}
