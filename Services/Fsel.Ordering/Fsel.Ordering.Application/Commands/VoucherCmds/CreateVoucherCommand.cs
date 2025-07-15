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
    using StringHelper = Shared.Helpers.StringHelper;

    public class CreateVoucherCommand : CreateVoucherCommandModel, IRequest<MethodResult<VoucherModel>>
    {
    }

    public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, MethodResult<VoucherModel>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;
        private readonly IEventRepository _eventRepository;

        public CreateVoucherCommandHandler(IVoucherRepository voucherRepository, IPackageRepository packageRepository, IMapper mapper, IEventRepository eventRepository)
        {
            _voucherRepository = voucherRepository;
            _packageRepository = packageRepository;
            _mapper = mapper;
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<VoucherModel>> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VoucherModel> methodResult = new MethodResult<VoucherModel>();
            if (request.Source != EnumVoucherSource.Admin && request.Source != EnumVoucherSource.Auto)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Source), request.Source);
                return methodResult;
            }
            if (request.Source == EnumVoucherSource.Auto && string.IsNullOrEmpty(request.CodePrefix))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.CodePrefix));
                return methodResult;
            }
            if (request.Source == EnumVoucherSource.Auto && await _voucherRepository.Queryable.AnyAsync(p => p.CodePrefix.ToLower() == request.CodePrefix.ToLower(), cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.CodePrefixAlreadyExists), nameof(request.CodePrefix));
                return methodResult;
            }
            if (request.Source == EnumVoucherSource.Auto && !string.IsNullOrEmpty(request.CodePrefix) && (request.CodePrefix.Length > 5 || !StringHelper.ContainsWhitespaceOrSpecialChars(request.CodePrefix)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.CodeInValidFormat), nameof(request.CodePrefix));
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.Banner))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.Banner), request.Banner);
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
            if (request.ApplicableSubjects.Any(p => p == EnumApplicableSubjectsVoucher.Other) && (request.File == null || request.File.Length == 0 || string.IsNullOrEmpty(request.ExcelFilePath)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.File), request.File);
                return methodResult;
            }
            var emails = new List<string>();
            if (request.ApplicableSubjects.Any(p => p == EnumApplicableSubjectsVoucher.Other))
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
            if (request.Source == EnumVoucherSource.Auto && !request.NumberOfChanges.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.NumberOfChanges), request.NumberOfChanges);
                return methodResult;
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
            if (request.Source == EnumVoucherSource.Admin && (string.IsNullOrEmpty(request.Code) || !StringHelper.ContainsWhitespaceOrSpecialChars(request.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.CodeInValidFormat), nameof(request.Code));
                return methodResult;
            }
            if (request.Source == EnumVoucherSource.Admin && await _voucherRepository.Queryable.AnyAsync(p => p.Code.ToLower() == request.Code.ToLower(), cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.CodeAlreadyExists), nameof(request.Code));
                return methodResult;
            }

            Voucher voucher = _mapper.Map<Voucher>(request);
            voucher.Source = request.Source;
            if (request.ApplicableSubjects.Any(p => p == EnumApplicableSubjectsVoucher.Other))
            {
                voucher.ApplicableEmails = emails;
            }
            request.PackageIds.ForEach(p => voucher.VoucherPackages.Add(new VoucherPackage()
            {
                PackageId = p
            }));
            if (request.EventIds == null || request.EventIds.Count == 0)
            {
                var eventDefault = await _eventRepository.Queryable.FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
                if (eventDefault != null)
                {
                    voucher.EventIds = new List<Guid>() { eventDefault.Id };
                }
            }

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.Source == EnumVoucherSource.Admin)
                {
                    if (!voucher.IsValid())
                    {
                        methodResult.AddErrorBadRequest(voucher.ErrorMessages);
                        return methodResult;
                    }
                    voucher = _voucherRepository.Add(voucher);
                }
                else
                {
                    var vouchers = new List<Voucher>();
                    var codes = new List<string>();
                    while (true)
                    {
                        string code;
                        do
                        {
                            if (string.IsNullOrEmpty(request.CodePrefix))
                            {
                                code = NumberHelper.GenerateCode(10);
                            }
                            else
                            {
                                code = request.CodePrefix + NumberHelper.GenerateCode(5);
                            }
                        } while (await _voucherRepository.Queryable.AnyAsync(p => p.Code.ToLower() == code.ToLower(), cancellationToken));

                        if (!codes.Contains(code))
                        {
                            codes.Add(code);
                            var newVoucher = voucher.Clone();
                            newVoucher.Id = Guid.NewGuid();
                            newVoucher.Code = code;
                            newVoucher.Quantity = 1;
                            newVoucher.VoucherPackages = new List<VoucherPackage>();
                            request.PackageIds.ForEach(p => newVoucher.VoucherPackages.Add(new VoucherPackage()
                            {
                                PackageId = p
                            }));
                            vouchers.Add(newVoucher);
                        }

                        if (codes.Count == request.Quantity)
                        {
                            break;
                        }
                    }

                    var voucherEntity = vouchers.First();
                    if (!voucherEntity.IsValid())
                    {
                        methodResult.AddErrorBadRequest(voucher.ErrorMessages);
                        return methodResult;
                    }
                    await _voucherRepository.AddList(vouchers);
                }

                await _voucherRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<VoucherModel>(voucher);
                return methodResult;
            });

            return methodResult;
        }
    }
}
