// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateVoucherCommand : UpdateVoucherCommandModel, IRequest<MethodResult<VoucherModel>>
    {
    }

    public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, MethodResult<VoucherModel>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;
        private readonly IEventRepository _eventRepository;

        public UpdateVoucherCommandHandler(IVoucherRepository voucherRepository, IPackageRepository packageRepository, IMapper mapper, IEventRepository eventRepository)
        {
            _voucherRepository = voucherRepository;
            _packageRepository = packageRepository;
            _mapper = mapper;
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<VoucherModel>> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VoucherModel> methodResult = new MethodResult<VoucherModel>();

            #region Validation

            var voucher = await _voucherRepository.Queryable.Include(p => p.VoucherPackages).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (voucher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(voucher));
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Banner) || string.IsNullOrEmpty(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }
            if ((string.IsNullOrEmpty(request.Code) || !Shared.Helpers.StringHelper.ContainsWhitespaceOrSpecialChars(request.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.Code));
                return methodResult;
            }
            if (request.Translations == null || request.Translations.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.Translations), request.Translations);
                return methodResult;
            }
            if (request.Category == EnumVoucherCategory.Percent && request.Value > 100)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.ValueGreaterThan100), nameof(request.Category), request.Category);
                return methodResult;
            }
            if (request.ApplicableSubjects == null || request.ApplicableSubjects.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.ApplicableSubjects), request.ApplicableSubjects);
                return methodResult;
            }
            if (request.ApplicableSubjects.Any(p => p == EnumApplicableSubjectsVoucher.Other) && request.ExcelFilePath.ToLower() != voucher.ExcelFilePath.ToLower() && (request.File == null || request.File.Length == 0 || string.IsNullOrEmpty(request.ExcelFilePath)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.File), request.File);
                return methodResult;
            }
            var emails = new List<string>();
            if (request.ApplicableSubjects.Any(p => p == EnumApplicableSubjectsVoucher.Other) && request.ExcelFilePath.ToLower() != voucher.ExcelFilePath.ToLower())
            {
                var result = request.File?.ImportAndValidateExcel(async (ImportEmailsInToVoucherModel x, IList<ImportEmailsInToVoucherModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
                {
                    if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or invalid" });
                    }
                    return await Task.FromResult(errors.Count == 0);
                });
                emails = result?.Datas.Select(p => p.Email!).ToList();
                if (emails == null || emails.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.File), request.File);
                    return methodResult;
                }
            }
            if (request.StartDate > request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherStartTimeMustSoonerThanEndTime), nameof(request.EndDate), request.EndDate);
                return methodResult;
            }
            if (request.PackageIds == null || request.PackageIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.PackageIds));
                return methodResult;
            }
            if (_packageRepository.IsIdsInValid(request.PackageIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.PackageIds));
                return methodResult;
            }
            if (request.EventIds != null && request.EventIds.Count > 0 && _eventRepository.IsIdsInValid(request.EventIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.EventIds));
                return methodResult;
            }

            #endregion Validation

            if (request.ApplicableSubjects.Any(p => p == EnumApplicableSubjectsVoucher.Other))
            {
                voucher.ApplicableEmails = emails;
            }
            if (request.EventIds == null || request.EventIds.Count == 0)
            {
                var eventDefault = await _eventRepository.Queryable.FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
                if (eventDefault != null)
                {
                    voucher.EventIds = new List<Guid>() { eventDefault.Id };
                }
            }

            var currentPackageIds = voucher.VoucherPackages.Select(p => p.PackageId).ToList();
            var packageIds = request.PackageIds.Where(p => !currentPackageIds.Contains(p)).ToList();

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                _mapper.Map(request, voucher);

                foreach (var item in voucher.VoucherPackages.ToList())
                {
                    if (!request.PackageIds.Contains(item.PackageId))
                    {
                        voucher.VoucherPackages.Remove(item);
                    };
                }

                foreach (var item in packageIds)
                {
                    voucher.VoucherPackages.Add(new VoucherPackage()
                    {
                        PackageId = item
                    });
                }

                if (!voucher.IsValid())
                {
                    methodResult.AddError(voucher.ErrorMessages);
                    return methodResult;
                }

                voucher = _voucherRepository.Update(voucher);
                await _voucherRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<VoucherModel>(voucher);
                return methodResult;
            });

            return methodResult;
        }
    }
}
