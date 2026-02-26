// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.ExamPractice.Application.Services.SystemServices;
using Fsel.ExamPractice.Application.Services.SystemServices.QueryModels;
using Fsel.ExamPractice.Domain.Enums;
using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
using Fsel.ExamPractice.Domain.IRepositories;
using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
using Fsel.ExamPractice.Infrastructure.Common;
using Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers;
using Fsel.Shared.Enums.ErrorCodes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.ExamPractice.Application.Commands.ExamPracticeCmd.V1i1
{
    public class CreateExamPracticeCommand : UpdateExamPracticeCommandModel, IRequest<MethodResult<ExamPracticeModel>>
    {
    }

    public class CreateExamPracticeCommandHandler : IRequestHandler<CreateExamPracticeCommand, MethodResult<ExamPracticeModel>>
    {
        private readonly IExamPracticeRepository _repo;
        private readonly IMapper _mapper;
        private readonly ExamPracticeHelper _helper;
        private readonly ISystemService _systemService;

        public CreateExamPracticeCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            IMapper mapper,
            ExamPracticeHelper examPracticeHelper,
            ISystemService systemService)
        {
            _repo = examPracticeRepository;
            _mapper = mapper;
            _helper = examPracticeHelper;
            _systemService = systemService;
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(CreateExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = new MethodResult<ExamPracticeModel>();

            if (!await ValidateCodeAsync(request, result, cancellationToken))
            {
                return result;
            }
            if (!ValidateTypeAndSubType(request, result))
            {
                return result;
            }
            if (!ValidateQuestions(request, result))
            {
                return result;
            }
            var examPractice = BuildEntity(request);
            if (!ValidateEntity(examPractice, result))
            {
                return result;
            }
            if (!await TryResolveProvinceAsync(request, examPractice, result))
            {
                return result;
            }
            if (!ValidateExamPracticeRulesAndPublish(request, examPractice, result))
            {
                return result;
            }
            await PersistAsync(examPractice, result, cancellationToken);
            return result;
        }

        // -------------------- Steps --------------------

        private async Task<bool> ValidateCodeAsync(CreateExamPracticeCommand request, MethodResult<ExamPracticeModel> result, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Code), request.Code);
                return false;
            }

            var existCode = await _repo.Queryable.AnyAsync(x => x.Code == request.Code, ct);
            if (existCode)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return false;
            }

            return true;
        }

        private static bool ValidateTypeAndSubType(CreateExamPracticeCommand request, MethodResult<ExamPracticeModel> result)
        {
            if (!request.Type.GetSubTypes().Contains(request.SubType))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.SubType);
                return false;
            }
            return true;
        }

        private bool ValidateQuestions(CreateExamPracticeCommand request, MethodResult<ExamPracticeModel> result)
        {
            var validate = ExamPracticeValidateBuilder
                .Create(request, _repo)
                .IsValidateQuestion(request.ExamPracticeSections, _mapper)
                .GetResult();

            if (validate.IsOK)
            {
                return true;
            }
            result.AddErrorBadRequest(validate.ErrorMessages);
            return false;
        }

        private Domain.Entities.ExamPractice BuildEntity(CreateExamPracticeCommand request)
        {
            // originalId: bạn đang set Guid.NewGuid(); giữ đúng behavior
            return ExamPracticeFactory.Create(request, _mapper).Build(version: 0, originalId: Guid.NewGuid());
        }

        private static bool ValidateEntity(Domain.Entities.ExamPractice entity, MethodResult<ExamPracticeModel> result)
        {
            if (entity.IsValid())
            {
                return true;
            }
            result.AddErrorBadRequest(entity.ErrorMessages);
            return false;
        }

        private async Task<bool> TryResolveProvinceAsync(
            CreateExamPracticeCommand request,
            Domain.Entities.ExamPractice entity,
            MethodResult<ExamPracticeModel> result)
        {
            if (!request.ProvinceId.HasValue)
            {
                return true;
            }

            var locationResults = await _systemService.GetLocationByIdsAsync(
                new GetLocationsByIdsQueryModel { IdsStr = request.ProvinceId.Value.ToString() });

            if (!locationResults.IsSuccessStatusCode)
            {
                result.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError));
                return false;
            }

            var locations = locationResults.Content?.Result;
            if (locations == null || !locations.Any())
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(locations), request.ProvinceId);
                return false;
            }

            entity.Province = locations.First().Name;
            return true;
        }

        private bool ValidateExamPracticeRulesAndPublish(
            CreateExamPracticeCommand request,
            Domain.Entities.ExamPractice entity,
            MethodResult<ExamPracticeModel> result)
        {
            if (request.Type != EnumExamPracticeType.ExamPractice)
            {
                return true;
            }

            // date range rule
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate >= request.EndDate)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.StartDate), nameof(request.EndDate));
                return false;
            }

            // province allowed only for specific subtype
            if (request.ProvinceId.HasValue &&
                request.SubType is not (EnumExamPracticeSubType.Practice or EnumExamPracticeSubType.HighschoolEntrance))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ProvinceId), request.SubType);
                return false;
            }

            // school grade must be empty when not Practice
            if (request.SubType != EnumExamPracticeSubType.Practice && !string.IsNullOrEmpty(request.SchoolGrade))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.SchoolGrade), request.SubType);
                return false;
            }

            // publish rule
            if (!request.IsDraft)
            {
                var leafSections = _helper.GetLeafSections(request.ExamPracticeSections);
                if (!_helper.AllSectionsHaveAtLeastOneQuestion(leafSections))
                {
                    result.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.MissingRequiredData),
                        nameof(entity.Status),
                        EnumExamPracticeStatus.Active);
                    return false;
                }

                if (entity.Status == EnumExamPracticeStatus.Draft)
                {
                    entity.Status = EnumExamPracticeStatus.Active;
                    entity.ActivatedAt = DateTime.UtcNow;
                }
            }

            return true;
        }

        private async Task PersistAsync(
            Domain.Entities.ExamPractice entity,
            MethodResult<ExamPracticeModel> result,
            CancellationToken ct)
        {
            await _repo.ExecuteTransactionAsync(async () =>
            {
                _repo.Add(entity);
                await _repo.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                result.Result = _mapper.Map<ExamPracticeModel>(entity);
                return result;
            });
        }
    }
}
