// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.PackageCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SavePackagesCommand : SavePackagesCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SavePackagesCommandHandler : IRequestHandler<SavePackagesCommand, MethodResult<bool>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public SavePackagesCommandHandler(IPackageRepository packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(SavePackagesCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.Packages == null || request.Packages.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var packageIds = request.Packages.Select(x => x.Id).ToList();

            if (!_packageRepository.Queryable.Any(p => packageIds.Contains(p.Id)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var packagesUpdate = new List<Package>();
            var packagesCreate = new List<Package>();

            foreach (var item in request.Packages)
            {
                if (item.Id.HasValue)
                {
                    var package = await _packageRepository.GetByIdAsync(item.Id.Value);
                    if (package == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                        return methodResult;
                    }
                    _mapper.Map(item, package);
                    if (!package.IsValid())
                    {
                        methodResult.AddError(package.ErrorMessages);
                        return methodResult;
                    }
                    packagesUpdate.Add(package);
                }
                else
                {
                    var package = _mapper.Map<Package>(item);
                    package.Code = EnumPackageCode.BASIC;
                    package.IncentivesWhenPurchasing = item.Name;
                    if (!package.IsValid())
                    {
                        methodResult.AddError(package.ErrorMessages);
                        return methodResult;
                    }
                    packagesCreate.Add(package);
                }
            }

            await _packageRepository.ExecuteTransactionAsync(async () =>
            {
                _packageRepository.UpdateList(packagesUpdate);
                await _packageRepository.AddList(packagesCreate);
                await _packageRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
